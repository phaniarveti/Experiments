﻿using System;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Web;
using Umbraco.Core.Media;
using umbraco;

namespace Umbraco.Web.Media
{
    public class ImageUrl
    {
        public static string GetImageUrl(string specifiedSrc, string field, string provider, string parameters, int? nodeId = null)
        {
            string url;

            var imageUrlProvider = GetProvider(provider);

            var parsedParameters = string.IsNullOrEmpty(parameters) ? new NameValueCollection() : HttpUtility.ParseQueryString(parameters);

            var queryValues = parsedParameters.Keys.Cast<string>().ToDictionary(key => key, key => parsedParameters[key]);

            if (string.IsNullOrEmpty(field))
            {
                url = imageUrlProvider.GetImageUrlFromFileName(specifiedSrc, queryValues);
            }
            else
            {
                var fieldValue = string.Empty;
                if (nodeId.HasValue)
                {
                    var contentFromCache = GetContentFromCache(nodeId.GetValueOrDefault(), field);
                    if (contentFromCache != null)
                    {
                        fieldValue = contentFromCache.ToString();
                    }
                    else
                    {
                        var itemPage = new page(content.Instance.XmlContent.GetElementById(nodeId.GetValueOrDefault().ToString(CultureInfo.InvariantCulture)));
                        var value = itemPage.Elements[field];
                        fieldValue = value != null ? value.ToString() : string.Empty;
                    }
                }
                else
                {
                    var context = HttpContext.Current;
                    if (context != null)
                    {
                        var elements = context.Items["pageElements"] as Hashtable;
                        if (elements != null)
                        {
                            var value = elements[field];
                            fieldValue = value != null ? value.ToString() : string.Empty;
                        }
                    }
                }

                int mediaId;
                url = int.TryParse(fieldValue, out mediaId)
                          ? imageUrlProvider.GetImageUrlFromMedia(mediaId, queryValues)
                          : imageUrlProvider.GetImageUrlFromFileName(fieldValue, queryValues);
            }

            return url;
        }

        private static IImageUrlProvider GetProvider(string provider)
        {
            return ImageUrlProviderResolver.Current.GetProvider(provider);
        }

        private static object GetContentFromCache(int nodeIdInt, string field)
        {
            var context = HttpContext.Current;
            
            if (context == null)
                return string.Empty;
            
            var content = context.Cache[String.Format("contentItem{0}_{1}", nodeIdInt.ToString(CultureInfo.InvariantCulture), field)];
            return content;
        }
    }
}