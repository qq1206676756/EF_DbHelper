# EF_DbHelper
**Entity Framework 数据访问通用类，支持读写分离**

- DbBase.cs 为通用操作封装类
- DBContextFactory.cs 为获取DBContext的工厂模型
- 如果需要实现自己的切换策略，可自己继承IReadDbStrategy后，自己写扩展

---

**使用例子如下：**

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
