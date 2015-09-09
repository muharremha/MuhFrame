using Muh.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Muh.Data.Mapping.Customers
{
    public partial class CustomerMap :NopEntityTypeConfiguration<Customer>
    {
        public CustomerMap()
        {
            this.ToTable("Customer");
            this.HasKey(c => c.Id);
            this.Property(u => u.Username).HasMaxLength(1000);
            this.Property(u => u.Email).HasMaxLength(1000);

            this.HasMany(c => c.Addresses)
                .WithMany()
                .Map(m => m.ToTable("CustomerAddress"));
            this.HasOptional(c => c.HomeAddress);            
            this.HasMany(c => c.CustomerRoles)
             .WithMany()
             .Map(m => m.ToTable("Customer_CustomerRole_Mapping"));

            this.Ignore(u => u.PasswordFormat);
        }
    }
}
