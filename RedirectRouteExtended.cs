using System;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Routing;
using RouteMagic;
using RouteMagic.HttpHandlers;

namespace RouteMagicRedirectExtension
{
    /// <summary>
    /// Slightly extended RouteMagic.Internals.RedirectRoute - allows specifying an action to call during redirection.
    /// Additionally redirects will also keep the query string.
    /// </summary>
    public class RedirectRouteExtended : RouteBase, IRouteHandler
    {
        public RedirectRouteExtended(RouteBase sourceRoute, RouteBase targetRoute, bool permanent)
            : this(sourceRoute, targetRoute, permanent, null)
        {
        }

        public RedirectRouteExtended(
            RouteBase sourceRoute,
            RouteBase targetRoute,
            bool permanent,
            RouteValueDictionary additionalRouteValues,
            Action<RequestContext, RedirectRouteExtended> onRedirectAction = null)
        {
            SourceRoute = sourceRoute;
            TargetRoute = targetRoute;
            Permanent = permanent;
            AdditionalRouteValues = additionalRouteValues;
            OnRedirectAction = onRedirectAction;
        }

        public Action<RequestContext, RedirectRouteExtended> OnRedirectAction { get; set; }

        public RouteBase SourceRoute
        {
            get;
            set;
        }

        public RouteBase TargetRoute
        {
            get;
            set;
        }

        public bool Permanent
        {
            get;
            set;
        }

        public RouteValueDictionary AdditionalRouteValues
        {
            get;
            private set;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext)
        {
            // Use the original route to match
            var routeData = SourceRoute.GetRouteData(httpContext);
            if (routeData == null)
            {
                return null;
            }
            // But swap its route handler with our own
            routeData.RouteHandler = this;
            return routeData;
        }

        public override VirtualPathData GetVirtualPath(RequestContext requestContext, RouteValueDictionary values)
        {
            // Redirect routes never generate an URL.
            return null;
        }

        public RedirectRouteExtended To(RouteBase targetRoute)
        {
            return To(targetRoute, null);
        }

        public RedirectRouteExtended To(RouteBase targetRoute, object routeValues)
        {
            return To(targetRoute, new RouteValueDictionary(routeValues));
        }

        public RedirectRouteExtended To(RouteBase targetRoute, RouteValueDictionary routeValues)
        {
            if (targetRoute == null)
            {
                throw new ArgumentNullException("targetRoute");
            }

            // Set once only
            if (TargetRoute != null)
            {
                throw new InvalidOperationException(/* TODO */);
            }
            TargetRoute = targetRoute;

            // Set once only
            if (AdditionalRouteValues != null)
            {
                throw new InvalidOperationException(/* TODO */);
            }
            AdditionalRouteValues = routeValues;
            return this;
        }

        public new IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            //run the onredirection action. For example, this can be used to log that the redirection ocurred,
            //or add additional route data at redirection-time.
            if (OnRedirectAction != null) OnRedirectAction(requestContext, this);

            var requestRouteValues = requestContext.RouteData.Values;

            var routeValues = AdditionalRouteValues.Merge(requestRouteValues);

            var vpd = TargetRoute.GetVirtualPath(requestContext, routeValues);

            if (vpd != null)
            {
                string targetUrl = "~/" + vpd.VirtualPath;

                //add query strings
                var qsHelper = requestContext.HttpContext.Request.QueryString;
                var queryString = String.Join("&", qsHelper.AllKeys.Select(i => i + "=" + qsHelper[i]));
                if (!string.IsNullOrWhiteSpace(queryString))
                {
                    targetUrl += "?" + queryString;
                }

                targetUrl = HttpUtility.UrlEncode(targetUrl);

                return new RedirectHttpHandler(targetUrl, Permanent, isReusable: false);
            }
            return new DelegateHttpHandler(rc => rc.HttpContext.Response.StatusCode = 404, requestContext.RouteData, false);
        }
    }
}