﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.ValueConverters;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    [PropertyValueType(typeof(JToken))]
    [PropertyValueCache(PropertyCacheValue.All, PropertyCacheLevel.Content)]
    public class RelatedLinksEditorValueConvertor : PropertyValueConverterBase
    {
        public override bool IsConverter(PublishedPropertyType propertyType)
        {
            return Constants.PropertyEditors.RelatedLinksAlias.Equals(propertyType.PropertyEditorAlias);
        }

        public override object ConvertDataToSource(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            if (sourceString.DetectIsJson())
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject(sourceString);
                    return obj;
                }
                catch (Exception ex)
                {
                    LogHelper.Error<JsonValueConverter>("Could not parse the string " + sourceString + " to a json object", ex);
                }
            }

            //it's not json, just return the string
            return sourceString;
        }

        public override object ConvertSourceToXPath(PublishedPropertyType propertyType, object source, bool preview)
        {
            if (source == null) return null;
            var sourceString = source.ToString();

            if (sourceString.DetectIsJson())
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<Array>(sourceString);

                    var d = new XmlDocument();
                    var e = d.CreateElement("links");
                    d.AppendChild(e);

                    var values = (IEnumerable<string>)source;
                    foreach (dynamic link in obj)
                    {
                        var ee = d.CreateElement("link");
                        ee.SetAttribute("title", link.title);
                        ee.SetAttribute("link", link.link);
                        ee.SetAttribute("type", link.type);
                        ee.SetAttribute("newwindow", link.newWindow);

                        e.AppendChild(ee);
                    }

                    return d.CreateNavigator();
                }
                catch (Exception ex)
                {
                    LogHelper.Error<JsonValueConverter>("Could not parse the string " + sourceString + " to a json object", ex);
                }
            }

            //it's not json, just return the string
            return sourceString;
        }
    }
}
