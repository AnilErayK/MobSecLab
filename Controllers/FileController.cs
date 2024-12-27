using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MobSecLab.Models;
using System;
using System.IO;
using System.Linq;
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
            else if(file.Length > 31457280){
                ViewBag.Error = "Dosya boyutu 30 MB'ı aştı. Lütfen daha küçük bir dosya seçin.";
                return View("Upload");
            }
            var apiBaseUrl = _configuration["MobSF:ApiBaseUrl"];
            var apiKey = _configuration["MobSF:ApiKey"];

            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                var fileBytes = ms.ToArray();

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromMinutes(10);
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
                            // AnalysisId ve UploadedFileName'i Session'a kaydet
                            HttpContext.Session.SetString("AnalysisId", hash);
                            HttpContext.Session.SetString("UploadedFileName", file.FileName);

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

        // Bu metot sadece Scan işleminden sonra çağrılır, public veya private yapınıza göre düzenleyebilirsiniz.
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

            var response = await client.PostAsync($"{apiBaseUrl}/report_json", formContent);

            if (response.IsSuccessStatusCode)
            {
                var jsonStr = await response.Content.ReadAsStringAsync();
                using (var doc = JsonDocument.Parse(jsonStr))
                {
                    var fileMd5 = FindLastHash(doc.RootElement);

                    if (fileMd5 == null)
                    {
                        ViewBag.Error = "JSON içinde hiçbir 'hash' alanı bulunamadı. Yanıt: " + jsonStr;
                        return View("Result");
                    }

                    var username = User.Identity.Name;
                    var currentUser = _context.Users.FirstOrDefault(u => u.Username == username);
                    if (currentUser == null)
                    {
                        ViewBag.Error = "Kullanıcı bulunamadı.";
                        return View("Result");
                    }

                    var userNo = currentUser.UserNo;
                    var fileName = HttpContext.Session.GetString("UploadedFileName");
                    if (string.IsNullOrEmpty(fileName))
                    {
                        ViewBag.Error = "Dosya ismi bulunamadı. Lütfen dosya yükleme adımını kontrol edin.";
                        return View("Result");
                    }

                    var fileSeq = CalculateFileSeq(fileMd5);

                    // total_malware_permissions
                    int totalMalware = 0;
                    var totalMalwareStr = FindPropertyValue(doc.RootElement, "total_malware_permissions");
                    if (!string.IsNullOrEmpty(totalMalwareStr) && int.TryParse(totalMalwareStr, out int tmVal))
                    {
                        totalMalware = tmVal;
                    }

                    // total_other_permissions
                    int totalOther = 0;
                    var totalOtherStr = FindPropertyValue(doc.RootElement, "total_other_permissions");
                    if (!string.IsNullOrEmpty(totalOtherStr) && int.TryParse(totalOtherStr, out int toVal))
                    {
                        totalOther = toVal;
                    }

                    int totalPermission = totalMalware + totalOther;

                    // min_sdk
                    var minSdk = FindPropertyValue(doc.RootElement, "min_sdk");
                    if (string.IsNullOrEmpty(minSdk)) 
                        minSdk = "Bilinmiyor";

                    // security_score
                    int securityScore = 0;
                    var securityScoreStr = FindPropertyValue(doc.RootElement, "security_score");
                    if (!string.IsNullOrEmpty(securityScoreStr) && int.TryParse(securityScoreStr, out int ssVal))
                    {
                        securityScore = ssVal;
                    }

                    // severity: "high" sayısı
                    int severityHighCount = CountOccurrences(jsonStr, "\"severity\": \"high\"");
                    // status: "dangerous" sayısı
                    int statusDangerousCount = CountOccurrences(jsonStr, "\"status\": \"dangerous\"");

                    // Files tablosuna kaydet
                    var newFile = new FileEntity
                    {
                        UserNo = userNo,
                        FileSeq = fileSeq,
                        File_Name = fileName,
                        File_md5 = fileMd5
                    };
                    _context.Files.Add(newFile);
                    _context.SaveChanges();

                    // Results tablosuna kaydet
                    var newResult = new Models.Results
                    {
                        FileSeq = fileSeq,
                        md5 = fileMd5,
                        File_Name = fileName,
                        UserNo = userNo,
                        TotalMalwarePermission = totalMalware,
                        TotalPermission = totalPermission,
                        SeverityHigh = severityHighCount,
                        StatusDangerous = statusDangerousCount,
                        minSdk = minSdk,
                        SecurityScore = securityScore
                    };
                    _context.Results.Add(newResult);
                    _context.SaveChanges();

                    // ViewBag üzerinden kullanıcıya göstereceğimiz veriler
                    ViewBag.FileName = fileName;
                    ViewBag.FileMd5 = fileMd5;
                    ViewBag.TotalMalware = totalMalware;
                    ViewBag.TotalPermission = totalPermission;
                    ViewBag.SeverityHigh = severityHighCount;
                    ViewBag.StatusDangerous = statusDangerousCount;
                    ViewBag.MinSdk = minSdk;
                    ViewBag.SecurityScore = securityScore;

                    ViewBag.Message = "Analiz tamamlandı.";
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

        private int CountOccurrences(string source, string substring)
        {
            int count = 0;
            int index = 0;
            while ((index = source.IndexOf(substring, index, StringComparison.OrdinalIgnoreCase)) != -1)
            {
                count++;
                index += substring.Length;
            }
            return count;
        }

        /// <summary>
        /// Bu metot JSON içinde belirtilen property'yi derinlemesine arar ve ilk bulduğu değeri string olarak döndürür.
        /// Eğer property bulunamazsa null döner.
        /// </summary>
        private string FindPropertyValue(JsonElement element, string propertyName)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in element.EnumerateObject())
                    {
                        if (prop.NameEquals(propertyName))
                        {
                            // Değerin tipine göre string olarak al
                            // number ise getint32().ToString(), string ise directly .GetString()
                            // JSON ne döndürüyorsa...
                            if (prop.Value.ValueKind == JsonValueKind.Number)
                                return prop.Value.GetRawText(); // numarayı string'e çevirir
                            else if (prop.Value.ValueKind == JsonValueKind.String)
                                return prop.Value.GetString();
                            else
                                return prop.Value.GetRawText();
                        }
                        else
                        {
                            var result = FindPropertyValue(prop.Value, propertyName);
                            if (result != null)
                                return result;
                        }
                    }
                    break;
                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                    {
                        var result = FindPropertyValue(item, propertyName);
                        if (result != null)
                            return result;
                    }
                    break;
            }
            return null;
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
                 await Task.Delay(60000); // Bekleme, gerçek senaryoda asenkron iyileştirilmeli.

                // Bekledikten sonra JSON sonuçlarını çek
                return await GetJsonResult(analysisId);
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                ViewBag.Error = "Tarama başlatılamadı: " + error;
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

        private string FindLastHash(JsonElement element)
        {
            string lastHash = null;

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    // Nesne ise tüm property'leri tara
                    foreach (var prop in element.EnumerateObject())
                    {
                        if (prop.Name == "hash")
                        {
                            // hash bulundu, bunu son bulunan hash olarak kaydet
                            lastHash = prop.Value.GetString();
                        }
                        else
                        {
                            // Bu property'nin değeri de karmaşık olabilir,
                            // tekrar recursion yapıyoruz
                            var result = FindLastHash(prop.Value);
                            if (result != null)
                            {
                                lastHash = result;
                            }
                        }
                    }
                    break;

                case JsonValueKind.Array:
                    // Array ise tüm elemanları tara
                    foreach (var arrElem in element.EnumerateArray())
                    {
                        var result = FindLastHash(arrElem);
                        if (result != null)
                        {
                            lastHash = result;
                        }
                    }
                    break;

                // Diğer durumlarda (string, number, null) alt property yok
                default:
                    break;
            }

            return lastHash;
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
        public IActionResult ResultsHistory()
        {
            var username = User.Identity.Name;
            var currentUser = _context.Users.FirstOrDefault(u => u.Username == username);
            if (currentUser == null)
            {
                ViewBag.Error = "Kullanıcı bulunamadı.";
                return View("Result");
            }

            var userNo = currentUser.UserNo;
            var userResults = _context.Results.Where(r => r.UserNo == userNo).ToList();

            return View("ResultsHistory", userResults);
        }

    }
}