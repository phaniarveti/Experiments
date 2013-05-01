﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Umbraco.Core.Configuration;

namespace Umbraco.Core
{
	/// <summary>
	/// Extension methods for UrlHelper
	/// </summary>
	public static class UrlHelperExtensions
	{
		/// <summary>
		/// Returns the base path (not including the 'action') of the MVC controller "SaveFileController"
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string GetSaveFileServicePath(this UrlHelper url)
		{
			var result = url.Action("SavePartialView", "SaveFile", new {area = GlobalSettings.UmbracoMvcArea});
			return result.TrimEnd("SavePartialView");
		}

	}
}
