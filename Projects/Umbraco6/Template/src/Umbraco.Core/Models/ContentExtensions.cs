﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Media;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Models
{
    public static class ContentExtensions
    {
        /// <summary>
        /// Set property values by alias with an annonymous object
        /// </summary>
        public static void PropertyValues(this IContent content, object value)
        {
            if (value == null)
                throw new Exception("No properties has been passed in");

            var propertyInfos = value.GetType().GetProperties();
            foreach (var propertyInfo in propertyInfos)
            {
                //Check if a PropertyType with alias exists thus being a valid property
                var propertyType = content.PropertyTypes.FirstOrDefault(x => x.Alias == propertyInfo.Name);
                if (propertyType == null)
                    throw new Exception(
                        string.Format(
                            "The property alias {0} is not valid, because no PropertyType with this alias exists",
                            propertyInfo.Name));

                //Check if a Property with the alias already exists in the collection thus being updated or inserted
                var item = content.Properties.FirstOrDefault(x => x.Alias == propertyInfo.Name);
                if (item != null)
                {
                    item.Value = propertyInfo.GetValue(value, null);
                    //Update item with newly added value
                    content.Properties.Add(item);
                }
                else
                {
                    //Create new Property to add to collection
                    var property = propertyType.CreatePropertyFromValue(propertyInfo.GetValue(value, null));
                    content.Properties.Add(property);
                }
            }
        }

        /// <summary>
        /// Sets and uploads the file from a HttpPostedFileBase object as the property value
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/> to add property value to</param>
        /// <param name="propertyTypeAlias">Alias of the property to save the value on</param>
        /// <param name="value">The <see cref="HttpPostedFileBase"/> containing the file that will be uploaded</param>
        public static void SetValue(this IContentBase content, string propertyTypeAlias, HttpPostedFileBase value)
        {
            // Ensure we get the filename without the path in IE in intranet mode 
            // http://stackoverflow.com/questions/382464/httppostedfile-filename-different-from-ie
            var fileName = value.FileName;
            if (fileName.LastIndexOf(@"\") > 0)
                fileName = fileName.Substring(fileName.LastIndexOf(@"\") + 1);

            var name =
                IOHelper.SafeFileName(
                    fileName.Substring(fileName.LastIndexOf(IOHelper.DirSepChar) + 1,
                                       fileName.Length - fileName.LastIndexOf(IOHelper.DirSepChar) - 1)
                            .ToLower());

            if (string.IsNullOrEmpty(name) == false)
                SetFileOnContent(content, propertyTypeAlias, name, value.InputStream);
        }

        /// <summary>
        /// Sets and uploads the file from a HttpPostedFile object as the property value
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/> to add property value to</param>
        /// <param name="propertyTypeAlias">Alias of the property to save the value on</param>
        /// <param name="value">The <see cref="HttpPostedFile"/> containing the file that will be uploaded</param>
        public static void SetValue(this IContentBase content, string propertyTypeAlias, HttpPostedFile value)
        {
            // Ensure we get the filename without the path in IE in intranet mode 
            // http://stackoverflow.com/questions/382464/httppostedfile-filename-different-from-ie
            var fileName = value.FileName;
            if (fileName.LastIndexOf(@"\") > 0)
                fileName = fileName.Substring(fileName.LastIndexOf(@"\") + 1);

            var name =
                IOHelper.SafeFileName(
                    fileName.Substring(fileName.LastIndexOf(IOHelper.DirSepChar) + 1,
                                       fileName.Length - fileName.LastIndexOf(IOHelper.DirSepChar) - 1)
                            .ToLower());

            if (string.IsNullOrEmpty(name) == false)
                SetFileOnContent(content, propertyTypeAlias, name, value.InputStream);
        }

        /// <summary>
        /// Sets and uploads the file from a HttpPostedFileWrapper object as the property value
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/> to add property value to</param>
        /// <param name="propertyTypeAlias">Alias of the property to save the value on</param>
        /// <param name="value">The <see cref="HttpPostedFileWrapper"/> containing the file that will be uploaded</param>
        public static void SetValue(this IContentBase content, string propertyTypeAlias, HttpPostedFileWrapper value)
        {
            // Ensure we get the filename without the path in IE in intranet mode 
            // http://stackoverflow.com/questions/382464/httppostedfile-filename-different-from-ie
            var fileName = value.FileName;
            if (fileName.LastIndexOf(@"\") > 0)
                fileName = fileName.Substring(fileName.LastIndexOf(@"\") + 1);

            var name = IOHelper.SafeFileName(fileName);

            if (string.IsNullOrEmpty(name) == false)
                SetFileOnContent(content, propertyTypeAlias, name, value.InputStream);
        }

        /// <summary>
        /// Sets and uploads the file from a <see cref="Stream"/> as the property value
        /// </summary>
        /// <param name="content"><see cref="IContentBase"/> to add property value to</param>
        /// <param name="propertyTypeAlias">Alias of the property to save the value on</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="fileStream"><see cref="Stream"/> to save to disk</param>
        public static void SetValue(this IContentBase content, string propertyTypeAlias, string fileName,
                                    Stream fileStream)
        {
            var name = IOHelper.SafeFileName(fileName);

            if (string.IsNullOrEmpty(name) == false && fileStream != null)
                SetFileOnContent(content, propertyTypeAlias, name, fileStream);
        }

        private static void SetFileOnContent(IContentBase content, string propertyTypeAlias, string name, Stream fileStream)
        {
            var property = content.Properties.FirstOrDefault(x => x.Alias == propertyTypeAlias);
            if (property == null)
                return;

            bool supportsResizing = false;
            var numberedFolder = MediaSubfolderCounter.Current.Increment();
            string fileName = UmbracoSettings.UploadAllowDirectories
                                              ? Path.Combine(numberedFolder.ToString(CultureInfo.InvariantCulture), name)
                                              : numberedFolder + "-" + name;

            string extension = Path.GetExtension(name).Substring(1).ToLowerInvariant();

            var fs = FileSystemProviderManager.Current.GetFileSystemProvider<MediaFileSystem>();
            fs.AddFile(fileName, fileStream);

            //Check if file supports resizing and create thumbnails
            if (("," + UmbracoSettings.ImageFileTypes + ",").Contains(string.Format(",{0},", extension)))
            {
                supportsResizing = true;

                // Make default thumbnail
                var thumbUrl = Resize(fs, fileName, extension, 100, "thumb");

                //Look up Prevalues for this upload datatype - if it is an upload datatype
                var uploadFieldId = new Guid("5032a6e6-69e3-491d-bb28-cd31cd11086c");
                if (property.PropertyType.DataTypeId == uploadFieldId)
                {
                    //Get Prevalues by the DataType's Id: property.PropertyType.DataTypeId
                    var values = ApplicationContext.Current.Services.DataTypeService.GetPreValuesByDataTypeId(property.PropertyType.DataTypeDefinitionId);
                    var thumbnailSizes = values.FirstOrDefault();
                    //Additional thumbnails configured as prevalues on the DataType
                    if (thumbnailSizes != null)
                    {
                        char sep = (!thumbnailSizes.Contains("") && thumbnailSizes.Contains(",")) ? ',' : ';';

                        foreach (string thumb in thumbnailSizes.Split(sep))
                        {
                            int thumbSize;
                            if (thumb != "" && int.TryParse(thumb, out thumbSize))
                            {
                                Resize(fs, fileName, extension, thumbSize, string.Format("thumb_{0}", thumbSize));
                            }
                        }
                    }
                }
            }

            //Check for auto fill of additional properties
            if (UmbracoSettings.ImageAutoFillImageProperties != null)
            {
                XmlNode uploadFieldConfigNode =
                    UmbracoSettings.ImageAutoFillImageProperties.SelectSingleNode(
                        string.Format("uploadField [@alias = \"{0}\"]", propertyTypeAlias));

                if (uploadFieldConfigNode != null)
                {
                    //Only add dimensions to web images
                    if (supportsResizing)
                    {
                        SetPropertyValue(content, uploadFieldConfigNode, "widthFieldAlias", GetDimensions(fs, fileName).Item1.ToString(CultureInfo.InvariantCulture));
                        SetPropertyValue(content, uploadFieldConfigNode, "heightFieldAlias", GetDimensions(fs, fileName).Item2.ToString(CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        SetPropertyValue(content, uploadFieldConfigNode, "widthFieldAlias", string.Empty);
                        SetPropertyValue(content, uploadFieldConfigNode, "heightFieldAlias", string.Empty);
                    }

                    SetPropertyValue(content, uploadFieldConfigNode, "lengthFieldAlias", fs.GetSize(fileName).ToString(CultureInfo.InvariantCulture));
                    SetPropertyValue(content, uploadFieldConfigNode, "extensionFieldAlias", extension);
                }
            }

            //Set the value of the property to that of the uploaded file's url
            property.Value = fs.GetUrl(fileName);
        }

        private static void SetPropertyValue(IContentBase content, XmlNode uploadFieldConfigNode, string propertyAlias, string propertyValue)
        {
            XmlNode propertyNode = uploadFieldConfigNode.SelectSingleNode(propertyAlias);
            if (propertyNode != null && string.IsNullOrEmpty(propertyNode.FirstChild.Value) == false && content.HasProperty(propertyNode.FirstChild.Value))
            {
                content.SetValue(propertyNode.FirstChild.Value, propertyValue);
            }
        }

        private static string Resize(MediaFileSystem fileSystem, string path, string extension, int maxWidthHeight, string fileNameAddition)
        {
            var fileNameThumb = DoResize(fileSystem, path, extension, GetDimensions(fileSystem, path).Item1, GetDimensions(fileSystem, path).Item2, maxWidthHeight, fileNameAddition);

            return fileSystem.GetUrl(fileNameThumb);
        }

        private static Tuple<int, int> GetDimensions(MediaFileSystem fileSystem, string path)
        {
            var fs = fileSystem.OpenFile(path);
            var image = Image.FromStream(fs);
            var fileWidth = image.Width;
            var fileHeight = image.Height;
            fs.Close();
            image.Dispose();

            return new Tuple<int, int>(fileWidth, fileHeight);
        }

        private static string DoResize(MediaFileSystem fileSystem, string path, string extension, int width, int height, int maxWidthHeight, string fileNameAddition)
        {
            var fs = fileSystem.OpenFile(path);
            var image = Image.FromStream(fs);
            fs.Close();

            string fileNameThumb = String.IsNullOrEmpty(fileNameAddition) ?
                string.Format("{0}_UMBRACOSYSTHUMBNAIL.jpg", path.Substring(0, path.LastIndexOf("."))) :
                string.Format("{0}_{1}.jpg", path.Substring(0, path.LastIndexOf(".")), fileNameAddition);

            var thumb = GenerateThumbnail(fileSystem,
                image,
                maxWidthHeight,
                width,
                height,
                path,
                extension,
                fileNameThumb,
                maxWidthHeight == 0);

            fileNameThumb = thumb.Item3;

            image.Dispose();

            return fileNameThumb;
        }

        private static Tuple<int, int, string> GenerateThumbnail(MediaFileSystem fileSystem, Image image, int maxWidthHeight, int fileWidth,
                                                int fileHeight, string fullFilePath, string extension,
                                                string thumbnailFileName, bool useFixedDimensions)
        {
            // Generate thumbnail
            float f = 1;
            if (!useFixedDimensions)
            {
                var fx = (float)image.Size.Width / (float)maxWidthHeight;
                var fy = (float)image.Size.Height / (float)maxWidthHeight;

                // must fit in thumbnail size
                f = Math.Max(fx, fy); //if (f < 1) f = 1;
            }

            var widthTh = (int)Math.Round((float)fileWidth / f); int heightTh = (int)Math.Round((float)fileHeight / f);

            // fixes for empty width or height
            if (widthTh == 0)
                widthTh = 1;
            if (heightTh == 0)
                heightTh = 1;

            // Create new image with best quality settings
            var bp = new Bitmap(widthTh, heightTh);
            var g = Graphics.FromImage(bp);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            // Copy the old image to the new and resized
            var rect = new Rectangle(0, 0, widthTh, heightTh);
            g.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);

            // Copy metadata
            var imageEncoders = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo codec = null;
            if (extension.ToLower() == "png" || extension.ToLower() == "gif")
                codec = imageEncoders.Single(t => t.MimeType.Equals("image/png"));
            else
                codec = imageEncoders.Single(t => t.MimeType.Equals("image/jpeg"));


            // Set compresion ratio to 90%
            var ep = new EncoderParameters();
            ep.Param[0] = new EncoderParameter(Encoder.Quality, 90L);

            // Save the new image using the dimensions of the image
            string newFileName = thumbnailFileName.Replace("UMBRACOSYSTHUMBNAIL",
                                                           string.Format("{0}x{1}", widthTh, heightTh));
            var ms = new MemoryStream();
            bp.Save(ms, codec, ep);
            ms.Seek(0, 0);

            fileSystem.AddFile(newFileName, ms);

            ms.Close();
            bp.Dispose();
            g.Dispose();

            return new Tuple<int, int, string>(widthTh, heightTh, newFileName);
        }

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Creator of this content/media item.
        /// </summary>
        public static IProfile GetCreatorProfile(this IContentBase content)
        {
            return ApplicationContext.Current.Services.UserService.GetProfileById(content.CreatorId);
        }

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Writer of this content.
        /// </summary>
        public static IProfile GetWriterProfile(this IContent content)
        {
            return ApplicationContext.Current.Services.UserService.GetProfileById(content.WriterId);
        }

        /// <summary>
        /// Checks whether an <see cref="IContent"/> item has any published versions
        /// </summary>
        /// <param name="content"></param>
        /// <returns>True if the content has any published versiom otherwise False</returns>
        public static bool HasPublishedVersion(this IContent content)
        {
            if (content.HasIdentity == false)
                return false;

            return ApplicationContext.Current.Services.ContentService.HasPublishedVersion(content.Id);
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="IContent"/> object
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to generate xml for</param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IContent content)
        {
            //nodeName should match Casing.SafeAliasWithForcingCheck(content.ContentType.Alias);
            var nodeName = UmbracoSettings.UseLegacyXmlSchema ? "node" : content.ContentType.Alias.ToSafeAliasWithForcingCheck();

            var x = content.ToXml(nodeName);
            x.Add(new XAttribute("nodeType", content.ContentType.Id));
            x.Add(new XAttribute("creatorName", content.GetCreatorProfile().Name));
            x.Add(new XAttribute("writerName", content.GetWriterProfile().Name));
            x.Add(new XAttribute("writerID", content.WriterId));
            x.Add(new XAttribute("template", content.Template == null ? "0" : content.Template.Id.ToString()));
            if (UmbracoSettings.UseLegacyXmlSchema)
            {
                x.Add(new XAttribute("nodeTypeAlias", content.ContentType.Alias));
            }

            return x;
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="IMedia"/> object
        /// </summary>
        /// <param name="media"><see cref="IContent"/> to generate xml for</param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IMedia media)
        {
            //nodeName should match Casing.SafeAliasWithForcingCheck(content.ContentType.Alias);
            var nodeName = UmbracoSettings.UseLegacyXmlSchema ? "node" : media.ContentType.Alias.ToSafeAliasWithForcingCheck();

            var x = media.ToXml(nodeName);
            x.Add(new XAttribute("nodeType", media.ContentType.Id));
            x.Add(new XAttribute("writerName", media.GetCreatorProfile().Name));
            x.Add(new XAttribute("writerID", media.CreatorId));
            x.Add(new XAttribute("version", media.Version));
            x.Add(new XAttribute("template", 0));
            if (UmbracoSettings.UseLegacyXmlSchema)
            {
                x.Add(new XAttribute("nodeTypeAlias", media.ContentType.Alias));
            }

            return x;
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="IContentBase"/> object
        /// </summary>
        /// <param name="contentBase"><see cref="IContent"/> to generate xml for</param>
        /// <param name="nodeName"></param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        private static XElement ToXml(this IContentBase contentBase, string nodeName)
        {
            var niceUrl = contentBase.Name.FormatUrl().ToLower();

            var xml = new XElement(nodeName,
                                   new XAttribute("id", contentBase.Id),
                                   new XAttribute("parentID", contentBase.Level > 1 ? contentBase.ParentId : -1),
                                   new XAttribute("level", contentBase.Level),
                                   new XAttribute("creatorID", contentBase.CreatorId),
                                   new XAttribute("sortOrder", contentBase.SortOrder),
                                   new XAttribute("createDate", contentBase.CreateDate.ToString("s")),
                                   new XAttribute("updateDate", contentBase.UpdateDate.ToString("s")),
                                   new XAttribute("nodeName", contentBase.Name),
                                   new XAttribute("urlName", niceUrl),//Format Url ?								   
                                   new XAttribute("path", contentBase.Path),
                                   new XAttribute("isDoc", ""));

            foreach (var property in contentBase.Properties)
            {
                if (property == null) continue;

                xml.Add(property.ToXml());

                //Check for umbracoUrlName convention
                if (property.Alias == "umbracoUrlName" && property.Value != null && 
                        property.Value.ToString().Trim() != string.Empty)
                    xml.SetAttributeValue("urlName", property.Value.ToString().FormatUrl().ToLower());
            }

            return xml;
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="IContent"/> object
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to generate xml for</param>
        /// <param name="isPreview">Boolean indicating whether the xml should be generated for preview</param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IContent content, bool isPreview)
        {
            //TODO Do a proper implementation of this
            //If current IContent is published we should get latest unpublished version
            return content.ToXml();
        }
    }
}