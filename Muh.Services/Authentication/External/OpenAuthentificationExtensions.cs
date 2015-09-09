using System;
using Muh.Core.Domain.Customers;

namespace Muh.Services.Authentication.External
{
    /// <summary>
    /// Open authentication extensions
    /// </summary>
    public static class OpenAuthenticationExtensions
    {
        public static bool IsMethodActive(this IExternalAuthenticationMethod method,
            ExternalAuthenticationSettings settings)
        {
            if (method == null)
                throw new ArgumentNullException("method");

            if (settings == null)
                throw new ArgumentNullException("settings");

            if (settings.ActiveAuthenticationMethodSystemNames == null)
                return false;

            return false;
        }
    }
}
