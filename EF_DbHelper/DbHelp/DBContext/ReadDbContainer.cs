using System.Data.Entity;

namespace DbHelp.DBContext
{
    public class ReadDbContext : BaseReadDbContext
    {
        static ReadDbContext()
        {
            Database.SetInitializer(new NullDatabaseInitializer<ReadDbContext>());
        }

        public ReadDbContext() : base("connReadStr")
        {
        }
    }

    public class ReadDb1Context : BaseReadDbContext
    {
        static ReadDb1Context()
        {
            Database.SetInitializer(new NullDatabaseInitializer<ReadDb1Context>());
        }

        public ReadDb1Context() : base("connReadStr1")
        {
        }
    }

    public class ReadDb2Context : BaseReadDbContext
    {
        static ReadDb2Context()
        {
            Database.SetInitializer(new NullDatabaseInitializer<ReadDb2Context>());
        }

        public ReadDb2Context() : base("connReadStr2")
        {
        }
    }
}