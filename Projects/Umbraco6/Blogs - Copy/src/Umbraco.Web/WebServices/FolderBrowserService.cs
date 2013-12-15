﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using Umbraco.Core;
using Umbraco.Web.Media.ThumbnailProviders;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.IO;
using umbraco.cms.businesslogic.Tags;
using umbraco.cms.businesslogic.media;
using Umbraco.Web.BaseRest;

namespace Umbraco.Web.WebServices
{
	//TODO: Can we convert this to MVC please instead of /base?

    [RestExtension("FolderBrowserService")]
    public class FolderBrowserService
    {
        [RestExtensionMethod(ReturnXml = false)]
        public static string GetChildren(int parentId)
        {
            var parentMedia = new global::umbraco.cms.businesslogic.media.Media(parentId);
            var currentUser = User.GetCurrent();
            var data = new List<object>();

            // Check user is logged in
            if (currentUser == null)
                throw new UnauthorizedAccessException("You must be logged in to use this service");

            // Check user is allowed to access selected media item
            if(!("," + parentMedia.Path + ",").Contains("," + currentUser.StartMediaId + ","))
                throw new UnauthorizedAccessException("You do not have access to this Media node");

            // Get children and filter
            //TODO: Only fetch files, not containers
            //TODO: Cache responses to speed up susequent searches
            foreach (var child in parentMedia.Children)
            {
                var fileProp = child.getProperty("umbracoFile") ?? 
                    child.GenericProperties.FirstOrDefault(x =>
                        x.PropertyType.DataTypeDefinition.DataType.Id == new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c"));

                var fileUrl = fileProp != null ? fileProp.Value.ToString() : "";
                var thumbUrl = ThumbnailProvidersResolver.Current.GetThumbnailUrl(fileUrl);
                var item = new
                {
                    Id = child.Id,
                    Path = child.Path,
                    Name = child.Text,
                    Tags = string.Join(",", Tag.GetTags(child.Id).Select(x => x.TagCaption)),
                    MediaTypeAlias = child.ContentType.Alias,
                    EditUrl = string.Format("editMedia.aspx?id={0}", child.Id),
                    FileUrl = fileUrl,
                    ThumbnailUrl = !string.IsNullOrEmpty(thumbUrl)
                        ? thumbUrl
                        : IOHelper.ResolveUrl(SystemDirectories.Umbraco + "/images/thumbnails/" + child.ContentType.Thumbnail)
                };

                data.Add(item);
            }

            return new JavaScriptSerializer().Serialize(data);
        }

        [RestExtensionMethod(ReturnXml = false)]
        public static string Delete(string nodeIds)
        {
            var nodeIdParts = nodeIds.Split(',');

            foreach (var nodeIdPart in nodeIdParts.Where(x => !string.IsNullOrEmpty(x)))
            {
                var nodeId = 0;
                if (!Int32.TryParse(nodeIdPart, out nodeId)) 
                    continue;
                
                var node = new global::umbraco.cms.businesslogic.media.Media(nodeId);
                node.delete(("," + node.Path + ",").Contains(",-21,"));
            }

            return new JavaScriptSerializer().Serialize(new
            {
                success = true
            });
        }
    }
}
