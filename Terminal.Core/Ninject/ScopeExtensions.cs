using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject.Syntax;
using Ninject.Activation;
using System.Web;

namespace Terminal.Core.Ninject
{
    public static class ScopeExtensions
    {
        public static IBindingNamedWithOrOnSyntax<T> InSessionScope<T>(this IBindingInSyntax<T> parent)
        {
            return parent.InScope(SessionScopeCallback);
        }

        private const string _sessionKey = "Ninject Session Scope Sync Root";
        private static object SessionScopeCallback(IContext context)
        {
            if (HttpContext.Current.Session[_sessionKey] == null)
            {
                HttpContext.Current.Session[_sessionKey] = new object();
            }

            return HttpContext.Current.Session[_sessionKey];
        }
    }
}
