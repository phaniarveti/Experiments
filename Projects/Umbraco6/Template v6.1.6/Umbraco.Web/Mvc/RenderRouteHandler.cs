using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using Umbraco.Web.Routing;
using umbraco.cms.businesslogic.template;
using System.Collections.Generic;

namespace Umbraco.Web.Mvc
{
	public class RenderRouteHandler : IRouteHandler
	{
		// Define reserved dictionary keys for controller, action and area specified in route additional values data
		private static class ReservedAdditionalKeys
		{
			internal const string Controller = "c";
			internal const string Action = "a";
			internal const string Area = "ar";
		}

		public RenderRouteHandler(IControllerFactory controllerFactory)
		{
			if (controllerFactory == null) throw new ArgumentNullException("controllerFactory");			
			_controllerFactory = controllerFactory;
		}

		/// <summary>
		/// Contructor generally used for unit testing
		/// </summary>
		/// <param name="controllerFactory"></param>
		/// <param name="umbracoContext"></param>
		internal RenderRouteHandler(IControllerFactory controllerFactory, UmbracoContext umbracoContext)
		{
			if (controllerFactory == null) throw new ArgumentNullException("controllerFactory");
			if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
			_controllerFactory = controllerFactory;
			_umbracoContext = umbracoContext;
		}

		private readonly IControllerFactory _controllerFactory;
		private readonly UmbracoContext _umbracoContext;
		
		/// <summary>
		/// Returns the current UmbracoContext
		/// </summary>
		public UmbracoContext UmbracoContext
		{
			get { return _umbracoContext ?? UmbracoContext.Current; }			
		}

		#region IRouteHandler Members

		/// <summary>
		/// Assigns the correct controller based on the Umbraco request and returns a standard MvcHandler to prcess the response,
		/// this also stores the render model into the data tokens for the current RouteData.
		/// </summary>
		/// <param name="requestContext"></param>
		/// <returns></returns>
		public IHttpHandler GetHttpHandler(RequestContext requestContext)
		{			
			if (UmbracoContext == null)
			{			
				throw new NullReferenceException("There is not current UmbracoContext, it must be initialized before the RenderRouteHandler executes");
			}
			var docRequest = UmbracoContext.PublishedContentRequest;
			if (docRequest == null)
			{
				throw new NullReferenceException("There is not current PublishedContentRequest, it must be initialized before the RenderRouteHandler executes");
			}
			
			SetupRouteDataForRequest(
				new RenderModel(docRequest.PublishedContent, docRequest.Culture),
				requestContext,
				docRequest);

			return GetHandlerForRoute(requestContext, docRequest);
			
		}

		#endregion

		/// <summary>
		/// Ensures that all of the correct DataTokens are added to the route values which are all required for rendering front-end umbraco views
		/// </summary>
		/// <param name="renderModel"></param>
		/// <param name="requestContext"></param>
		/// <param name="docRequest"></param>
		internal void SetupRouteDataForRequest(RenderModel renderModel, RequestContext requestContext, PublishedContentRequest docRequest)
		{
			//put essential data into the data tokens, the 'umbraco' key is required to be there for the view engine
			requestContext.RouteData.DataTokens.Add("umbraco", renderModel); //required for the RenderModelBinder and view engine
			requestContext.RouteData.DataTokens.Add("umbraco-doc-request", docRequest); //required for RenderMvcController
			requestContext.RouteData.DataTokens.Add("umbraco-context", UmbracoContext); //required for UmbracoTemplatePage
		}

        private void UpdateRouteDataForRequest(RenderModel renderModel, RequestContext requestContext)
        {
            requestContext.RouteData.DataTokens["umbraco"] = renderModel;
            // the rest should not change -- it's only the published content that has changed
        }

		/// <summary>
		/// Checks the request and query strings to see if it matches the definition of having a Surface controller
		/// posted value, if so, then we return a PostedDataProxyInfo object with the correct information.
		/// </summary>
		/// <param name="requestContext"></param>
		/// <returns></returns>
		private static PostedDataProxyInfo GetPostedFormInfo(RequestContext requestContext)
		{
			if (requestContext.HttpContext.Request.RequestType != "POST")
				return null;

			//this field will contain a base64 encoded version of the surface route vals 
			if (requestContext.HttpContext.Request["uformpostroutevals"].IsNullOrWhiteSpace())
				return null;

			var encodedVal = requestContext.HttpContext.Request["uformpostroutevals"];
			var decryptedString = encodedVal.DecryptWithMachineKey();
			var parsedQueryString = HttpUtility.ParseQueryString(decryptedString);

			var decodedParts = new Dictionary<string, string>();

			foreach (var key in parsedQueryString.AllKeys)
			{
				decodedParts[key] = parsedQueryString[key];
			}

			//validate all required keys exist

			//the controller
			if (!decodedParts.Any(x => x.Key == ReservedAdditionalKeys.Controller))
				return null;
			//the action
			if (!decodedParts.Any(x => x.Key == ReservedAdditionalKeys.Action))
				return null;
			//the area
			if (!decodedParts.Any(x => x.Key == ReservedAdditionalKeys.Area))
				return null;

			////the controller type, if it contains this then it is a plugin controller, not locally declared.
			//if (decodedParts.Any(x => x.Key == "t"))
			//{
			//    return new PostedDataProxyInfo
			//    {
			//        ControllerName = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "c").Value),
			//        ActionName = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "a").Value),
			//        Area = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "ar").Value),
			//        ControllerType = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == "t").Value)
			//    };				
			//}

			foreach (var item in decodedParts.Where(x => !new string[] { 
				ReservedAdditionalKeys.Controller, 
				ReservedAdditionalKeys.Action, 
				ReservedAdditionalKeys.Area }.Contains(x.Key)))
			{
				// Populate route with additional values which aren't reserved values so they eventually to action parameters
			    requestContext.RouteData.Values[item.Key] = item.Value;
			}

			//return the proxy info without the surface id... could be a local controller.
			return new PostedDataProxyInfo
			{
				ControllerName = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == ReservedAdditionalKeys.Controller).Value),
				ActionName = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == ReservedAdditionalKeys.Action).Value),
				Area = requestContext.HttpContext.Server.UrlDecode(decodedParts.Single(x => x.Key == ReservedAdditionalKeys.Area).Value),
			};
		}

		/// <summary>
		/// Handles a posted form to an Umbraco Url and ensures the correct controller is routed to and that
		/// the right DataTokens are set.
		/// </summary>
		/// <param name="requestContext"></param>
		/// <param name="postedInfo"></param>
		private IHttpHandler HandlePostedValues(RequestContext requestContext, PostedDataProxyInfo postedInfo)
		{
			//set the standard route values/tokens
			requestContext.RouteData.Values["controller"] = postedInfo.ControllerName;
			requestContext.RouteData.Values["action"] = postedInfo.ActionName;

            IHttpHandler handler;
            
            //get the route from the defined routes
		    using (RouteTable.Routes.GetReadLock())
		    {
		        Route surfaceRoute;
		        if (postedInfo.Area.IsNullOrWhiteSpace())
		        {
                    //find the controller in the route table without an area
		            surfaceRoute = RouteTable.Routes.OfType<Route>()
		                                         .SingleOrDefault(x => x.Defaults != null &&
		                                                               x.Defaults.ContainsKey("controller") &&
		                                                               x.Defaults["controller"].ToString() == postedInfo.ControllerName &&
		                                                               !x.DataTokens.ContainsKey("area"));                    
		        }
                else
		        {
                    //find the controller in the route table with the specified area
		            surfaceRoute = RouteTable.Routes.OfType<Route>()
		                                         .SingleOrDefault(x => x.Defaults != null &&
		                                                               x.Defaults.ContainsKey("controller") &&
						                      x.Defaults["controller"].ToString().InvariantEquals(postedInfo.ControllerName) &&
		                                                               x.DataTokens.ContainsKey("area") &&
						                      x.DataTokens["area"].ToString().InvariantEquals(postedInfo.Area));                    
		        }

                if (surfaceRoute == null)
                    throw new InvalidOperationException("Could not find a Surface controller route in the RouteTable for controller name " + postedInfo.ControllerName);
                
                //set the area if one is there.
                if (surfaceRoute.DataTokens.ContainsKey("area"))
                {
                    requestContext.RouteData.DataTokens["area"] = surfaceRoute.DataTokens["area"];
                }

                //set the 'Namespaces' token so the controller factory knows where to look to construct it
                if (surfaceRoute.DataTokens.ContainsKey("Namespaces"))
                {
                    requestContext.RouteData.DataTokens["Namespaces"] = surfaceRoute.DataTokens["Namespaces"];
                }
                handler = surfaceRoute.RouteHandler.GetHttpHandler(requestContext);

		    }

			return handler;
		}

		/// <summary>
		/// Returns a RouteDefinition object based on the current renderModel
		/// </summary>
		/// <param name="requestContext"></param>
		/// <param name="publishedContentRequest"></param>
		/// <returns></returns>
		internal virtual RouteDefinition GetUmbracoRouteDefinition(RequestContext requestContext, PublishedContentRequest publishedContentRequest)
		{
		    var defaultControllerType = DefaultRenderMvcControllerResolver.Current.GetDefaultControllerType();
            var defaultControllerName = ControllerExtensions.GetControllerName(defaultControllerType);
			//creates the default route definition which maps to the 'UmbracoController' controller
			var def = new RouteDefinition
				{
					ControllerName = defaultControllerName,
                    ControllerType = defaultControllerType,
					PublishedContentRequest = publishedContentRequest,
					ActionName = ((Route)requestContext.RouteData.Route).Defaults["action"].ToString(),
					HasHijackedRoute = false
				};

            //check that a template is defined), if it doesn't and there is a hijacked route it will just route
            // to the index Action
            if (publishedContentRequest.HasTemplate)
            {
                //the template Alias should always be already saved with a safe name.
                //if there are hyphens in the name and there is a hijacked route, then the Action will need to be attributed
                // with the action name attribute.
                var templateName = publishedContentRequest.TemplateAlias.Split('.')[0].ToSafeAlias();
                def.ActionName = templateName;
            }

			//check if there's a custom controller assigned, base on the document type alias.
            var controllerType = _controllerFactory.GetControllerTypeInternal(requestContext, publishedContentRequest.PublishedContent.DocumentTypeAlias);

			//check if that controller exists
		    if (controllerType != null)
		    {
                //ensure the controller is of type 'IRenderMvcController' and ControllerBase
                if (TypeHelper.IsTypeAssignableFrom<IRenderMvcController>(controllerType)
                    && TypeHelper.IsTypeAssignableFrom<ControllerBase>(controllerType))
		        {
		            //set the controller and name to the custom one
		            def.ControllerType = controllerType;
		            def.ControllerName = ControllerExtensions.GetControllerName(controllerType);
		            if (def.ControllerName != defaultControllerName)
		            {
		                def.HasHijackedRoute = true;
		            }
		        }
		        else
		        {
		            LogHelper.Warn<RenderRouteHandler>(
						"The current Document Type {0} matches a locally declared controller of type {1}. Custom Controllers for Umbraco routing must implement '{2}' and inherit from '{3}'.",
		                () => publishedContentRequest.PublishedContent.DocumentTypeAlias,
		                () => controllerType.FullName,
                        () => typeof(IRenderMvcController).FullName,
                        () => typeof(ControllerBase).FullName);
		            //exit as we cannnot route to the custom controller, just route to the standard one.
		            return def;
		        }
		    }

            //store the route definition
            requestContext.RouteData.DataTokens["umbraco-route-def"] = def;

		    return def;
		}

		internal IHttpHandler GetHandlerOnMissingTemplate(PublishedContentRequest pcr)
		{
            // missing template, so we're in a 404 here
            // so the content, if any, is a custom 404 page of some sort

			if (!pcr.HasPublishedContent)
				// means the builder could not find a proper document to handle 404
				return new PublishedContentNotFoundHandler();

			if (!pcr.HasTemplate)
				// means the engine could find a proper document, but the document has no template
				// at that point there isn't much we can do and there is no point returning
				// to Mvc since Mvc can't do much
				return new PublishedContentNotFoundHandler("In addition, no template exists to render the custom 404.");

            // so we have a template, so we should have a rendering engine
            if (pcr.RenderingEngine == RenderingEngine.WebForms) // back to webforms ?                
                return (global::umbraco.UmbracoDefault)System.Web.Compilation.BuildManager.CreateInstanceFromVirtualPath("~/default.aspx", typeof(global::umbraco.UmbracoDefault));
            else if (pcr.RenderingEngine != RenderingEngine.Mvc) // else ?
                return new PublishedContentNotFoundHandler("In addition, no rendering engine exists to render the custom 404.");

			return null;
		}

		/// <summary>
		/// this will determine the controller and set the values in the route data
		/// </summary>
		/// <param name="requestContext"></param>
		/// <param name="publishedContentRequest"></param>
		internal IHttpHandler GetHandlerForRoute(RequestContext requestContext, PublishedContentRequest publishedContentRequest)
		{
			var routeDef = GetUmbracoRouteDefinition(requestContext, publishedContentRequest);
            
			//Need to check for a special case if there is form data being posted back to an Umbraco URL
			var postedInfo = GetPostedFormInfo(requestContext);
			if (postedInfo != null)
			{
				return HandlePostedValues(requestContext, postedInfo);
			}

			//here we need to check if there is no hijacked route and no template assigned, if this is the case
			//we want to return a blank page, but we'll leave that up to the NoTemplateHandler.
			if (!publishedContentRequest.HasTemplate && !routeDef.HasHijackedRoute)
			{
				publishedContentRequest.UpdateOnMissingTemplate(); // will go 404

                // HandleHttpResponseStatus returns a value indicating that the request should
                // not be processed any further, eg because it has been redirect. then, exit.
			    if (UmbracoModule.HandleHttpResponseStatus(requestContext.HttpContext, publishedContentRequest))
			        return null;

                var handler = GetHandlerOnMissingTemplate(publishedContentRequest);

				// if it's not null it can be either the PublishedContentNotFoundHandler (no document was
				// found to handle 404, or document with no template was found) or the WebForms handler 
				// (a document was found and its template is WebForms)

				// if it's null it means that a document was found and its template is Mvc

				// if we have a handler, return now
				if (handler != null)
					return handler;

				// else we are running Mvc
                // update the route data - because the PublishedContent has changed
                UpdateRouteDataForRequest(
                    new RenderModel(publishedContentRequest.PublishedContent, publishedContentRequest.Culture),
                    requestContext);
                // update the route definition
				routeDef = GetUmbracoRouteDefinition(requestContext, publishedContentRequest);
			}

			//no post values, just route to the controller/action requried (local)

			requestContext.RouteData.Values["controller"] = routeDef.ControllerName;
			if (!string.IsNullOrWhiteSpace(routeDef.ActionName))
			{
				requestContext.RouteData.Values["action"] = routeDef.ActionName;
			}

            // Set the session state requirements
            requestContext.HttpContext.SetSessionStateBehavior(GetSessionStateBehavior(requestContext, routeDef.ControllerName));

			// reset the friendly path so in the controllers and anything occuring after this point in time,
			//the URL is reset back to the original request.
			requestContext.HttpContext.RewritePath(UmbracoContext.OriginalRequestUrl.PathAndQuery);

            return new UmbracoMvcHandler(requestContext);
		}

        private SessionStateBehavior GetSessionStateBehavior(RequestContext requestContext, string controllerName)
        {
            return _controllerFactory.GetControllerSessionBehavior(requestContext, controllerName);
        }
	}
}