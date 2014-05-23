RouteMagicRedirectExtension
===========================

Adds RedirectRouteExtended class which handles query strings in redirects, and allows for a specified Action to call during redirects, which may be helpful during logging.

For Example:

    var defaults = new
    {
        controller = "Home",
        action = "Index",
        id = UrlParameter.Optional
    };

    var newRoute = routes.MapRoute("Default_WithStreet", "Street/{controller}/{action}/{id}", defaults);
    routes.Redirect(r => r.MapRoute("Default", "{controller}/{action}/{id}", defaults)).To(newRoute);

With this route config, <code>Home/LivingRoom?furniture=Couch</code> will automagically be rerouted to <code>Street/Home/LivingRoom?furniture=Couch</code>, whereas RouteMagic would redirect instead to just <code>Street/Home/LivingRoom</code>.

Additionally, we can supply an action to run during a redirection:

    routes.Redirect(r => r.MapRoute("Default", "{controller}/{action}/{id}", defaults), onRedirectAction: SomeImportantAction).To(newRoute);
        }
        
    ...
    
    public static void SomeImportantAction(RequestContext context, RedirectRouteExtended rre)
    {
        Logger.Info("Redirected: " + context.HttpContext.Request.Url.AbsoluteUri);
    }
    
And 'SomeImportantAction' will be called whenever the redirect happens. Squeeeeeeee!