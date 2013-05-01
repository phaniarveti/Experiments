﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;
using System.Configuration;
using System.Web;
using System.Text.RegularExpressions;
using System.Xml;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;

namespace Umbraco.Core.IO
{
	public static class IOHelper
    {
        private static string _rootDir = "";

        // static compiled regex for faster performance
        private readonly static Regex ResolveUrlPattern = new Regex("(=[\"\']?)(\\W?\\~(?:.(?![\"\']?\\s+(?:\\S+)=|[>\"\']))+.)[\"\']?", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

        public static char DirSepChar
        {
            get
            {
                return Path.DirectorySeparatorChar;
            }
        }

        //helper to try and match the old path to a new virtual one
        public static string FindFile(string virtualPath)
        {
            string retval = virtualPath;

            if (virtualPath.StartsWith("~"))
                retval = virtualPath.Replace("~", SystemDirectories.Root);

            if (virtualPath.StartsWith("/") && !virtualPath.StartsWith(SystemDirectories.Root))
                retval = SystemDirectories.Root + "/" + virtualPath.TrimStart('/');

            return retval;
        }

        //Replaces tildes with the root dir
        public static string ResolveUrl(string virtualPath)
        {
            if (virtualPath.StartsWith("~"))
                return virtualPath.Replace("~", SystemDirectories.Root).Replace("//", "/");
            else
                return VirtualPathUtility.ToAbsolute(virtualPath, SystemDirectories.Root);
        }

		[Obsolete("Use Umbraco.Web.Templates.TemplateUtilities.ResolveUrlsFromTextString instead, this method on this class will be removed in future versions")]
        internal static string ResolveUrlsFromTextString(string text)
        {
            if (UmbracoSettings.ResolveUrlsFromTextString)
            {				
				using (var timer = DisposableTimer.DebugDuration(typeof(IOHelper), "ResolveUrlsFromTextString starting", "ResolveUrlsFromTextString complete"))
				{
					// find all relative urls (ie. urls that contain ~)
					var tags = ResolveUrlPattern.Matches(text);
					LogHelper.Debug(typeof(IOHelper), "After regex: " + timer.Stopwatch.ElapsedMilliseconds + " matched: " + tags.Count);
					foreach (Match tag in tags)
					{						
						string url = "";
						if (tag.Groups[1].Success)
							url = tag.Groups[1].Value;

						// The richtext editor inserts a slash in front of the url. That's why we need this little fix
						//                if (url.StartsWith("/"))
						//                    text = text.Replace(url, ResolveUrl(url.Substring(1)));
						//                else
						if (!String.IsNullOrEmpty(url))
						{
							string resolvedUrl = (url.Substring(0, 1) == "/") ? ResolveUrl(url.Substring(1)) : ResolveUrl(url);
							text = text.Replace(url, resolvedUrl);
						}
					}
				}
            }
            return text;
        }

        public static string MapPath(string path, bool useHttpContext)
        {
            // Check if the path is already mapped
            if ((path.Length >= 2 && path[1] == Path.VolumeSeparatorChar)
                || path.StartsWith(@"\\")) //UNC Paths start with "\\". If the site is running off a network drive mapped paths will look like "\\Whatever\Boo\Bar"
            {
                return path;
            }
			// Check that we even have an HttpContext! otherwise things will fail anyways
			// http://umbraco.codeplex.com/workitem/30946

            if (useHttpContext && HttpContext.Current != null)
            {
                //string retval;
                if (!string.IsNullOrEmpty(path) && (path.StartsWith("~") || path.StartsWith(SystemDirectories.Root)))
                    return System.Web.Hosting.HostingEnvironment.MapPath(path);
                else
                    return System.Web.Hosting.HostingEnvironment.MapPath("~/" + path.TrimStart('/'));
            }

			//var root = (!string.IsNullOrEmpty(System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath)) 
			//    ? System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath.TrimEnd(IOHelper.DirSepChar) 
			//    : getRootDirectorySafe();

        	var root = GetRootDirectorySafe();
        	var newPath = path.TrimStart('~', '/').Replace('/', IOHelper.DirSepChar);
        	var retval = root + IOHelper.DirSepChar.ToString() + newPath;

        	return retval;
        }

        public static string MapPath(string path)
        {
            return MapPath(path, true);
        }

        //use a tilde character instead of the complete path
		internal static string ReturnPath(string settingsKey, string standardPath, bool useTilde)
        {
            string retval = ConfigurationManager.AppSettings[settingsKey];

            if (string.IsNullOrEmpty(retval))
                retval = standardPath;

            return retval.TrimEnd('/');
        }

        internal static string ReturnPath(string settingsKey, string standardPath)
        {
            return ReturnPath(settingsKey, standardPath, false);

        }

        /// <summary>
        /// Validates if the current filepath matches a directory where the user is allowed to edit a file
        /// </summary>
        /// <param name="filePath">filepath </param>
        /// <param name="validDir"></param>
        /// <returns>true if valid, throws a FileSecurityException if not</returns>
        internal static bool ValidateEditPath(string filePath, string validDir)
        {
            if (!filePath.StartsWith(MapPath(SystemDirectories.Root)))
                filePath = MapPath(filePath);
            if (!validDir.StartsWith(MapPath(SystemDirectories.Root)))
                validDir = MapPath(validDir);

            if (!filePath.StartsWith(validDir))
                throw new FileSecurityException(String.Format("The filepath '{0}' is not within an allowed directory for this type of files", filePath.Replace(MapPath(SystemDirectories.Root), "")));

            return true;
        }

        internal static bool ValidateEditPath(string filePath, IEnumerable<string> validDirs)
        {
            foreach (var dir in validDirs)
            {
                var validDir = dir;
                if (!filePath.StartsWith(MapPath(SystemDirectories.Root)))
                    filePath = MapPath(filePath);
                if (!validDir.StartsWith(MapPath(SystemDirectories.Root)))
                    validDir = MapPath(validDir);

                if (filePath.StartsWith(validDir))
                    return true;
            }

           throw new FileSecurityException(String.Format("The filepath '{0}' is not within an allowed directory for this type of files", filePath.Replace(MapPath(SystemDirectories.Root), "")));
        }

        internal static bool ValidateFileExtension(string filePath, List<string> validFileExtensions)
        {
            if (!filePath.StartsWith(MapPath(SystemDirectories.Root)))
                filePath = MapPath(filePath);
            var f = new FileInfo(filePath);


            if (!validFileExtensions.Contains(f.Extension.Substring(1)))
                throw new FileSecurityException(String.Format("The extension for the current file '{0}' is not of an allowed type for this editor. This is typically controlled from either the installed MacroEngines or based on configuration in /config/umbracoSettings.config", filePath.Replace(MapPath(SystemDirectories.Root), "")));

            return true;
        }


        /// <summary>
        /// Returns the path to the root of the application, by getting the path to where the assembly where this
        /// method is included is present, then traversing until it's past the /bin directory. Ie. this makes it work
        /// even if the assembly is in a /bin/debug or /bin/release folder
        /// </summary>
        /// <returns></returns>
        internal static string GetRootDirectorySafe()
        {
            if (!String.IsNullOrEmpty(_rootDir))
            {
                return _rootDir;
            }

			var codeBase = Assembly.GetExecutingAssembly().CodeBase;
			var uri = new Uri(codeBase);
			var path = uri.LocalPath;
        	var baseDirectory = Path.GetDirectoryName(path);
            _rootDir = baseDirectory.Substring(0, baseDirectory.LastIndexOf("bin") - 1);

            return _rootDir;

        }

        /// <summary>
        /// Check to see if filename passed has any special chars in it and strips them to create a safe filename.  Used to overcome an issue when Umbraco is used in IE in an intranet environment.
        /// </summary>
        /// <param name="filePath">The filename passed to the file handler from the upload field.</param>
        /// <returns>A safe filename without any path specific chars.</returns>
        internal static string SafeFileName(string filePath)
        {
            if (String.IsNullOrEmpty(filePath))
                return String.Empty;

            if (!String.IsNullOrWhiteSpace(filePath))
            {
                foreach (var character in Path.GetInvalidFileNameChars())
                {
                    filePath = filePath.Replace(character, '-');
                }
            }
            else
            {
                filePath = String.Empty;
            }

            //Break up the file in name and extension before applying the UrlReplaceCharacters
            var fileNamePart = filePath.Substring(0, filePath.LastIndexOf('.'));
            var ext = filePath.Substring(filePath.LastIndexOf('.'));

            //Because the file usually is downloadable as well we check characters against 'UmbracoSettings.UrlReplaceCharacters'
            XmlNode replaceChars = UmbracoSettings.UrlReplaceCharacters;
            foreach (XmlNode n in replaceChars.SelectNodes("char"))
            {
                if (n.Attributes.GetNamedItem("org") != null && n.Attributes.GetNamedItem("org").Value != "")
                    fileNamePart = fileNamePart.Replace(n.Attributes.GetNamedItem("org").Value, XmlHelper.GetNodeValue(n));
            }

            filePath = string.Concat(fileNamePart, ext);

            // Adapted from: http://stackoverflow.com/a/4827510/5018
            // Combined both Reserved Characters and Character Data 
            // from http://en.wikipedia.org/wiki/Percent-encoding
            var stringBuilder = new StringBuilder();

            const string reservedCharacters = "!*'();:@&=+$,/?%#[]-~{}\"<>\\^`| ";

            foreach (var character in filePath)
            {
                if (reservedCharacters.IndexOf(character) == -1)
                    stringBuilder.Append(character);
                else
                    stringBuilder.Append("-");
            }

            // Remove repeating dashes
            // From: http://stackoverflow.com/questions/5111967/regex-to-remove-a-specific-repeated-character
            var reducedString = Regex.Replace(stringBuilder.ToString(), "-+", "-");

            return reducedString;
        }
    }
}
