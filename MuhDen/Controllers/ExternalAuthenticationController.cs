using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using Muh.Core;
using Muh.Services.Authentication.External;
using Muh.Web.Framework.Controllers;
using MuhDen.Models.Customer;

namespace MuhDen.Controllers
{
    public partial class ExternalAuthenticationController : BaseController
    {
        #region Fields

        private readonly IOpenAuthenticationService _openAuthenticationService;

        #endregion

        #region Constructors

        public ExternalAuthenticationController(IOpenAuthenticationService openAuthenticationService)
        {
            this._openAuthenticationService = openAuthenticationService;
        }

        #endregion

        #region Methods

        public RedirectResult RemoveParameterAssociation(string returnUrl)
        {

            //prevent open redirection attack
            if (!Url.IsLocalUrl(returnUrl))
                returnUrl = Url.RouteUrl("HomePage");
            
            ExternalAuthorizerHelper.RemoveParameters();
            return   Redirect(returnUrl);
        }

        [ChildActionOnly]
        public ActionResult ExternalMethods()
        {
            //model
            var model = new List<ExternalAuthenticationMethodModel>();

            //TODO :Muh_değiştirildi 
            //foreach (var eam in _openAuthenticationService
            //    .LoadActiveExternalAuthenticationMethods(0))
            //{
            //    var eamModel = new ExternalAuthenticationMethodModel();

            //    string actionName;
            //    string controllerName;
            //    RouteValueDictionary routeValues;
            //    eam.GetPublicInfoRoute(out actionName, out controllerName, out routeValues);
            //    eamModel.ActionName = actionName;
            //    eamModel.ControllerName = controllerName;
            //    eamModel.RouteValues = routeValues;

            //    model.Add(eamModel);
            //}

            return PartialView(model);
        }

        #endregion
    }
}