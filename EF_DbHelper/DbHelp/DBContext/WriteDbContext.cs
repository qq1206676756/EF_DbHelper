using System.Data.Entity;
using System.Reflection;

namespace DbHelp.DBContext
{
    public class WriteDbContext : DbContext
    {
        public WriteDbContext() :
            base("name=connWriteStr")
        {
            Configuration.AutoDetectChangesEnabled = true;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.AddFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}