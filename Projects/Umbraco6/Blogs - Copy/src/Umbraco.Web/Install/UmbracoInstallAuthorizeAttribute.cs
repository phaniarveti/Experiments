using System;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using umbraco.BasePages;

namespace Umbraco.Web.Install
{
	/// <summary>
	/// Ensures authorization occurs for the installer if it has already completed. If install has not yet occured
	/// then the authorization is successful
	/// </summary>
	internal class UmbracoInstallAuthorizeAttribute : AuthorizeAttribute
	{
		private readonly ApplicationContext _applicationContext;

		public UmbracoInstallAuthorizeAttribute(ApplicationContext appContext)
		{
			if (appContext == null) throw new ArgumentNullException("appContext");
			_applicationContext = appContext;
		}

		public UmbracoInstallAuthorizeAttribute()
			: this(ApplicationContext.Current)
		{
			
		}

		/// <summary>
		/// Ensures that the user must be logged in or that the application is not configured just yet.
		/// </summary>
		/// <param name="httpContext"></param>
		/// <returns></returns>
		protected override bool AuthorizeCore(HttpContextBase httpContext)
		{
			if (httpContext == null)
			{
				throw new ArgumentNullException("httpContext");
			}

			try
			{
				//if its not configured then we can continue
				if (!_applicationContext.IsConfigured)
				{
					return true;
				}
				
				//otherwise we need to ensure that a user is logged in
				var isLoggedIn = BasePage.ValidateUserContextID(BasePage.umbracoUserContextID);
				if (isLoggedIn)
				{
					return true;
				}

				return false;			
			}
			catch (Exception)
			{
				return false;
			}
		}

		public override void OnAuthorization(AuthorizationContext filterContext)
		{
			Mandate.ParameterNotNull(filterContext, "filterContext");
			if (OutputCacheAttribute.IsChildActionCacheActive(filterContext))
				throw new InvalidOperationException("Cannot use UmbracoInstallAuthorizeAttribute on a child action");
			if (AuthorizeCore(filterContext.HttpContext))
			{
				//with a little help from dotPeek... this is what it normally would do
				var cache = filterContext.HttpContext.Response.Cache;
				cache.SetProxyMaxAge(new TimeSpan(0L));
				cache.AddValidationCallback(CacheValidateHandler, null);
			}
			else
			{
				//they aren't authorized but the app has installed
				throw new HttpException((int)global::System.Net.HttpStatusCode.Unauthorized,
				                        "You must login to view this resource.");
			}
		}

		private void CacheValidateHandler(HttpContext context, object data, ref HttpValidationStatus validationStatus)
		{
			validationStatus = OnCacheAuthorization(new HttpContextWrapper(context));
		}
	}
}