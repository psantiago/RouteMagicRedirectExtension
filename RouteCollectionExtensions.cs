using System;
using System.Web.Routing;
using RouteMagic.Internals;

namespace RouteMagicRedirectExtension
{
    /// <summary>
    /// Extends RouteCollection to have a redirect function to add a RedirectRouteExtended instance the RouteCollection.
    /// </summary>
    public static class RouteCollectionExtensions
    {
        
        /// <summary>
        /// We always want to map the RedirectRoute *BEFORE* the legacy route that we're going to redirect.
        /// Otherwise the redirect route will never match because the legacy route will supersede it. 
        /// Hence the Func&lt;RouteCollection, RouteBase&gt;.
        /// </summary>
        /// <param name="routes"></param>
        /// <param name="routeMapping"></param>
        /// <param name="permanent"></param>
        /// <param name="onRedirectAction">This action will be triggered whenever this redirect happens. This can be useful for logging.</param>
        /// <returns></returns>
        public static RedirectRouteExtended Redirect(
            this RouteCollection routes,
            Func<RouteCollection, RouteBase> routeMapping,
            bool permanent = false,
            Action<RequestContext, RedirectRouteExtended> onRedirectAction = null)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }
            if (routeMapping == null)
            {
                throw new ArgumentNullException("routeMapping");
            }

            var routeCollection = new RouteCollection();
            var legacyRoute = routeMapping(routeCollection);

            var redirectRoute = new RedirectRouteExtended(legacyRoute, null, permanent, null, onRedirectAction);
            routes.Add(new NormalizeRoute(redirectRoute));
            return redirectRoute;
        }
    }
}
