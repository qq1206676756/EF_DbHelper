using DbHelp.Strategy;
using System.Data.Entity;
using System.Runtime.Remoting.Messaging;

namespace DbHelp.DBContext
{
    public class DbContextFactory
    {
        //todo:这里可以自己通过注入的方式来实现，就会更加灵活
        private static readonly IReadDbStrategy ReadDbStrategy = new RandomStrategy();

        public DbContext GetWriteDbContext()
        {
            string key = typeof(DbContextFactory).Name + "WriteDbContext";
            DbContext dbContext = CallContext.GetData(key) as DbContext;
            if (dbContext == null)
            {
                dbContext = new WriteDbContext();
                CallContext.SetData(key, dbContext);
            }
            return dbContext;
        }

        public DbContext GetReadDbContext()
        {
            string key = typeof(DbContextFactory).Name + "ReadDbContext";
            DbContext dbContext = CallContext.GetData(key) as DbContext;
            if (dbContext == null)
            {
                dbContext = ReadDbStrategy.GetDbContext();
                CallContext.SetData(key, dbContext);
            }
            return dbContext;
        }
    }
}