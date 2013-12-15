﻿using System;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Configuration
{
    internal class ClientDependencyConfiguration
    {
        private readonly string _fileName;

        public ClientDependencyConfiguration()
        {
            _fileName = IOHelper.MapPath(string.Format("{0}/ClientDependency.config", SystemDirectories.Config));
        }

        /// <summary>
        /// Increases the version number in ClientDependency.config by 1
        /// </summary>
        internal bool IncreaseVersionNumber()
        {
            try
            {
                var clientDependencyConfigXml = XDocument.Load(_fileName, LoadOptions.PreserveWhitespace);
                if (clientDependencyConfigXml.Root != null)
                {

                    var versionAttribute = clientDependencyConfigXml.Root.Attribute("version");

                    int oldVersion;
                    int.TryParse(versionAttribute.Value, out oldVersion);
                    var newVersion = oldVersion + 1;

                    versionAttribute.SetValue(newVersion);
                    clientDependencyConfigXml.Save(_fileName, SaveOptions.DisableFormatting);

                    LogHelper.Info<ClientDependencyConfiguration>(string.Format("Updated version number from {0} to {1}", oldVersion, newVersion));
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<ClientDependencyConfiguration>("Couldn't update ClientDependency version number", ex);
            }

            return false;
        }
    }
}