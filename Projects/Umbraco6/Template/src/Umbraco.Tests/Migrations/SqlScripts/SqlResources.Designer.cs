﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18010
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Umbraco.Tests.Migrations.SqlScripts {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class SqlResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal SqlResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Umbraco.Tests.Migrations.SqlScripts.SqlResources", typeof(SqlResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to /*******************************************************************************************
        ///
        ///
        ///
        ///
        ///
        ///
        ///
        ///    Umbraco database installation script for MySQL
        /// 
        ///IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT
        /// 
        ///    Database version: 4.8.0.4
        ///    
        ///    Please increment this version number if ANY change is made to this script,
        ///    so compatibility with scripts for other database systems can be verified easily.
        ///    The first 3 digits depict the Umbraco version, t [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string MySqlTotal_480 {
            get {
                return ResourceManager.GetString("MySqlTotal_480", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to -- Script Date: 10-01-2013 11:50  - Generated by ExportSqlCe version 3.5.2.18
        ///-- Database information:
        ///-- Locale Identifier: 1030
        ///-- Encryption Mode: 
        ///-- Case Sensitive: False
        ///-- Database: C:\Temp\Playground\Umb4110Starterkit\Umb4110Starterkit\App_Data\Umbraco.sdf
        ///-- ServerVersion: 4.0.8876.1
        ///-- DatabaseSize: 1114112
        ///-- Created: 10-01-2013 11:39
        ///
        ///-- User Table information:
        ///-- Number of tables: 43
        ///-- cmsContent: 12 row(s)
        ///-- cmsContentType: 10 row(s)
        ///-- cmsContentTypeAllowedContentType: 8 row(s [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SqlCe_SchemaAndData_4110 {
            get {
                return ResourceManager.GetString("SqlCe_SchemaAndData_4110", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CREATE TABLE [umbracoRelation] 
        ///( 
        ///[id] [int] NOT NULL IDENTITY(1, 1), 
        ///[parentId] [int] NOT NULL, 
        ///[childId] [int] NOT NULL, 
        ///[relType] [int] NOT NULL, 
        ///[datetime] [datetime] NOT NULL CONSTRAINT [DF_umbracoRelation_datetime] DEFAULT (getdate()), 
        ///[comment] [nvarchar] (1000)  NOT NULL 
        ///) 
        /// 
        ///; 
        ///ALTER TABLE [umbracoRelation] ADD CONSTRAINT [PK_umbracoRelation] PRIMARY KEY  ([id]) 
        ///; 
        ///CREATE TABLE [cmsDocument] 
        ///( 
        ///[nodeId] [int] NOT NULL, 
        ///[published] [bit] NOT NULL, 
        ///[documentUser] [int] NOT [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SqlCeTotal_480 {
            get {
                return ResourceManager.GetString("SqlCeTotal_480", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to /*******************************************************************************************
        ///
        ///
        ///
        ///
        ///
        ///
        ///
        ///    Umbraco database installation script for SQL Server
        /// 
        ///IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT
        /// 
        ///    Database version: 4.8.0.0
        ///    
        ///    Please increment this version number if ANY change is made to this script,
        ///    so compatibility with scripts for other database systems can be verified easily.
        ///    The first 3 digits depict the Umbraco versi [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string SqlServerTotal_480 {
            get {
                return ResourceManager.GetString("SqlServerTotal_480", resourceCulture);
            }
        }
    }
}
