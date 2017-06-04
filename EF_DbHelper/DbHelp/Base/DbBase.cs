using DbHelp.DBContext;
using EntityFramework.Extensions;
using Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace DbHelp.Base
{
    public class DbBase
    {
        //是否读写分离(可以配置在配置文件中)
        private static readonly bool IsReadWriteSeparation = true;

        #region EF上下文对象(主库)

        protected DbContext MasterDb => _masterDb.Value;
        private readonly Lazy<DbContext> _masterDb = new Lazy<DbContext>(() => new DbContextFactory().GetWriteDbContext());

        #endregion EF上下文对象(主库)

        #region EF上下文对象(从库)

        protected DbContext SlaveDb => IsReadWriteSeparation ? _slaveDb.Value : _masterDb.Value;
        private readonly Lazy<DbContext> _slaveDb = new Lazy<DbContext>(() => new DbContextFactory().GetReadDbContext());

        #endregion EF上下文对象(从库)

        #region 自定义其他方法

        /// <summary>
        /// 执行存储过程或自定义sql语句--返回集合(自定义返回类型)
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parms"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public List<TModel> Query<TModel>(string sql, List<SqlParameter> parms, CommandType cmdType = CommandType.Text)
        {
            //存储过程（exec getActionUrlId @name,@ID）
            if (cmdType == CommandType.StoredProcedure)
            {
                StringBuilder paraNames = new StringBuilder();
                foreach (var sqlPara in parms)
                {
                    paraNames.Append($" @{sqlPara},");
                }
                sql = paraNames.Length > 0 ? $"exec {sql} {paraNames.ToString().Trim(',')}" : $"exec {sql} ";
            }
            var list = SlaveDb.Database.SqlQuery<TModel>(sql, parms.ToArray());
            var enityList = list.ToList();
            return enityList;
        }

        /// <summary>
        /// 自定义语句和存储过程的增删改--返回影响的行数
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parms"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public int Execute(string sql, List<SqlParameter> parms, CommandType cmdType = CommandType.Text)
        {
            //存储过程（exec getActionUrlId @name,@ID）
            if (cmdType == CommandType.StoredProcedure)
            {
                StringBuilder paraNames = new StringBuilder();
                foreach (var sqlPara in parms)
                {
                    paraNames.Append($" @{sqlPara},");
                }
                sql = paraNames.Length > 0 ?
                    $"exec {sql} {paraNames.ToString().Trim(',')}" :
                    $"exec {sql} ";
            }
            int ret = MasterDb.Database.ExecuteSqlCommand(sql, parms.ToArray());
            return ret;
        }

        #endregion 自定义其他方法
    }

    /// <summary>
    /// mssql数据库 数据层 父类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DbBase<T> : DbBase where T : class, new()
    {
        #region INSERT

        /// <summary>
        /// 新增 实体
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void Insert(T model)
        {
            MasterDb.Set<T>().Add(model);
        }

        /// <summary>
        /// 普通批量插入
        /// </summary>
        /// <param name="datas"></param>
        public void InsertRange(List<T> datas)
        {
            MasterDb.Set<T>().AddRange(datas);
        }

        #endregion INSERT

        #region DELETE

        /// <summary>
        /// 根据模型删除
        /// </summary>
        /// <param name="model">包含要删除id的对象</param>
        /// <returns></returns>
        public void Delete(T model)
        {
            MasterDb.Set<T>().Attach(model);
            MasterDb.Set<T>().Remove(model);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="whereLambda"></param>
        public void Delete(Expression<Func<T, bool>> whereLambda)
        {
            MasterDb.Set<T>().Where(whereLambda).Delete();
        }

        #endregion DELETE

        #region UPDATE

        /// <summary>
        /// 单个对象指定列修改
        /// </summary>
        /// <param name="model">要修改的实体对象</param>
        /// <param name="proNames">要修改的 属性 名称</param>
        /// <param name="isProUpdate"></param>
        /// <returns></returns>
        public void Update(T model, List<string> proNames, bool isProUpdate = true)
        {
            //将 对象 添加到 EF中
            MasterDb.Set<T>().Attach(model);
            var setEntry = ((IObjectContextAdapter)MasterDb).ObjectContext.ObjectStateManager.GetObjectStateEntry(model);
            //指定列修改
            if (isProUpdate)
            {
                foreach (string proName in proNames)
                {
                    setEntry.SetModifiedProperty(proName);
                }
            }
            //忽略类修改
            else
            {
                Type t = typeof(T);
                List<PropertyInfo> proInfos = t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
                foreach (var item in proInfos)
                {
                    string proName = item.Name;
                    if (proNames.Contains(proName))
                    {
                        continue;
                    }
                    setEntry.SetModifiedProperty(proName);
                }
            }
        }

        /// <summary>
        /// 单个对象修改
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public void Update(T model)
        {
            DbEntityEntry entry = MasterDb.Entry<T>(model);
            MasterDb.Set<T>().Attach(model);
            entry.State = EntityState.Modified;
        }

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <param name="updateExpression"></param>
        public void Update(Expression<Func<T, bool>> whereLambda, Expression<Func<T, T>> updateExpression)
        {
            MasterDb.Set<T>().Where(whereLambda).Update(updateExpression);
        }

        /// <summary>
        /// 批量修改
        /// </summary>
        /// <param name="models"></param>
        /// <returns></returns>
        public void UpdateAll(List<T> models)
        {
            foreach (var model in models)
            {
                DbEntityEntry entry = MasterDb.Entry(model);
                entry.State = EntityState.Modified;
            }
        }

        /// <summary>
        /// 批量统一修改
        /// </summary>
        /// <param name="model">要修改的实体对象</param>
        /// <param name="whereLambda">查询条件</param>
        /// <param name="modifiedProNames"></param>
        /// <returns></returns>
        public void Update(T model, Expression<Func<T, bool>> whereLambda, params string[] modifiedProNames)
        {
            //查询要修改的数据
            List<T> listModifing = MasterDb.Set<T>().Where(whereLambda).ToList();
            Type t = typeof(T);
            List<PropertyInfo> proInfos = t.GetProperties(BindingFlags.Instance | BindingFlags.Public).ToList();
            Dictionary<string, PropertyInfo> dictPros = new Dictionary<string, PropertyInfo>();
            proInfos.ForEach(p =>
            {
                if (modifiedProNames.Contains(p.Name))
                {
                    dictPros.Add(p.Name, p);
                }
            });
            if (dictPros.Count <= 0)
            {
                throw new Exception("指定修改的字段名称有误或为空");
            }
            foreach (var item in dictPros)
            {
                PropertyInfo proInfo = item.Value;

                //取出 要修改的值
                object newValue = proInfo.GetValue(model, null);

                //批量设置 要修改 对象的 属性
                foreach (T oModel in listModifing)
                {
                    //为 要修改的对象 的 要修改的属性 设置新的值
                    proInfo.SetValue(oModel, newValue, null);
                }
            }
        }

        #endregion UPDATE

        #region SELECT

        /// <summary>
        /// 根据主键查询
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public T FindById(dynamic id)
        {
            return SlaveDb.Set<T>().Find(id);
        }

        /// <summary>
        /// 获取默认一条数据，没有则为NULL
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public T FirstOrDefault(Expression<Func<T, bool>> whereLambda = null)
        {
            if (whereLambda == null)
            {
                return SlaveDb.Set<T>().FirstOrDefault();
            }
            return SlaveDb.Set<T>().FirstOrDefault(whereLambda);
        }

        /// <summary>
        /// 获取全部数据
        /// </summary>
        /// <returns></returns>
        public List<T> GetAll(string ordering = null)
        {
            return ordering == null
                ? SlaveDb.Set<T>().ToList()
                : SlaveDb.Set<T>().OrderBy(ordering).ToList();
        }

        /// <summary>
        /// 带条件查询获取数据
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <param name="ordering"></param>
        /// <returns></returns>
        public List<T> GetAll(Expression<Func<T, bool>> whereLambda, string ordering = null)
        {
            var iQueryable = SlaveDb.Set<T>().Where(whereLambda);
            return ordering == null
                ? iQueryable.ToList()
                : iQueryable.OrderBy(ordering).ToList();
        }

        /// <summary>
        /// 带条件查询获取数据
        /// </summary>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public IQueryable<T> GetAllIQueryable(Expression<Func<T, bool>> whereLambda = null)
        {
            return whereLambda == null ? SlaveDb.Set<T>() : SlaveDb.Set<T>().Where(whereLambda);
        }

        /// <summary>
        /// 获取数量
        /// </summary>
        /// <param name="whereLambd"></param>
        /// <returns></returns>
        public int GetCount(Expression<Func<T, bool>> whereLambd = null)
        {
            return whereLambd == null ? SlaveDb.Set<T>().Count() : SlaveDb.Set<T>().Where(whereLambd).Count();
        }

        /// <summary>
        /// 判断对象是否存在
        /// </summary>
        /// <param name="whereLambd"></param>
        /// <returns></returns>
        public bool Any(Expression<Func<T, bool>> whereLambd)
        {
            return SlaveDb.Set<T>().Where(whereLambd).Any();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="rows">总条数</param>
        /// <param name="orderBy">排序条件（一定要有）</param>
        /// <param name="whereLambda">查询添加（可有，可无）</param>
        /// <param name="isOrder">是否是Order排序</param>
        /// <returns></returns>
        public List<T> Page<TKey>(int pageIndex, int pageSize, out int rows, Expression<Func<T, TKey>> orderBy, Expression<Func<T, bool>> whereLambda = null, bool isOrder = true)
        {
            IQueryable<T> data = isOrder ?
                SlaveDb.Set<T>().OrderBy(orderBy) :
                SlaveDb.Set<T>().OrderByDescending(orderBy);

            if (whereLambda != null)
            {
                data = data.Where(whereLambda);
            }
            rows = data.Count();
            return data.PageBy((pageIndex - 1) * pageSize, pageSize).ToList();
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="rows">总条数</param>
        /// <param name="ordering">排序条件（一定要有）</param>
        /// <param name="whereLambda">查询添加（可有，可无）</param>
        /// <returns></returns>
        public List<T> Page(int pageIndex, int pageSize, out int rows, string ordering, Expression<Func<T, bool>> whereLambda = null)
        {
            // 分页 一定注意： Skip 之前一定要 OrderBy
            var data = SlaveDb.Set<T>().OrderBy(ordering);
            if (whereLambda != null)
            {
                data = data.Where(whereLambda);
            }
            rows = data.Count();
            return data.PageBy((pageIndex - 1) * pageSize, pageSize).ToList();
        }

        /// <summary>
        /// 查询转换
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        public List<TDto> Select<TDto>(Expression<Func<T, bool>> whereLambda)
        {
            return SlaveDb.Set<T>().Where(whereLambda).Select<TDto>().ToList();
        }

        #endregion SELECT

        #region ORTHER

        /// <summary>
        /// 执行存储过程或自定义sql语句--返回集合
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parms"></param>
        /// <param name="cmdType"></param>
        /// <returns></returns>
        public List<T> Query(string sql, List<SqlParameter> parms, CommandType cmdType = CommandType.Text)
        {
            return Query<T>(sql, parms, cmdType);
        }

        /// <summary>
        /// 提交保存
        /// </summary>
        /// <returns></returns>
        public int SaveChanges()
        {
            return MasterDb.SaveChanges();
        }

        /// <summary>
        /// 回滚
        /// </summary>
        public void RollBackChanges()
        {
            var items = MasterDb.ChangeTracker.Entries().ToList();
            items.ForEach(o => o.State = EntityState.Unchanged);
        }

        #endregion ORTHER
    }
}