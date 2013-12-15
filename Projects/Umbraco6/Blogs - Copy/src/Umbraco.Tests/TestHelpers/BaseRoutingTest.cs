using System.Configuration;
using System.Linq;
using System.Web.Routing;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Tests.Stubs;
using Umbraco.Web;
using Umbraco.Web.Routing;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests.TestHelpers
{
	[TestFixture, RequiresSTA]
	public abstract class BaseRoutingTest : BaseWebTest
	{		
		/// <summary>
		/// Return a new RoutingContext
		/// </summary>
		/// <param name="url"></param>
		/// <param name="templateId">
		/// The template Id to insert into the Xml cache file for each node, this is helpful for unit testing with templates but you		 
		/// should normally create the template in the database with this id
		///</param>
		/// <param name="routeData"></param>
		/// <returns></returns>
		protected RoutingContext GetRoutingContext(string url, int templateId, RouteData routeData = null)
		{
			var umbracoContext = GetUmbracoContext(url, templateId, routeData);
			var contentStore = new DefaultPublishedContentStore();
			var niceUrls = new NiceUrlProvider(contentStore, umbracoContext);
			var routingContext = new RoutingContext(
				umbracoContext,
				Enumerable.Empty<IPublishedContentLookup>(),
				new FakeLastChanceLookup(),
				contentStore,
				niceUrls);

			//assign the routing context back to the umbraco context
			umbracoContext.RoutingContext = routingContext;

			return routingContext;
		}

		/// <summary>
		/// Return a new RoutingContext
		/// </summary>
		/// <param name="url"></param>
		/// <param name="template"></param>
		/// <param name="routeData"></param>
		/// <returns></returns>
		protected RoutingContext GetRoutingContext(string url, Template template, RouteData routeData = null)
		{
			return GetRoutingContext(url, template.Id, routeData);
		}

		/// <summary>
		/// Return a new RoutingContext that doesn't require testing based on template
		/// </summary>
		/// <param name="url"></param>
		/// <param name="routeData"></param>
		/// <returns></returns>
		protected RoutingContext GetRoutingContext(string url, RouteData routeData = null)
		{
			return GetRoutingContext(url, 1234, routeData);
		}

		
	}
}