﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Umbraco.Core;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Provides utilities to handle domains.
	/// </summary>
	internal class DomainHelper
	{
		/// <summary>
		/// Represents an Umbraco domain and its normalized uri.
		/// </summary>
		/// <remarks>
		/// <para>In Umbraco it is valid to create domains with name such as <c>example.com</c>, <c>https://www.example.com</c>, <c>example.com/foo/</c>.</para>
		/// <para>The normalized uri of a domain begins with a scheme and ends with no slash, eg <c>http://example.com/</c>, <c>https://www.example.com/</c>, <c>http://example.com/foo/</c>.</para>
		/// </remarks>
		internal class DomainAndUri
		{
			/// <summary>
			/// The Umbraco domain.
			/// </summary>
			public Domain Domain;

			/// <summary>
			/// The normalized uri of the domain.
			/// </summary>
			public Uri Uri;

			/// <summary>
			/// Gets a string that represents the <see cref="DomainAndUri"/> instance.
			/// </summary>
			/// <returns>A string that represents the current <see cref="DomainAndUri"/> instance.</returns>
			public override string ToString()
			{
				return string.Format("{{ \"{0}\", \"{1}\" }}", Domain.Name, Uri);
			}
		}

		private static bool IsWildcardDomain(Domain d)
		{
			// supporting null or whitespace for backward compatibility, 
			// although we should not allow ppl to create them anymore
			return string.IsNullOrWhiteSpace(d.Name) || d.Name.StartsWith("*");
		}

		private static Domain SanitizeForBackwardCompatibility(Domain d)
		{
			// this is a _really_ nasty one that should be removed in 6.x
			// some people were using hostnames such as "/en" which happened to work pre-4.10
			// but make _no_ sense at all... and 4.10 throws on them, so here we just try
			// to find a way so 4.11 does not throw.
			// but, really.
			// no.
			var context = System.Web.HttpContext.Current;
			if (context != null && d.Name.StartsWith("/"))
			{
				// turn /en into http://whatever.com/en so it becomes a parseable uri
				var authority = context.Request.Url.GetLeftPart(UriPartial.Authority);
				d.Name = authority + d.Name;
			}
			return d;
		}

		/// <summary>
		/// Finds the domain that best matches the current uri, into an enumeration of domains.
		/// </summary>
		/// <param name="domains">The enumeration of Umbraco domains.</param>
		/// <param name="current">The uri of the current request, or null.</param>
		/// <param name="defaultToFirst">A value indicating whether to return the first domain of the list when no domain matches.</param>
		/// <returns>The domain and its normalized uri, that best matches the current uri, else the first domain (if <c>defaultToFirst</c> is <c>true</c>), else null.</returns>
		public static DomainAndUri DomainMatch(IEnumerable<Domain> domains, Uri current, bool defaultToFirst)
		{
			// sanitize the list to have proper uris for comparison (scheme, path end with /)
			// we need to end with / because example.com/foo cannot match example.com/foobar
			// we need to order so example.com/foo matches before example.com/
			var scheme = current == null ? Uri.UriSchemeHttp : current.Scheme;
			var domainsAndUris = domains
				.Where(d => !IsWildcardDomain(d))
				.Select(d => SanitizeForBackwardCompatibility(d))
				.Select(d => new { Domain = d, UriString = UriUtility.EndPathWithSlash(UriUtility.StartWithScheme(d.Name, scheme)) })
				.OrderByDescending(t => t.UriString)
				.Select(t => new DomainAndUri { Domain = t.Domain, Uri = new Uri(t.UriString) });

			if (!domainsAndUris.Any())
				return null;

			DomainAndUri domainAndUri;
			if (current == null)
			{
				// take the first one by default
				domainAndUri = domainsAndUris.First();
			}
			else
			{
				// look for a domain that would be the base of the hint
				// else take the first one by default
				var hintWithSlash = current.EndPathWithSlash();
				domainAndUri = domainsAndUris
					.FirstOrDefault(t => t.Uri.IsBaseOf(hintWithSlash));
				if (domainAndUri == null && defaultToFirst)
					domainAndUri = domainsAndUris.First();
			}

			if (domainAndUri != null)
				domainAndUri.Uri = domainAndUri.Uri.TrimPathEndSlash();
			return domainAndUri;
		}

		/// <summary>
		/// Gets an enumeration of <see cref="DomainAndUri"/> matching an enumeration of Umbraco domains.
		/// </summary>
		/// <param name="domains">The enumeration of Umbraco domains.</param>
		/// <param name="current">The uri of the current request, or null.</param>
		/// <returns>The enumeration of <see cref="DomainAndUri"/> matching the enumeration of Umbraco domains.</returns>
		public static IEnumerable<DomainAndUri> DomainMatches(IEnumerable<Domain> domains, Uri current)
		{
			var scheme = current == null ? Uri.UriSchemeHttp : current.Scheme;
			var domainsAndUris = domains
				.Where(d => !IsWildcardDomain(d))
				.Select(d => SanitizeForBackwardCompatibility(d))
				.Select(d => new { Domain = d, UriString = UriUtility.TrimPathEndSlash(UriUtility.StartWithScheme(d.Name, scheme)) })
				.OrderByDescending(t => t.UriString)
				.Select(t => new DomainAndUri { Domain = t.Domain, Uri = new Uri(t.UriString) });
			return domainsAndUris;
		}

		/// <summary>
		/// Gets a value indicating whether there is another domain defined down in the path to a node under the current domain's root node.
		/// </summary>
		/// <param name="current">The current domain.</param>
		/// <param name="path">The path to a node under the current domain's root node.</param>
		/// <returns>A value indicating if there is another domain defined down in the path.</returns>
		public static bool ExistsDomainInPath(Domain current, string path)
		{
			var domains = Domain.GetDomains();
			var stopNodeId = current == null ? -1 : current.RootNodeId;

			return path.Split(',')
				.Reverse()
				.Select(id => int.Parse(id))
				.TakeWhile(id => id != stopNodeId)
				.Any(id => domains.Any(d => d.RootNodeId == id && !IsWildcardDomain(d)));
		}

		/// <summary>
		/// Gets the deepest wildcard <see cref="Domain"/> in a node path.
		/// </summary>
		/// <param name="domains">The enumeration of Umbraco domains.</param>
		/// <param name="path">The node path.</param>
		/// <param name="rootNodeId">The current domain root node identifier, or null.</param>
		/// <returns>The deepest wildcard <see cref="Domain"/> in the path, or null.</returns>
		public static Domain LookForWildcardDomain(IEnumerable<Domain> domains, string path, int? rootNodeId)
		{
			var nodeIds = path.Split(',').Select(p => int.Parse(p)).Skip(1).Reverse();

			foreach (var nodeId in nodeIds)
			{
				var domain = domains.Where(d => d.RootNodeId == nodeId && IsWildcardDomain(d)).FirstOrDefault();
				if (domain != null)
					return domain;

				// stop at current domain root if any
				// "When you perform comparisons with nullable types, if the value of one of the nullable
				// types is null and the other is not, all comparisons evaluate to false."
				if (nodeId == rootNodeId)
					break;
			}

			return null;
		}

		/// <summary>
		/// Returns the part of a path relative to the uri of a domain.
		/// </summary>
		/// <param name="domainUri">The normalized uri of the domain.</param>
		/// <param name="path">The full path of the uri.</param>
		/// <returns>The path part relative to the uri of the domain.</returns>
		/// <remarks>Eg the relative part of <c>/foo/bar/nil</c> to domain <c>example.com/foo</c> is <c>/bar/nil</c>.</remarks>
		public static string PathRelativeToDomain(Uri domainUri, string path)
		{
			return path.Substring(domainUri.AbsolutePath.Length).EnsureStartsWith('/');
		}
	}
}
