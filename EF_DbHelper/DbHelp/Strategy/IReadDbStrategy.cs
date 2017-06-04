using System.Data.Entity;

namespace DbHelp.Strategy
{
    /// <summary>
    /// 从数据库获取策略接口
    /// </summary>
    public interface IReadDbStrategy
    {
        /// <summary>
        /// 获取读库
        /// </summary>
        /// <returns></returns>
        DbContext GetDbContext();
    }
}