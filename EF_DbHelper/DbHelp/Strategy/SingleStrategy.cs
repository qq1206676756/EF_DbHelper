using DbHelp.DBContext;
using System.Data.Entity;

namespace DbHelp.Strategy
{
    /// <summary>
    /// 单一策略
    /// </summary>
    public class SingleStrategy : IReadDbStrategy
    {
        public DbContext GetDbContext()
        {
            return new ReadDbContext();
        }
    }
}