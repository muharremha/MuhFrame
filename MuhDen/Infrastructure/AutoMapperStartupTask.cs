using Muh.Core.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Muh.Core.Domain.Customers;
using MuhDen.Models.Customer;

namespace MuhDen.Infrastructure
{
    public class AutoMapperStartupTask : IStartupTask
    {
        public void Execute()
        {
            //customer roles
            Mapper.CreateMap<CustomerRole, CustomerRoleModel>()
                .ForMember(dest => dest.PurchasedWithProductName, mo => mo.Ignore())
                .ForMember(dest => dest.CustomProperties, mo => mo.Ignore());
            Mapper.CreateMap<CustomerRoleModel, CustomerRole>()
                .ForMember(dest => dest.PermissionRecords, mo => mo.Ignore());
        }


         public int Order
        {
            get { return 0; }
        }
    }
}