using AutoMapper.QueryableExtensions;
using System;
using System.Linq;

namespace Extension
{
    public static class QueryableExtension
    {
        /// <summary>
        /// 分页查询扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="skipCount"></param>
        /// <param name="maxResultCount"></param>
        /// <returns></returns>
        public static IQueryable<T> PageBy<T>(this IQueryable<T> query, int skipCount, int maxResultCount)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            return query.Skip(skipCount).Take(maxResultCount);
        }

        /// <summary>
        /// 查询扩展
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<T> Select<T>(this IQueryable query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            return query.ProjectTo<T>();
        }
    }
}