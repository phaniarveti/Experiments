using System.Diagnostics;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using umbraco.interfaces;
using Umbraco.Core;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides an implementation of <see cref="IPublishedContentLookup"/> that handles page nice urls.
	/// </summary>
	/// <remarks>
	/// <para>Handles <c>/foo/bar</c> where <c>/foo/bar</c> is the nice url of a document.</para>
	/// </remarks>
	//[ResolutionWeight(10)]
	internal class LookupByNiceUrl : IPublishedContentLookup
    {
	    private readonly bool _doDomainLookup;

	    public LookupByNiceUrl()
	    {
	        _doDomainLookup = true;
	    }

        /// <summary>
        /// Constructor to specify whether we also want to lookup domains stored in the repository
        /// </summary>
        /// <param name="doDomainLookup"></param>
	    internal LookupByNiceUrl(bool doDomainLookup)
	    {
	        _doDomainLookup = doDomainLookup;
	    }

	    /// <summary>
		/// Tries to find and assign an Umbraco document to a <c>PublishedContentRequest</c>.
		/// </summary>
		/// <param name="docRequest">The <c>PublishedContentRequest</c>.</param>		
		/// <returns>A value indicating whether an Umbraco document was found and assigned.</returns>
		public virtual bool TrySetDocument(PublishedContentRequest docRequest)
        {
			string route;
			if (docRequest.HasDomain)
				route = docRequest.Domain.RootNodeId.ToString() + DomainHelper.PathRelativeToDomain(docRequest.DomainUri, docRequest.Uri.GetAbsolutePathDecoded());
			else
				route = docRequest.Uri.GetAbsolutePathDecoded();

			var node = LookupDocumentNode(docRequest, route);
            return node != null;
        }

		/// <summary>
		/// Tries to find an Umbraco document for a <c>PublishedContentRequest</c> and a route.
		/// </summary>
		/// <param name="docreq">The document request.</param>
		/// <param name="route">The route.</param>
		/// <returns>The document node, or null.</returns>
        protected IPublishedContent LookupDocumentNode(PublishedContentRequest docreq, string route)
        {
			LogHelper.Debug<LookupByNiceUrl>("Test route \"{0}\"", () => route);

			// first ask the cache for a node
			// return '0' if in preview mode
        	var nodeId = !docreq.RoutingContext.UmbracoContext.InPreviewMode
							? docreq.RoutingContext.UmbracoContext.RoutesCache.GetNodeId(route)
        	             	: 0;

			// if a node was found, get it by id and ensure it exists
			// else clear the cache
            IPublishedContent node = null;
            if (nodeId > 0)
            {
				node = docreq.RoutingContext.PublishedContentStore.GetDocumentById(
					docreq.RoutingContext.UmbracoContext,
					nodeId);

                if (node != null)
                {
                    docreq.PublishedContent = node;
					LogHelper.Debug<LookupByNiceUrl>("Cache hit, id={0}", () => nodeId);
                }
                else
                {
                    docreq.RoutingContext.UmbracoContext.RoutesCache.ClearNode(nodeId);
                }
            }

			// if we still have no node, get it by route
            if (node == null)
            {
				LogHelper.Debug<LookupByNiceUrl>("Cache miss, query");
				node = docreq.RoutingContext.PublishedContentStore.GetDocumentByRoute(
					docreq.RoutingContext.UmbracoContext,
					route);

                if (node != null)
                {
                    docreq.PublishedContent = node;
					LogHelper.Debug<LookupByNiceUrl>("Query matches, id={0}", () => docreq.DocumentId);

					var iscanon = _doDomainLookup && !DomainHelper.ExistsDomainInPath(docreq.Domain, node.Path);
					if (!iscanon)
						LogHelper.Debug<LookupByNiceUrl>("Non canonical url");

					// do not store if previewing or if non-canonical
					if (!docreq.RoutingContext.UmbracoContext.InPreviewMode && iscanon)
						docreq.RoutingContext.UmbracoContext.RoutesCache.Store(docreq.DocumentId, route);
                    
                }
                else
                {
					LogHelper.Debug<LookupByNiceUrl>("Query does not match");
                }
            }

            return node;
        }
    }
}