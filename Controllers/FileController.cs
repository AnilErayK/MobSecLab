using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MobSecLab.Models;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Http; // For session extensions
using System.Threading.Tasks; // For async/await
using System.Collections.Generic;

namespace MobSecLab.Controllers
{
    [Authorize]
    public class FileController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly MobSecLabContext _context;

        public FileController(IConfiguration configuration, IHttpClientFactory httpClientFactory, MobSecLabContext context)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UploadPost()
        {
            var file = Request.Form.Files.FirstOrDefault();
            if (file == null || file.Length == 0)
            {
                ViewBag.Error = "Lütfen bir dosya seçin.";
                return View("Upload");
            }

            var apiBaseUrl = _configuration["MobSF:ApiBaseUrl"];
            var apiKey = _configuration["MobSF:ApiKey"];

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                var fileBytes = ms.ToArray();

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add("X-Mobsf-Api-Key", apiKey);

                using (var content = new MultipartFormDataContent())
                {
                    var fileContent = new ByteArrayContent(fileBytes);
                    fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                    content.Add(fileContent, "file", file.FileName);

                    var response = await client.PostAsync($"{apiBaseUrl}/upload", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var respStr = await response.Content.ReadAsStringAsync();
                        using (JsonDocument doc = JsonDocument.Parse(respStr))
                        {
                            var hash = doc.RootElement.GetProperty("hash").GetString();
                            // AnalysisId'yi Session'a kaydediyoruz
                            HttpContext.Session.SetString("AnalysisId", hash);
                            ViewBag.Message = "Dosya yüklendi. Taramayı başlatmak için aşağıdaki butona tıklayın.";
                            return View("Result");
                        }
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        ViewBag.Error = "Dosya yükleme başarısız: " + error;
                        return View("Upload");
                    }
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Scan()
        {
            var analysisId = HttpContext.Session.GetString("AnalysisId");
            if (string.IsNullOrEmpty(analysisId))
            {
                ViewBag.Error = "Analysis ID bulunamadı!";
                return View("Result");
            }

            var apiBaseUrl = _configuration["MobSF:ApiBaseUrl"];
            var apiKey = _configuration["MobSF:ApiKey"];

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("X-Mobsf-Api-Key", apiKey);

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("hash", analysisId)
            });

            // Tarama başlat
            var response = await client.PostAsync($"{apiBaseUrl}/scan", formContent);
            if (response.IsSuccessStatusCode)
            {
                ViewBag.Message = "Tarama başlatıldı. Lütfen bekleyin...";
                Thread.Sleep(60000); // bekleme, gerçek scenaryoda asenkron iyileştirilir

                // Bekledik, şimdi JSON sonuç dosyasını alalım:
                return await GetJsonResult(analysisId);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ViewBag.Error = "Tarama başlatılamadı: " + error;
                return View("Result");
            }
        }

        private async Task<IActionResult> GetJsonResult(string analysisId)
        {
            var apiBaseUrl = _configuration["MobSF:ApiBaseUrl"];
            var apiKey = _configuration["MobSF:ApiKey"];

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("X-Mobsf-Api-Key", apiKey);

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("hash", analysisId)
            });

            // Endpointi 'json_report' yerine 'report_json' olarak güncelliyoruz.
            var response = await client.PostAsync($"{apiBaseUrl}/report_json", formContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();
                using (var doc = JsonDocument.Parse(jsonStr))
                {
                    var fileMd5 = doc.RootElement.GetProperty("hash").GetString();

                    var username = User.Identity.Name;
                    var currentUser = _context.Users.FirstOrDefault(u => u.Username == username);
                    var userNo = currentUser.UserNo;

                    var fileName = HttpContext.Session.GetString("UploadedFileName");

                    var fileSeq = CalculateFileSeq(fileMd5);

                    var newFile = new FileEntity
                    {
                        UserNo = userNo,
                        FileSeq = fileSeq,
                        File_Name = fileName,
                        File_md5 = fileMd5
                    };

                    _context.Files.Add(newFile);
                    _context.SaveChanges();

                    ViewBag.Message = "JSON raporu alındı ve veritabanına kaydedildi. MD5: " + fileMd5;
                }
                return View("Result");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ViewBag.Error = "JSON raporu alınamadı: " + error;
                return View("Result");
            }
        }

        // Bu metod daha önce açıkladığımız FileSeq mantığını uygular.
        // Eğer MD5 ilk kez geliyorsa max fileSeq+1, yoksa aynı seq döner.
        private int CalculateFileSeq(string md5)
        {
            var existingFile = _context.Files.FirstOrDefault(f => f.File_md5 == md5);
            if (existingFile == null)
            {
                int maxSeq = _context.Files.Any() ? _context.Files.Max(f => f.FileSeq) : 0;
                return maxSeq + 1;
            }
            else
            {
                return existingFile.FileSeq;
            }
        }


        [HttpGet]
        public async Task<IActionResult> Download()
        {
            var analysisId = HttpContext.Session.GetString("AnalysisId");
            if (string.IsNullOrEmpty(analysisId))
            {
                ViewBag.Error = "Analysis ID yok, rapor indirilemez.";
                return View("Result");
            }

            var apiBaseUrl = _configuration["MobSF:ApiBaseUrl"];
            var apiKey = _configuration["MobSF:ApiKey"];

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("X-Mobsf-Api-Key", apiKey);

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("hash", analysisId)
            });

            var response = await client.PostAsync($"{apiBaseUrl}/report_json", formContent);
            if (response.IsSuccessStatusCode)
            {
                var jsonBytes = await response.Content.ReadAsByteArrayAsync();
                return File(jsonBytes, "application/json", $"{analysisId}.json");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ViewBag.Error = "Rapor indirilemedi: " + error;
                return View("Result");
            }
        }

        [HttpGet]
        public IActionResult History()
        {
            // Burada önceki analizleri listeleme gibi bir özellik ekleyebilirsiniz.
            return Content("Daha önce yüklenen dosyalar burada listelenecek.");
        }

    }
}
