using System;
using System.Data.Entity;
using System.Reflection;

namespace DbHelp.DBContext
{
    public class BaseReadDbContext : DbContext
    {
        static BaseReadDbContext()
        {
            Database.SetInitializer(new NullDatabaseInitializer<BaseReadDbContext>());
        }

        public BaseReadDbContext(string connReadStr) : base(connReadStr)
        {
            Configuration.AutoDetectChangesEnabled = false;
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.AddFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            throw new InvalidOperationException("只读数据库,不允许写入");
        }
    }
}