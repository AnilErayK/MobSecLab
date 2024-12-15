using Microsoft.EntityFrameworkCore;

namespace MobSecLab.Models
{
    public class MobSecLabContext : DbContext
    {
        public MobSecLabContext(DbContextOptions<MobSecLabContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }

        public DbSet<FileEntity> Files { get; set; }

    }
}
