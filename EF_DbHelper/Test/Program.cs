using DbHelp.Base;
using Model;
using System;
using System.Threading;

namespace Test
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //todo:这里仅仅是测试DbBase类的使用，实际使用可以自己结合项目做分层使用

            DbBase<AccoutInfo> db = new DbBase<AccoutInfo>();
            var model = new AccoutInfo()
            {
                Name = "qt",
                Time = DateTime.Now
            };
            db.Insert(model);
            db.SaveChanges();

            Thread.Sleep(3000);

            var account = db.FirstOrDefault(p => p.Id == model.Id);
            Console.WriteLine(account.Name);
        }
    }
}