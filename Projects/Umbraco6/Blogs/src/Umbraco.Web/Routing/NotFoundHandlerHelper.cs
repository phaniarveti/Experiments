﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml;
using System.Reflection;

using Umbraco.Core;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Routing
{
	// provides internal access to legacy url -- should get rid of it eventually
    internal static class NotFoundHandlerHelper
    {
        const string ContextKey = "Umbraco.Web.Routing.NotFoundHandlerHelper.Url";

        static NotFoundHandlerHelper()
        {
            InitializeNotFoundHandlers();
        }

        public static string GetLegacyUrlForNotFoundHandlers()
        {
            // that's not backward-compatible because when requesting "/foo.aspx"
            // 4.9  : url = "foo.aspx"
            // 4.10 : url = "/foo"
            //return pcr.Uri.AbsolutePath;

            // so we have to run the legacy code for url preparation :-(

            var httpContext = HttpContext.Current;

            if (httpContext == null)
                return "";

            var url = httpContext.Items[ContextKey] as string;
            if (url != null)
                return url;

            // code from requestModule.UmbracoRewrite
            string tmp = httpContext.Request.Path.ToLower();

            // note: requestModule.UmbracoRewrite also did some stripping of &umbPage
            // from the querystring... that was in v3.x to fix some issues with pre-forms
            // auth. Paul Sterling confirmed in jan. 2013 that we can get rid of it.

            // code from requestHandler.cleanUrl
            string root = Umbraco.Core.IO.SystemDirectories.Root.ToLower();
            if (!string.IsNullOrEmpty(root) && tmp.StartsWith(root))
                tmp = tmp.Substring(root.Length);
            tmp = tmp.TrimEnd('/');
            if (tmp == "/default.aspx")
                tmp = string.Empty;
            else if (tmp == root)
                tmp = string.Empty;

            // code from UmbracoDefault.Page_PreInit
            if (tmp != "" && httpContext.Request["umbPageID"] == null)
            {
                string tryIntParse = tmp.Replace("/", "").Replace(".aspx", string.Empty);
                int result;
                if (int.TryParse(tryIntParse, out result))
                    tmp = tmp.Replace(".aspx", string.Empty);
            }
            else if (!string.IsNullOrEmpty(httpContext.Request["umbPageID"]))
            {
                int result;
                if (int.TryParse(httpContext.Request["umbPageID"], out result))
                {
                    tmp = httpContext.Request["umbPageID"];
                }
            }

            // code from requestHandler.ctor
            if (tmp != "")
                tmp = tmp.Substring(1);

            httpContext.Items[ContextKey] = tmp;
            return tmp;
        }

        static IEnumerable<Type> _customHandlerTypes = null;

        static void InitializeNotFoundHandlers()
        {
            // initialize handlers
            // create the definition cache

            LogHelper.Debug<LookupByNotFoundHandlers>("Registering custom handlers.");

            var customHandlerTypes = new List<Type>();

            var customHandlers = new XmlDocument();
            customHandlers.Load(Umbraco.Core.IO.IOHelper.MapPath(Umbraco.Core.IO.SystemFiles.NotFoundhandlersConfig));

            foreach (XmlNode n in customHandlers.DocumentElement.SelectNodes("notFound"))
            {
                var assemblyName = n.Attributes.GetNamedItem("assembly").Value;
                var typeName = n.Attributes.GetNamedItem("type").Value;

                string ns = assemblyName;
                var nsAttr = n.Attributes.GetNamedItem("namespace");
                if (nsAttr != null && !string.IsNullOrWhiteSpace(nsAttr.Value))
                    ns = nsAttr.Value;

                LogHelper.Debug<LookupByNotFoundHandlers>("Registering '{0}.{1},{2}'.", () => ns, () => typeName, () => assemblyName);

                Type type = null;
                try
                {
                    //TODO: This isn't a good way to load the assembly, its already in the Domain so we should be getting the type
                    // this loads the assembly into the wrong assembly load context!!

                    var assembly = Assembly.LoadFrom(Umbraco.Core.IO.IOHelper.MapPath(Umbraco.Core.IO.SystemDirectories.Bin + "/" + assemblyName + ".dll"));
                    type = assembly.GetType(ns + "." + typeName);
                }
                catch (Exception e)
                {
                    LogHelper.Error<LookupByNotFoundHandlers>("Error registering handler, ignoring.", e);
                }

                if (type != null)
                    customHandlerTypes.Add(type);
            }

            _customHandlerTypes = customHandlerTypes;
        }

        public static IEnumerable<Type> CustomHandlerTypes
        {
            get
            {
                return _customHandlerTypes;
            }
        }
    }
}
