using NUnit.Framework;
using Rhino.Mocks;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.template;

namespace Umbraco.Tests.Routing
{
	[TestFixture]
	public class LookupByPageIdQueryTests : BaseRoutingTest
	{
		/// <summary>
		/// We don't need a db for this test, will run faster without one
		/// </summary>
		protected override bool RequiresDbSetup
		{
			get { return false; }
		}

		[TestCase("/?umbPageId=1046", 1046)]
		[TestCase("/?UMBPAGEID=1046", 1046)]
		[TestCase("/default.aspx?umbPageId=1046", 1046)] //TODO: Should this match??
		[TestCase("/some/other/page?umbPageId=1046", 1046)] //TODO: Should this match??
		[TestCase("/some/other/page.aspx?umbPageId=1046", 1046)] //TODO: Should this match??
		public void Lookup_By_Page_Id(string urlAsString, int nodeMatch)
		{		
			var routingContext = GetRoutingContext(urlAsString);
			var url = routingContext.UmbracoContext.CleanedUmbracoUrl; //very important to use the cleaned up umbraco url
			var docRequest = new PublishedContentRequest(url, routingContext);
			var lookup = new LookupByPageIdQuery();			

			//we need to manually stub the return output of HttpContext.Request["umbPageId"]
			routingContext.UmbracoContext.HttpContext.Request.Stub(x => x["umbPageID"])
				.Return(routingContext.UmbracoContext.HttpContext.Request.QueryString["umbPageID"]);

			var result = lookup.TrySetDocument(docRequest);

			Assert.IsTrue(result);
			Assert.AreEqual(docRequest.DocumentId, nodeMatch);
		}
	}
}