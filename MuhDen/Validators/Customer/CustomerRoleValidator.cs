using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentValidation;
using Muh.Web.Framework.Validators;
using MuhDen.Models.Customer;
using Muh.Services.Localization;

namespace MuhDen.Validators.Customer
{
    public class CustomerRoleValidator : BaseNopValidator<CustomerRoleModel>
    {
        public CustomerRoleValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Customers.CustomerRoles.Fields.Name.Required"));
        }
    }
}