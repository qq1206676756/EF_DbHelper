using Model;
using System.Data.Entity.ModelConfiguration;

namespace DbHelp.ModelConfigurations
{
    public class AccoutInfoConfiguration : EntityTypeConfiguration<AccoutInfo>
    {
        public AccoutInfoConfiguration()
        {
            HasKey(o => o.Id);
            Property(o => o.Name).IsRequired();
        }
    }
}