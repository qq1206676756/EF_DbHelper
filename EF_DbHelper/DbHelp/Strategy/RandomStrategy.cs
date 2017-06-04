using DbHelp.DBContext;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Reflection;

namespace DbHelp.Strategy
{
    /// <summary>
    /// 随机策略
    /// </summary>
    public class RandomStrategy : IReadDbStrategy
    {
        //所有读库类型
        public static List<Type> DbTypes;

        static RandomStrategy()
        {
            LoadDbs();
        }

        //加载所有的读库类型
        private static void LoadDbs()
        {
            DbTypes = new List<Type>();
            var assembly = Assembly.GetExecutingAssembly();
            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.BaseType == typeof(BaseReadDbContext))
                {
                    DbTypes.Add(type);
                }
            }
        }

        public DbContext GetDbContext()
        {
            int randomIndex = new Random().Next(0, DbTypes.Count);
            var dbType = DbTypes[randomIndex];
            var dbContext = Activator.CreateInstance(dbType) as DbContext;
            return dbContext;
        }
    }
}