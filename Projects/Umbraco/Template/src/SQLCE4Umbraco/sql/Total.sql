/*******************************************************************************************







    Umbraco database installation script for SQL Server CE 4
 
IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT
 
    Database version: 6.0.0.0
    
    Please increment this version number if ANY change is made to this script,
    so compatibility with scripts for other database systems can be verified easily.
    The first 3 digits depict the Umbraco version, the last digit is the database version.
    (e.g. version 4.0.0.3 means "Umbraco version 4.0.0, database version 3")
    
    Check-in policy: only commit this script if
     * you ran the Umbraco installer completely;
     * you ran it on the targetted database system;
     * you ran the Runway and Module installations;
     * you were able to browse the Boost site;
     * you were able to open the Umbraco administration panel;
     * you have documented the code change in this script;
     * you have incremented the version number in this script.
 
IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT IMPORTANT







********************************************************************************************/

CREATE TABLE [umbracoRelation] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[parentId] [int] NOT NULL, 
[childId] [int] NOT NULL, 
[relType] [int] NOT NULL, 
[datetime] [datetime] NOT NULL CONSTRAINT [DF_umbracoRelation_datetime] DEFAULT (getdate()), 
[comment] [nvarchar] (1000)  NOT NULL 
) 
 
; 
ALTER TABLE [umbracoRelation] ADD CONSTRAINT [PK_umbracoRelation] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [cmsDocument] 
( 
[nodeId] [int] NOT NULL, 
[published] [bit] NOT NULL, 
[documentUser] [int] NOT NULL, 
[versionId] [uniqueidentifier] NOT NULL, 
[text] [nvarchar] (255) NOT NULL, 
[releaseDate] [datetime] NULL, 
[expireDate] [datetime] NULL, 
[updateDate] [datetime] NOT NULL CONSTRAINT [DF_cmsDocument_updateDate] DEFAULT (getdate()), 
[templateId] [int] NULL, 
[alias] [nvarchar] (255)  NULL ,
[newest] [bit] NOT NULL CONSTRAINT [DF_cmsDocument_newest] DEFAULT (0)
) 
 
; 
ALTER TABLE [cmsDocument] ADD CONSTRAINT [PK_cmsDocument] PRIMARY KEY  ([versionId]) 
; 
CREATE TABLE [umbracoLog] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[userId] [int] NOT NULL, 
[NodeId] [int] NOT NULL, 
[Datestamp] [datetime] NOT NULL CONSTRAINT [DF_umbracoLog_Datestamp] DEFAULT (getdate()), 
[logHeader] [nvarchar] (50) NOT NULL, 
[logComment] [nvarchar] (1000)  NULL 
) 
 
; 
ALTER TABLE [umbracoLog] ADD CONSTRAINT [PK_umbracoLog] PRIMARY KEY  ([id]) 
; 

/* TABLES ARE NEVER USED, REMOVED FOR 4.1

CREATE TABLE [umbracoUserGroup] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[userGroupName] [nvarchar] (255) NOT NULL 
) 
; 
ALTER TABLE [umbracoUserGroup] ADD CONSTRAINT [PK_userGroup] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [umbracoUser2userGroup] 
( 
[user] [int] NOT NULL, 
[userGroup] [int] NOT NULL 
)  
; 
ALTER TABLE [umbracoUser2userGroup] ADD CONSTRAINT [PK_user2userGroup] PRIMARY KEY  ([user], [userGroup]) 
; 

*/

CREATE TABLE [umbracoApp] 
( 
[sortOrder] [tinyint] NOT NULL CONSTRAINT [DF_app_sortOrder] DEFAULT (0), 
[appAlias] [nvarchar] (50) NOT NULL, 
[appIcon] [nvarchar] (255) NOT NULL, 
[appName] [nvarchar] (255) NOT NULL, 
[appInitWithTreeAlias] [nvarchar] (255) NULL 
) 
 
; 
ALTER TABLE [umbracoApp] ADD CONSTRAINT [PK_umbracoApp] PRIMARY KEY  ([appAlias]) 
; 
CREATE TABLE [cmsPropertyData] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[contentNodeId] [int] NOT NULL, 
[versionId] [uniqueidentifier] NULL, 
[propertytypeid] [int] NOT NULL, 
[dataInt] [int] NULL, 
[dataDate] [datetime] NULL, 
[dataNvarchar] [nvarchar] (500) NULL, 
[dataNtext] [ntext] NULL 
) 
 
; 
ALTER TABLE [cmsPropertyData] ADD CONSTRAINT [PK_cmsPropertyData] PRIMARY KEY  ([id]) 
; 
CREATE INDEX [IX_cmsPropertyData] ON [cmsPropertyData] ([id]) 
; 
CREATE TABLE [cmsContent] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[nodeId] [int] NOT NULL, 
[contentType] [int] NOT NULL 
) 
 
; 
ALTER TABLE [cmsContent] ADD CONSTRAINT [PK_cmsContent] PRIMARY KEY  ([pk]) 
; 
CREATE TABLE [cmsContentType] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[nodeId] [int] NOT NULL, 
[alias] [nvarchar] (255) NULL, 
[icon] [nvarchar] (255) NULL,
[thumbnail] nvarchar(255) NOT NULL CONSTRAINT [DF_cmsContentType_thumbnail] DEFAULT ('folder.png'),
[isContainer] bit NOT NULL CONSTRAINT [DF_cmsContentType_isContainer] DEFAULT (0),
[allowAtRoot] bit NOT NULL  CONSTRAINT [DF_cmsContentType_allowAtRoot] DEFAULT (0)
) 
 
; 
ALTER TABLE [cmsContentType] ADD CONSTRAINT [PK_cmsContentType] PRIMARY KEY  ([pk]) 
; 
CREATE TABLE [cmsContentType2ContentType] ([parentContentTypeId] int NOT NULL,[childContentTypeId] int NOT NULL)
;
ALTER TABLE [cmsContentType2ContentType] ADD CONSTRAINT  [cmsContentType2ContentType_PK] PRIMARY KEY ([parentContentTypeId],[childContentTypeId])
;
CREATE TABLE [cmsMacroPropertyType] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[macroPropertyTypeAlias] [nvarchar] (50) NULL, 
[macroPropertyTypeRenderAssembly] [nvarchar] (255) NULL, 
[macroPropertyTypeRenderType] [nvarchar] (255) NULL, 
[macroPropertyTypeBaseType] [nvarchar] (255) NULL 
) 
 
; 
ALTER TABLE [cmsMacroPropertyType] ADD CONSTRAINT [PK_macroPropertyType] PRIMARY KEY  ([id]) 
; 

/* TABLE IS NEVER USED, REMOVED FOR 4.1 

CREATE TABLE [umbracoStylesheetProperty] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[stylesheetPropertyEditor] [bit] NOT NULL CONSTRAINT [DF_stylesheetProperty_stylesheetPropertyEditor] DEFAULT (0), 
[stylesheet] [int] NOT NULL, 
[stylesheetPropertyAlias] [nvarchar] (50) NULL, 
[stylesheetPropertyName] [nvarchar] (255) NULL, 
[stylesheetPropertyValue] [nvarchar] (400) NULL 
) 
; 

ALTER TABLE [umbracoStylesheetProperty] ADD CONSTRAINT [PK_stylesheetProperty] PRIMARY KEY  ([id]) 
;

*/
 
CREATE TABLE [cmsPropertyTypeGroup] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[contenttypeNodeId] [int] NOT NULL, 
[text] [nvarchar] (255) NOT NULL, 
[sortorder] [int] NOT NULL,
[parentGroupId] int NULL CONSTRAINT [DF_cmsPropertyTypeGroup_parentGroupId] DEFAULT NULL
) 
 
; 
ALTER TABLE [cmsPropertyTypeGroup] ADD CONSTRAINT [PK_cmsPropertyTypeGroup] PRIMARY KEY  ([id]) 
; 
ALTER TABLE [cmsPropertyTypeGroup] ADD CONSTRAINT [FK_cmsPropertyTypeGroup_cmsPropertyTypeGroup] FOREIGN KEY([parentGroupId]) REFERENCES [cmsPropertyTypeGroup] ([id])
; 
CREATE TABLE [cmsTemplate] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[nodeId] [int] NOT NULL, 
[master] [int] NULL, 
[alias] [nvarchar] (100) NULL, 
[design] [ntext]  NOT NULL 
) 
 
; 
ALTER TABLE [cmsTemplate] ADD CONSTRAINT [PK_templates] PRIMARY KEY  ([pk]) 
; 
CREATE TABLE [umbracoUser2app] 
( 
[user] [int] NOT NULL, 
[app] [nvarchar] (50) NOT NULL 
) 
 
; 
ALTER TABLE [umbracoUser2app] ADD CONSTRAINT [PK_user2app] PRIMARY KEY  ([user], [app]) 
; 
CREATE TABLE [umbracoUserType] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[userTypeAlias] [nvarchar] (50) NULL, 
[userTypeName] [nvarchar] (255) NOT NULL, 
[userTypeDefaultPermissions] [nvarchar] (50) NULL 
) 
 
; 
ALTER TABLE [umbracoUserType] ADD CONSTRAINT [PK_userType] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [umbracoUser] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[userDisabled] [bit] NOT NULL CONSTRAINT [DF_umbracoUser_userDisabled] DEFAULT (0), 
[userNoConsole] [bit] NOT NULL CONSTRAINT [DF_umbracoUser_userNoConsole] DEFAULT (0), 
[userType] [int] NOT NULL, 
[startStructureID] [int] NOT NULL, 
[startMediaID] [int] NULL, 
[userName] [nvarchar] (255) NOT NULL, 
[userLogin] [nvarchar] (125) NOT NULL, 
[userPassword] [nvarchar] (125) NOT NULL, 
[userEmail] [nvarchar] (255) NOT NULL, 
[userDefaultPermissions] [nvarchar] (50) NULL, 
[userLanguage] [nvarchar] (10) NULL 
) 
 
; 
ALTER TABLE [umbracoUser] ADD CONSTRAINT [PK_user] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [cmsDocumentType] 
( 
[contentTypeNodeId] [int] NOT NULL, 
[templateNodeId] [int] NOT NULL, 
[IsDefault] [bit] NOT NULL CONSTRAINT [DF_cmsDocumentType_IsDefault] DEFAULT (0) 
) 
 
; 
ALTER TABLE [cmsDocumentType] ADD CONSTRAINT [PK_cmsDocumentType] PRIMARY KEY  ([contentTypeNodeId], [templateNodeId]) 
; 
CREATE TABLE [cmsMemberType] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[NodeId] [int] NOT NULL, 
[propertytypeId] [int] NOT NULL, 
[memberCanEdit] [bit] NOT NULL CONSTRAINT [DF_cmsMemberType_memberCanEdit] DEFAULT (0), 
[viewOnProfile] [bit] NOT NULL CONSTRAINT [DF_cmsMemberType_viewOnProfile] DEFAULT (0) 
) 
 
; 
ALTER TABLE [cmsMemberType] ADD CONSTRAINT [PK_cmsMemberType] PRIMARY KEY  ([pk]) 
; 
CREATE TABLE [cmsMember] 
( 
[nodeId] [int] NOT NULL, 
[Email] [nvarchar] (1000) NOT NULL CONSTRAINT [DF_cmsMember_Email] DEFAULT (''), 
[LoginName] [nvarchar] (1000) NOT NULL CONSTRAINT [DF_cmsMember_LoginName] DEFAULT (''), 
[Password] [nvarchar] (1000) NOT NULL CONSTRAINT [DF_cmsMember_Password] DEFAULT ('') 
) 
 
; 
CREATE TABLE [umbracoNode] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[trashed] [bit] NOT NULL CONSTRAINT [DF_umbracoNode_trashed] DEFAULT (0), 
[parentID] [int] NOT NULL, 
[nodeUser] [int] NULL, 
[level] [int] NOT NULL, 
[path] [nvarchar] (150)  NOT NULL, 
[sortOrder] [int] NOT NULL, 
[uniqueID] [uniqueidentifier] NULL, 
[text] [nvarchar] (255) NULL, 
[nodeObjectType] [uniqueidentifier] NULL, 
[createDate] [datetime] NOT NULL CONSTRAINT [DF_umbracoNode_createDate] DEFAULT (getdate()) 
) 
 
; 
ALTER TABLE [umbracoNode] ADD CONSTRAINT [PK_structure] PRIMARY KEY  ([id]) 
; 
; 
CREATE TABLE [cmsPropertyType] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[dataTypeId] [int] NOT NULL, 
[contentTypeId] [int] NOT NULL, 
[propertyTypeGroupId] int,
[Alias] [nvarchar] (255) NOT NULL, 
[Name] [nvarchar] (255) NULL, 
[helpText] [nvarchar] (1000) NULL, 
[sortOrder] [int] NOT NULL CONSTRAINT [DF__cmsProper__sortO__1EA48E88] DEFAULT (0), 
[mandatory] [bit] NOT NULL CONSTRAINT [DF__cmsProper__manda__2180FB33] DEFAULT (0), 
[validationRegExp] [nvarchar] (255)  NULL, 
[Description] [nvarchar] (2000)  NULL 
) 
 
; 
ALTER TABLE [cmsPropertyType] ADD CONSTRAINT [PK_cmsPropertyType] PRIMARY KEY  ([id]) 
; 

CREATE TABLE [cmsMacroProperty] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[macroPropertyHidden] [bit] NOT NULL CONSTRAINT [DF_macroProperty_macroPropertyHidden] DEFAULT (0), 
[macroPropertyType] [int] NOT NULL, 
[macro] [int] NOT NULL, 
[macroPropertySortOrder] [tinyint] NOT NULL CONSTRAINT [DF_macroProperty_macroPropertySortOrder] DEFAULT (0), 
[macroPropertyAlias] [nvarchar] (50) NOT NULL, 
[macroPropertyName] [nvarchar] (255) NOT NULL 
) 
 
; 
ALTER TABLE [cmsMacroProperty] ADD CONSTRAINT [PK_macroProperty] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [cmsMacro] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[macroUseInEditor] [bit] NOT NULL CONSTRAINT [DF_macro_macroUseInEditor] DEFAULT (0), 
[macroRefreshRate] [int] NOT NULL CONSTRAINT [DF_macro_macroRefreshRate] DEFAULT (0), 
[macroAlias] [nvarchar] (255) NOT NULL, 
[macroName] [nvarchar] (255) NULL, 
[macroScriptType] [nvarchar] (255) NULL, 
[macroScriptAssembly] [nvarchar] (255) NULL, 
[macroXSLT] [nvarchar] (255) NULL, 
[macroCacheByPage] [bit] NOT NULL CONSTRAINT [DF_cmsMacro_macroCacheByPage] DEFAULT (1), 
[macroCachePersonalized] [bit] NOT NULL CONSTRAINT [DF_cmsMacro_macroCachePersonalized] DEFAULT (0), 
[macroDontRender] [bit] NOT NULL CONSTRAINT [DF_cmsMacro_macroDontRender] DEFAULT (0) 
) 
 
; 
ALTER TABLE [cmsMacro] ADD CONSTRAINT [PK_macro] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [cmsContentVersion] 
( 
[id] [int] NOT NULL IDENTITY(1, 1) PRIMARY KEY, 
[ContentId] [int] NOT NULL, 
[VersionId] [uniqueidentifier] NOT NULL, 
[VersionDate] [datetime] NOT NULL CONSTRAINT [DF_cmsContentVersion_VersionDate] DEFAULT (getdate()) 
) 
 
; 
CREATE TABLE [umbracoAppTree] 
( 
[treeSilent] [bit] NOT NULL CONSTRAINT [DF_umbracoAppTree_treeSilent] DEFAULT (0), 
[treeInitialize] [bit] NOT NULL CONSTRAINT [DF_umbracoAppTree_treeInitialize] DEFAULT (1), 
[treeSortOrder] [tinyint] NOT NULL, 
[appAlias] [nvarchar] (50) NOT NULL, 
[treeAlias] [nvarchar] (150) NOT NULL, 
[treeTitle] [nvarchar] (255) NOT NULL, 
[treeIconClosed] [nvarchar] (255) NOT NULL, 
[treeIconOpen] [nvarchar] (255) NOT NULL, 
[treeHandlerAssembly] [nvarchar] (255) NOT NULL, 
[treeHandlerType] [nvarchar] (255) NOT NULL,
[action] [nvarchar] (255) NULL  
) 
 
; 
ALTER TABLE [umbracoAppTree] ADD CONSTRAINT [PK_umbracoAppTree] PRIMARY KEY  ([appAlias], [treeAlias]) 
; 

CREATE TABLE [cmsContentTypeAllowedContentType] 
( 
[Id] [int] NOT NULL, 
[AllowedId] [int] NOT NULL,
[sortOrder] int NOT NULL CONSTRAINT [DF_cmsContentTypeAllowedContentType_sortOrder] DEFAULT (1)
) 
 
; 
ALTER TABLE [cmsContentTypeAllowedContentType] ADD CONSTRAINT [PK_cmsContentTypeAllowedContentType] PRIMARY KEY  ([Id], [AllowedId]) 
; 
CREATE TABLE [cmsContentXml] 
( 
[nodeId] [int] NOT NULL, 
[xml] [ntext]  NOT NULL 
) 
 
; 
ALTER TABLE [cmsContentXml] ADD CONSTRAINT [PK_cmsContentXml] PRIMARY KEY  ([nodeId]) 
; 
CREATE TABLE [cmsDataType] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[nodeId] [int] NOT NULL, 
[controlId] [uniqueidentifier] NOT NULL, 
[dbType] [nvarchar] (50) NOT NULL 
) 
 
; 
ALTER TABLE [cmsDataType] ADD CONSTRAINT [PK_cmsDataType] PRIMARY KEY  ([pk]) 
; 
CREATE TABLE [cmsDataTypePreValues] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[datatypeNodeId] [int] NOT NULL, 
[value] [nvarchar] (255) NULL, 
[sortorder] [int] NOT NULL, 
[alias] [nvarchar] (50)  NULL 
) 
 
; 
ALTER TABLE [cmsDataTypePreValues] ADD CONSTRAINT [PK_cmsDataTypePreValues] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [cmsDictionary] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[id] [uniqueidentifier] NOT NULL, 
[parent] [uniqueidentifier] NOT NULL, 
[key] [nvarchar] (1000)  NOT NULL 
) 
 
; 
ALTER TABLE [cmsDictionary] ADD CONSTRAINT [PK_cmsDictionary] PRIMARY KEY  ([pk]) 
; 
CREATE TABLE [cmsLanguageText] 
( 
[pk] [int] NOT NULL IDENTITY(1, 1), 
[languageId] [int] NOT NULL, 
[UniqueId] [uniqueidentifier] NOT NULL, 
[value] [nvarchar] (1000)  NOT NULL 
) 
 
; 
ALTER TABLE [cmsLanguageText] ADD CONSTRAINT [PK_cmsLanguageText] PRIMARY KEY  ([pk]) 
; 
CREATE TABLE [cmsMember2MemberGroup] 
( 
[Member] [int] NOT NULL, 
[MemberGroup] [int] NOT NULL 
) 
 
; 
ALTER TABLE [cmsMember2MemberGroup] ADD CONSTRAINT [PK_cmsMember2MemberGroup] PRIMARY KEY  ([Member], [MemberGroup]) 
; 
CREATE TABLE [cmsStylesheet] 
( 
[nodeId] [int] NOT NULL, 
[filename] [nvarchar] (100) NOT NULL, 
[content] [ntext] NULL 
) 
 
; 
CREATE TABLE [cmsStylesheetProperty] 
( 
[nodeId] [int] NOT NULL, 
[stylesheetPropertyEditor] [bit] NULL, 
[stylesheetPropertyAlias] [nvarchar] (50) NULL, 
[stylesheetPropertyValue] [nvarchar] (400) NULL 
) 
 
; 
CREATE TABLE [umbracoDomains] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[domainDefaultLanguage] [int] NULL, 
[domainRootStructureID] [int] NULL, 
[domainName] [nvarchar] (255) NOT NULL 
) 
 
; 
ALTER TABLE [umbracoDomains] ADD CONSTRAINT [PK_domains] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [umbracoLanguage] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[languageISOCode] [nvarchar] (10) NULL, 
[languageCultureName] [nvarchar] (100) NULL 
) 
 
; 
ALTER TABLE [umbracoLanguage] ADD CONSTRAINT [PK_language] PRIMARY KEY  ([id]) 
; 
CREATE TABLE [umbracoRelationType] 
( 
[id] [int] NOT NULL IDENTITY(1, 1), 
[dual] [bit] NOT NULL, 
[parentObjectType] [uniqueidentifier] NOT NULL, 
[childObjectType] [uniqueidentifier] NOT NULL, 
[name] [nvarchar] (255)  NOT NULL, 
[alias] [nvarchar] (100)  NULL 
) 
 
; 
ALTER TABLE [umbracoRelationType] ADD CONSTRAINT [PK_umbracoRelationType] PRIMARY KEY  ([id]) 
; 

/* TABLE IS NEVER USED, REMOVED FOR 4.1 

CREATE TABLE [umbracoStylesheet] 
( 
[nodeId] [int] NOT NULL, 
[filename] [nvarchar] (100) NOT NULL, 
[content] [ntext] NULL 
)  
; 

ALTER TABLE [umbracoStylesheet] ADD CONSTRAINT [PK_umbracoStylesheet] PRIMARY KEY  ([nodeId]) 
;

*/
 
CREATE TABLE [umbracoUser2NodeNotify] 
( 
[userId] [int] NOT NULL, 
[nodeId] [int] NOT NULL, 
[action] [nchar] (1)  NOT NULL 
) 
 
; 
ALTER TABLE [umbracoUser2NodeNotify] ADD CONSTRAINT [PK_umbracoUser2NodeNotify] PRIMARY KEY  ([userId], [nodeId], [action]) 
; 
CREATE TABLE [umbracoUser2NodePermission] 
( 
[userId] [int] NOT NULL, 
[nodeId] [int] NOT NULL, 
[permission] [nchar] (1) NOT NULL 
) 
 
; 
ALTER TABLE [umbracoUser2NodePermission] ADD CONSTRAINT [PK_umbracoUser2NodePermission] PRIMARY KEY  ([userId], [nodeId], [permission]) 
; 
CREATE TABLE [umbracoUserLogins] 
( 
[contextID] [uniqueidentifier] NOT NULL, 
[userID] [int] NOT NULL, 
[timeout] [bigint] NOT NULL 
) 
 
; 
/* 
ALTER TABLE [umbracoAppTree] ADD 
CONSTRAINT [FK_umbracoAppTree_umbracoApp] FOREIGN KEY ([appAlias]) REFERENCES [umbracoApp] ([appAlias]) 
; 
*/
ALTER TABLE [cmsPropertyData] ADD 
CONSTRAINT [FK_cmsPropertyData_umbracoNode] FOREIGN KEY ([contentNodeId]) REFERENCES [umbracoNode] ([id]) 
; 

/* TABLES ARE NEVER USED, REMOVED FOR 4.1 

ALTER TABLE [umbracoUser2userGroup] ADD 
CONSTRAINT [FK_user2userGroup_user] FOREIGN KEY ([user]) REFERENCES [umbracoUser] ([id]), 
CONSTRAINT [FK_user2userGroup_userGroup] FOREIGN KEY ([userGroup]) REFERENCES [umbracoUserGroup] ([id]) 
;

*/
 
ALTER TABLE [cmsDocument] ADD 
CONSTRAINT [FK_cmsDocument_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
; 
ALTER TABLE [cmsMacroProperty] ADD 
CONSTRAINT [FK_umbracoMacroProperty_umbracoMacroPropertyType] FOREIGN KEY ([macroPropertyType]) REFERENCES [cmsMacroPropertyType] ([id]) 
; 
ALTER TABLE [umbracoUser] ADD 
CONSTRAINT [FK_user_userType] FOREIGN KEY ([userType]) REFERENCES [umbracoUserType] ([id]) 
; 
ALTER TABLE [umbracoNode] ADD 
CONSTRAINT [FK_umbracoNode_umbracoNode] FOREIGN KEY ([parentID]) REFERENCES [umbracoNode] ([id]) 
; 
ALTER TABLE [cmsTemplate] ADD 
CONSTRAINT [FK_cmsTemplate_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
; 
ALTER TABLE [cmsContentType] ADD 
CONSTRAINT [FK_cmsContentType_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
; 
ALTER TABLE [cmsContent] ADD 
CONSTRAINT [FK_cmsContent_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
; 
ALTER TABLE [umbracoUser2app] ADD  
CONSTRAINT [FK_umbracoUser2app_umbracoUser] FOREIGN KEY ([user]) REFERENCES [umbracoUser] ([id]) 
; 

/* 
CONSTRAINT [FK_umbracoUser2app_umbracoApp] FOREIGN KEY ([app]) REFERENCES [umbracoApp] ([appAlias]),
*/
 
ALTER TABLE [cmsTemplate] DROP CONSTRAINT [FK_cmsTemplate_umbracoNode] 
; 
ALTER TABLE [cmsContent] DROP CONSTRAINT [FK_cmsContent_umbracoNode] 
; 
ALTER TABLE [cmsMacroProperty] DROP CONSTRAINT [FK_umbracoMacroProperty_umbracoMacroPropertyType] 
; 
/* 
ALTER TABLE [umbracoAppTree] DROP CONSTRAINT [FK_umbracoAppTree_umbracoApp] 
; 
ALTER TABLE [umbracoUser2app] DROP CONSTRAINT [FK_umbracoUser2app_umbracoApp] 
; 
*/
ALTER TABLE [umbracoUser2app] DROP CONSTRAINT [FK_umbracoUser2app_umbracoUser] 
; 
ALTER TABLE [cmsPropertyData] DROP CONSTRAINT [FK_cmsPropertyData_umbracoNode] 
; 

/* TABLE IS NEVER USED, REMOVED FOR 4.1

ALTER TABLE [umbracoUser2userGroup] DROP CONSTRAINT [FK_user2userGroup_user] 
; 
ALTER TABLE [umbracoUser2userGroup] DROP CONSTRAINT [FK_user2userGroup_userGroup] 
;

*/ 

ALTER TABLE [umbracoUser] DROP CONSTRAINT [FK_user_userType] 
; 
ALTER TABLE [cmsContentType] DROP CONSTRAINT [FK_cmsContentType_umbracoNode] 
; 
ALTER TABLE [cmsDocument] DROP CONSTRAINT [FK_cmsDocument_umbracoNode] 
; 
ALTER TABLE [umbracoNode] DROP CONSTRAINT [FK_umbracoNode_umbracoNode] 
; 
!!!SET IDENTITY_INSERT [umbracoNode] ON 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-92, 0, -1, 0, 11, N'-1,-92', 37, 'f0bc4bfb-b499-40d6-ba86-058885a5178c', N'Label', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20040930 14:01:49.920') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-90, 0, -1, 0, 11, N'-1,-90', 35, '84c6b441-31df-4ffe-b67e-67d5bc3ae65a', N'Upload', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20040930 14:01:49.920') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-89, 0, -1, 0, 11, N'-1,-89', 34, 'c6bac0dd-4ab9-45b1-8e30-e4b619ee5da3', N'Textbox multiple', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20040930 14:01:49.920') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-88, 0, -1, 0, 11, N'-1,-88', 33, '0cc0eba1-9960-42c9-bf9b-60e150b429ae', N'Textstring', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20040930 14:01:49.920') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-87, 0, -1, 0, 11, N'-1,-87', 32, 'ca90c950-0aff-4e72-b976-a30b1ac57dad', N'Richtext editor', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20040930 14:01:49.920') 
||INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-51, 0, -1, 0, 11, N'-1,-51', 4, '2e6d3631-066e-44b8-aec4-96f09099b2b5', N'Numeric', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20040930 14:01:49.920') 
||INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-49, 0, -1, 0, 11, N'-1,-49', 2, '92897bc6-a5f3-4ffe-ae27-f2e7e33dda49', N'True/false', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20040930 14:01:49.920') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-43, 0, -1, 0, 1, N'-1,-43', 2, 'fbaf13a8-4036-41f2-93a3-974f678c312a', N'Checkbox list', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20041015 14:11:04.367') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-42, 0, -1, 0, 1, N'-1,-42', 2, '0b6a45e7-44ba-430d-9da5-4e46060b9e03', N'Dropdown', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20041015 14:10:59.000') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-41, 0, -1, 0, 1, N'-1,-41', 2, '5046194e-4237-453c-a547-15db3a07c4e1', N'Date Picker', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20041015 14:10:54.303') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-40, 0, -1, 0, 1, N'-1,-40', 2, 'bb5f57c9-ce2b-4bb9-b697-4caca783a805', N'Radiobox', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20041015 14:10:49.253') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-39, 0, -1, 0, 1, N'-1,-39', 2, 'f38f0ac7-1d27-439c-9f3f-089cd8825a53', N'Dropdown multiple', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20041015 14:10:44.480') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-38, 0, -1, 0, 1, N'-1,-38', 2, 'fd9f1447-6c61-4a7c-9595-5aa39147d318', N'Folder Browser', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20041015 14:10:37.020') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-37, 0, -1, 0, 1, N'-1,-37', 2, '0225af17-b302-49cb-9176-b9f35cab9c17', N'Approved Color', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20041015 14:10:30.580') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-36, 0, -1, 0, 1, N'-1,-36', 2, 'e4d66c0f-b935-4200-81f0-025f7256b89a', N'Date Picker with time', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20041015 14:10:23.007') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (-1, 0, -1, 0, 0, N'-1', 0, '916724a5-173d-4619-b97e-b9de133dd6f5', N'SYSTEM DATA: umbraco master root', 'ea7d8624-4cfe-4578-a871-24aa946bf34d', '20040930 14:01:49.920') 
||INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1031, 0, -1, 1, 1, N'-1,1031', 2, 'f38bd2d7-65d0-48e6-95dc-87ce06ec2d3d', N'Folder', '4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e', '20041201 00:13:40.743') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1032, 0, -1, 1, 1, N'-1,1032', 2, 'cc07b313-0843-4aa8-bbda-871c8da728c8', N'Image', '4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e', '20041201 00:13:43.737') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1033, 0, -1, 1, 1, N'-1,1033', 2, '4c52d8ab-54e6-40cd-999c-7a5f24903e4d', N'File', '4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e', '20041201 00:13:46.210') 
||INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1034, 0, -1, 0, 1, N'-1,1034', 2, 'a6857c73-d6e9-480c-b6e6-f15f6ad11125', N'Content Picker', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20060103 13:07:29.203') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1035, 0, -1, 0, 1, N'-1,1035', 2, '93929b9a-93a2-4e2a-b239-d99334440a59', N'Media Picker', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20060103 13:07:36.143') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1036, 0, -1, 0, 1, N'-1,1036', 2, '2b24165f-9782-4aa3-b459-1de4a4d21f60', N'Member Picker', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20060103 13:07:40.260') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1038, 0, -1, 0, 1, N'-1,1038', 2, '1251c96c-185c-4e9b-93f4-b48205573cbd', N'Simple Editor', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20060103 13:07:55.250') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1039, 0, -1, 0, 1, N'-1,1039', 2, '06f349a9-c949-4b6a-8660-59c10451af42', N'Ultimate Picker', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20060103 13:07:55.250') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1040, 0, -1, 0, 1, N'-1,1040', 2, '21e798da-e06e-4eda-a511-ed257f78d4fa', N'Related Links', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20060103 13:07:55.250') 
|INSERT INTO [umbracoNode] ([id], [trashed], [parentID], [nodeUser], [level], [path], [sortOrder], [uniqueID], [text], [nodeObjectType], [createDate]) VALUES (1041, 0, -1, 0, 1, N'-1,1041', 2, 'b6b73142-b9c1-4bf8-a16d-e1c23320b549', N'Tags', '30a2a501-1978-4ddb-a57b-f7efed43ba3c', '20060103 13:07:55.250')
|SET IDENTITY_INSERT [umbracoNode] OFF 
;
!!!SET IDENTITY_INSERT [cmsContentType] ON 
|INSERT INTO [cmsContentType] ([pk], [nodeId], [alias], [icon], [thumbnail], [isContainer], [allowAtRoot]) VALUES (532, 1031, N'Folder', N'folder.gif', N'folder.png', 1, 1) 
|INSERT INTO [cmsContentType] ([pk], [nodeId], [alias], [icon], [thumbnail]) VALUES (533, 1032, N'Image', N'mediaPhoto.gif', N'mediaPhoto.png') 
|INSERT INTO [cmsContentType] ([pk], [nodeId], [alias], [icon], [thumbnail]) VALUES (534, 1033, N'File', N'mediaFile.gif', N'mediaFile.png') 
|SET IDENTITY_INSERT [cmsContentType] OFF 
;
!!!SET IDENTITY_INSERT [umbracoUser] ON 
|INSERT INTO [umbracoUser] ([id], [userDisabled], [userNoConsole], [userType], [startStructureID], [startMediaID], [userName], [userLogin], [userPassword], [userEmail], [userDefaultPermissions], [userLanguage]) VALUES (0, 0, 0, 1, -1, -1, N'Administrator', N'admin', N'default', N'', NULL, N'en') 
|SET IDENTITY_INSERT [umbracoUser] OFF 
;
!!!SET IDENTITY_INSERT [umbracoUserType] ON 
|INSERT INTO [umbracoUserType] ([id], [userTypeAlias], [userTypeName], [userTypeDefaultPermissions]) VALUES (1, N'admin', N'Administrators', N'CADMOSKTPIURZ:') 
|INSERT INTO [umbracoUserType] ([id], [userTypeAlias], [userTypeName], [userTypeDefaultPermissions]) VALUES (2, N'writer', N'Writer', N'CAH:') 
|INSERT INTO [umbracoUserType] ([id], [userTypeAlias], [userTypeName], [userTypeDefaultPermissions]) VALUES (3, N'editor', N'Editors', N'CADMOSKTPUZ:') 
|insert into umbracoUserType ([id], [userTypeAlias], [userTypeName], [userTypeDefaultPermissions]) values (4, N'translator', N'Translator', 'A')
|SET IDENTITY_INSERT [umbracoUserType] OFF 
;
!!!INSERT INTO [umbracoUser2app] ([user], [app]) VALUES (0, N'content') 
|INSERT INTO [umbracoUser2app] ([user], [app]) VALUES (0, N'developer') 
|INSERT INTO [umbracoUser2app] ([user], [app]) VALUES (0, N'media') 
|INSERT INTO [umbracoUser2app] ([user], [app]) VALUES (0, N'member') 
|INSERT INTO [umbracoUser2app] ([user], [app]) VALUES (0, N'settings') 
|INSERT INTO [umbracoUser2app] ([user], [app]) VALUES (0, N'users') 
;
/* 
|INSERT INTO [umbracoApp] ([appAlias], [sortOrder], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (N'content', 0, N'.traycontent', N'Indhold', N'content') 
|INSERT INTO [umbracoApp] ([appAlias], [sortOrder], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (N'developer', 7, N'.traydeveloper', N'Developer', NULL) 
|INSERT INTO [umbracoApp] ([appAlias], [sortOrder], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (N'media', 1, N'.traymedia', N'Mediearkiv', NULL) 
|INSERT INTO [umbracoApp] ([appAlias], [sortOrder], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (N'member', 8, N'.traymember', N'Medlemmer', NULL) 
|INSERT INTO [umbracoApp] ([appAlias], [sortOrder], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (N'settings', 6, N'.traysettings', N'Indstillinger', NULL) 
|INSERT INTO [umbracoApp] ([appAlias], [sortOrder], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (N'users', 5, N'.trayusers', N'Brugere', NULL)
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'content', N'content', 1, 1, 0, N'Indhold', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadContent') 
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'developer', N'cacheBrowser', 0, 1, 0, N'CacheBrowser', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadCache') 
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'developer', N'CacheItem', 0, 0, 0, N'Cachebrowser', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadCacheItem') 
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'developer', N'datatype', 0, 1, 1, N'Datatyper', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadDataTypes') 
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'developer', N'macros', 0, 1, 2, N'Macros', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMacros') 
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'developer', N'xslt', 0, 1, 5, N'XSLT Files', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadXslt') 
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'media', N'media', 0, 1, 0, N'Medier', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMedia') 
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'member', N'member', 0, 1, 0, N'Medlemmer', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMembers') 
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'member', N'memberGroup', 0, 1, 1, N'MemberGroups', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMemberGroups') 
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'member', N'memberType', 0, 1, 2, N'Medlemstyper', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMemberTypes') 
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType], [action]) VALUES (N'settings', N'dictionary', 0, 1, 3, N'Dictionary', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadDictionary', N'openDictionary()') 
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'settings', N'languages', 0, 1, 4, N'Languages', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadLanguages') 
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'settings', N'mediaTypes', 0, 1, 5, N'Medietyper', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadMediaTypes') 
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'settings', N'nodeTypes', 0, 1, 6, N'Dokumenttyper', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadNodeTypes') 
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'settings', N'stylesheetProperty', 0, 0, 0, N'Stylesheet Property', N'', N'', N'umbraco', N'loadStylesheetProperty') 
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'settings', N'stylesheets', 0, 1, 0, N'Stylesheets', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadStylesheets') 
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'settings', N'templates', 0, 1, 1, N'Templates', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadTemplates')
|INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'users', N'users', 0, 1, 0, N'Brugere', N'.sprTreeFolder', N'.sprTreeFolder_o', N'umbraco', N'loadUsers') 
;
*/
!!!SET IDENTITY_INSERT [cmsMacroPropertyType] ON 
|INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (3, N'mediaCurrent', N'umbraco.macroRenderings', N'media', N'Int32') 
|INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (4, N'contentSubs', N'umbraco.macroRenderings', N'content', N'Int32') 
|INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (5, N'contentRandom', N'umbraco.macroRenderings', N'content', N'Int32') 
|INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (6, N'contentPicker', N'umbraco.macroRenderings', N'content', N'Int32') 
|INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (13, N'number', N'umbraco.macroRenderings', N'numeric', N'Int32') 
|INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (14, N'bool', N'umbraco.macroRenderings', N'yesNo', N'Boolean') 
|INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (16, N'text', N'umbraco.macroRenderings', N'text', N'String') 
|INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (17, N'contentTree', N'umbraco.macroRenderings', N'content', N'Int32') 
|INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (18, N'contentType', N'umbraco.macroRenderings', N'contentTypeSingle', N'Int32') 
|INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (19, N'contentTypeMultiple', N'umbraco.macroRenderings', N'contentTypeMultiple', N'Int32') 
|INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (20, N'contentAll', N'umbraco.macroRenderings', N'content', N'Int32') 
|INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (21, N'tabPicker', N'umbraco.macroRenderings', N'tabPicker', N'String') 
|INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (22, N'tabPickerMultiple', N'umbraco.macroRenderings', N'tabPickerMultiple', N'String') 
|INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (23, N'propertyTypePicker', N'umbraco.macroRenderings', N'propertyTypePicker', N'String') 
|INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (24, N'propertyTypePickerMultiple', N'umbraco.macroRenderings', N'propertyTypePickerMultiple', N'String') 
|INSERT INTO [cmsMacroPropertyType] ([id], [macroPropertyTypeAlias], [macroPropertyTypeRenderAssembly], [macroPropertyTypeRenderType], [macroPropertyTypeBaseType]) VALUES (25, N'textMultiLine', N'umbraco.macroRenderings', N'textMultiple', N'String') 
|SET IDENTITY_INSERT [cmsMacroPropertyType] OFF 
;
!!!SET IDENTITY_INSERT [cmsPropertyTypeGroup] ON 
|INSERT INTO [cmsPropertyTypeGroup] ([id], [contenttypeNodeId], [text], [sortorder]) VALUES (3, 1032, N'Image', 1) 
|INSERT INTO [cmsPropertyTypeGroup] ([id], [contenttypeNodeId], [text], [sortorder]) VALUES (4, 1033, N'File', 1) 
|INSERT INTO [cmsPropertyTypeGroup] ([id], [contenttypeNodeId], [text], [sortorder]) VALUES (5, 1031, N'Contents', 1) 
|SET IDENTITY_INSERT [cmsPropertyTypeGroup] OFF 
;
!!!SET IDENTITY_INSERT [cmsPropertyType] ON 
|INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [propertyTypeGroupId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (6, -90, 1032, 3, N'umbracoFile', N'Upload image', NULL, 0, 0, NULL, NULL) 
|INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [propertyTypeGroupId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (7, -92, 1032, 3, N'umbracoWidth', N'Width', NULL, 0, 0, NULL, NULL) 
|INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [propertyTypeGroupId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (8, -92, 1032, 3, N'umbracoHeight', N'Height', NULL, 0, 0, NULL, NULL) 
|INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [propertyTypeGroupId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (9, -92, 1032, 3, N'umbracoBytes', N'Size', NULL, 0, 0, NULL, NULL) 
|INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [propertyTypeGroupId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (10, -92, 1032, 3, N'umbracoExtension', N'Type', NULL, 0, 0, NULL, NULL) 
|INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [propertyTypeGroupId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (24, -90, 1033, 4, N'umbracoFile', N'Upload file', NULL, 0, 0, NULL, NULL) 
|INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [propertyTypeGroupId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (25, -92, 1033, 4, N'umbracoExtension', N'Type', NULL, 0, 0, NULL, NULL) 
|INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [propertyTypeGroupId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (26, -92, 1033, 4, N'umbracoBytes', N'Size', NULL, 0, 0, NULL, NULL) 
|INSERT INTO [cmsPropertyType] ([id], [dataTypeId], [contentTypeId], [propertyTypeGroupId], [Alias], [Name], [helpText], [sortOrder], [mandatory], [validationRegExp], [Description]) VALUES (27, -38, 1031, 5, N'contents', N'Contents:', NULL, 0, 0, NULL, NULL) 
|SET IDENTITY_INSERT [cmsPropertyType] OFF 
;
!!!SET IDENTITY_INSERT [umbracoLanguage] ON 
|INSERT INTO [umbracoLanguage] ([id], [languageISOCode], [languageCultureName]) VALUES (1, N'en-US', N'en-US') 
|SET IDENTITY_INSERT [umbracoLanguage] OFF 
;
INSERT INTO [cmsContentTypeAllowedContentType] ([Id], [AllowedId]) VALUES (1031, 1031)
;
INSERT INTO [cmsContentTypeAllowedContentType] ([Id], [AllowedId]) VALUES (1031, 1032) 
;
INSERT INTO [cmsContentTypeAllowedContentType] ([Id], [AllowedId]) VALUES (1031, 1033) 
;
!!!SET IDENTITY_INSERT [cmsDataType] ON 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (4, -49, '38b352c1-e9f8-4fd8-9324-9a2eab06d97a', 'Integer') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (6, -51, '1413afcb-d19a-4173-8e9a-68288d2a73b8', 'Integer') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (8, -87, '5E9B75AE-FACE-41c8-B47E-5F4B0FD82F83', 'Ntext') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (9, -88, 'ec15c1e5-9d90-422a-aa52-4f7622c63bea', 'Nvarchar') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (10, -89, '67db8357-ef57-493e-91ac-936d305e0f2a', 'Ntext') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (11, -90, '5032a6e6-69e3-491d-bb28-cd31cd11086c', 'Nvarchar') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (12, -91, 'a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6', 'Nvarchar') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (13, -92, '6c738306-4c17-4d88-b9bd-6546f3771597', 'Nvarchar') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (14, -36, 'b6fb1622-afa5-4bbf-a3cc-d9672a442222', 'Date') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (15, -37, 'f8d60f68-ec59-4974-b43b-c46eb5677985', 'Nvarchar') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (16, -38, 'cccd4ae9-f399-4ed2-8038-2e88d19e810c', 'Nvarchar') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (17, -39, '928639ed-9c73-4028-920c-1e55dbb68783', 'Nvarchar') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (18, -40, 'a52c7c1c-c330-476e-8605-d63d3b84b6a6', 'Nvarchar') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (19, -41, '23e93522-3200-44e2-9f29-e61a6fcbb79a', 'Date') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (20, -42, 'a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6', 'Integer') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (21, -43, 'b4471851-82b6-4c75-afa4-39fa9c6a75e9', 'Nvarchar') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (22, -44, 'a3776494-0574-4d93-b7de-efdfdec6f2d1', 'Ntext') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (23, -128, 'a52c7c1c-c330-476e-8605-d63d3b84b6a6', 'Nvarchar') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (24, -129, '928639ed-9c73-4028-920c-1e55dbb68783', 'Nvarchar') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (25, -130, 'a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6', 'Nvarchar') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (26, -131, 'a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6', 'Nvarchar') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (27, -132, 'a74ea9c9-8e18-4d2a-8cf6-73c6206c5da6', 'Nvarchar') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (28, -133, '6c738306-4c17-4d88-b9bd-6546f3771597', 'Ntext') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (29, -134, '928639ed-9c73-4028-920c-1e55dbb68783', 'Nvarchar') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (30, -50, 'aaf99bb2-dbbe-444d-a296-185076bf0484', 'Date') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (31, 1034, '158aa029-24ed-4948-939e-c3da209e5fba', 'Integer') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (32, 1035, 'ead69342-f06d-4253-83ac-28000225583b', 'Integer') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (33, 1036, '39f533e4-0551-4505-a64b-e0425c5ce775', 'Integer') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (35, 1038, '60b7dabf-99cd-41eb-b8e9-4d2e669bbde9', 'Ntext') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (36, 1039, 'cdbf0b5d-5cb2-445f-bc12-fcaaec07cf2c', 'Ntext') 
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (37, 1040, '71b8ad1a-8dc2-425c-b6b8-faa158075e63', 'Ntext')
|INSERT INTO [cmsDataType] ([pk], [nodeId], [controlId], [dbType]) VALUES (38, 1041, '4023e540-92f5-11dd-ad8b-0800200c9a66', 'Ntext')
|SET IDENTITY_INSERT [cmsDataType] OFF 
;
ALTER TABLE [cmsTemplate] ADD CONSTRAINT [FK_cmsTemplate_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
;
ALTER TABLE [cmsContent] ADD CONSTRAINT [FK_cmsContent_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
;
ALTER TABLE [cmsMacroProperty] ADD CONSTRAINT [FK_umbracoMacroProperty_umbracoMacroPropertyType] FOREIGN KEY ([macroPropertyType]) REFERENCES [cmsMacroPropertyType] ([id]) 
;
/* 
ALTER TABLE [umbracoAppTree] ADD CONSTRAINT [FK_umbracoAppTree_umbracoApp] FOREIGN KEY ([appAlias]) REFERENCES [umbracoApp] ([appAlias]) 
;
ALTER TABLE [umbracoUser2app] ADD CONSTRAINT [FK_umbracoUser2app_umbracoApp] FOREIGN KEY ([app]) REFERENCES [umbracoApp] ([appAlias]) 
;
*/
ALTER TABLE [umbracoUser2app] ADD CONSTRAINT [FK_umbracoUser2app_umbracoUser] FOREIGN KEY ([user]) REFERENCES [umbracoUser] ([id]) 
;
ALTER TABLE [cmsPropertyData] ADD CONSTRAINT [FK_cmsPropertyData_umbracoNode] FOREIGN KEY ([contentNodeId]) REFERENCES [umbracoNode] ([id]) 
;
ALTER TABLE [umbracoUser] ADD CONSTRAINT [FK_user_userType] FOREIGN KEY ([userType]) REFERENCES [umbracoUserType] ([id]) 
;
ALTER TABLE [cmsContentType] ADD CONSTRAINT [FK_cmsContentType_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
;
ALTER TABLE [cmsDocument] ADD CONSTRAINT [FK_cmsDocument_umbracoNode] FOREIGN KEY ([nodeId]) REFERENCES [umbracoNode] ([id]) 
;
ALTER TABLE [umbracoNode] ADD CONSTRAINT [FK_umbracoNode_umbracoNode] FOREIGN KEY ([parentID]) REFERENCES [umbracoNode] ([id]) 
;
!!!set identity_insert umbracoNode on
|insert into umbracoNode (id, trashed, parentID, nodeUser, level, path, sortOrder, uniqueID, text, nodeObjectType) values (-20, 0, -1, 0, 0, '-1,-20', 0, '0F582A79-1E41-4CF0-BFA0-76340651891A', 'Recycle Bin', '01BB7FF2-24DC-4C0C-95A2-C24EF72BBAC8')
|set identity_insert umbracoNode off
;
ALTER TABLE cmsDataTypePreValues ALTER COLUMN value NVARCHAR(2500) NULL
;
CREATE TABLE [cmsTask]
(
[closed] [bit] NOT NULL CONSTRAINT [DF__cmsTask__closed__04E4BC85] DEFAULT ((0)),
[id] [int] NOT NULL IDENTITY(1, 1),
[taskTypeId] [int] NOT NULL,
[nodeId] [int] NOT NULL,
[parentUserId] [int] NOT NULL,
[userId] [int] NOT NULL,
[DateTime] [datetime] NOT NULL CONSTRAINT [DF__cmsTask__DateTim__05D8E0BE] DEFAULT (getdate()),
[Comment] [nvarchar] (500) NULL
)
;
ALTER TABLE [cmsTask] ADD CONSTRAINT [PK_cmsTask] PRIMARY KEY  ([id])
;
CREATE TABLE [cmsTaskType]
(
[id] [int] NOT NULL IDENTITY(1, 1),
[alias] [nvarchar] (255) NOT NULL
)
;
ALTER TABLE [cmsTaskType] ADD CONSTRAINT [PK_cmsTaskType] PRIMARY KEY  ([id])
;
ALTER TABLE [cmsTask] ADD
CONSTRAINT [FK_cmsTask_cmsTaskType] FOREIGN KEY ([taskTypeId]) REFERENCES [cmsTaskType] ([id])
;
insert into cmsTaskType (alias) values ('toTranslate')
;
/* Add send to translate actions to admins and editors */
update umbracoUserType set userTypeDefaultPermissions = userTypeDefaultPermissions + '5' where userTypeAlias in ('editor','admin')
;
/* Add translator usertype */
;
insert into umbracoRelationType (dual, parentObjectType, childObjectType, name, alias) values (1, 'c66ba18e-eaf3-4cff-8a22-41b16d66a972', 'c66ba18e-eaf3-4cff-8a22-41b16d66a972', 'Relate Document On Copy','relateDocumentOnCopy')
;
ALTER TABLE cmsMacro ADD macroPython nvarchar(255)
;
/* 
INSERT INTO [umbracoAppTree]([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES(0, 1, 4, 'developer', 'python', 'Python Files', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadPython') 
;
INSERT INTO [umbracoAppTree]([treeSilent], [treeInitialize], [treeSortOrder], [appAlias], [treeAlias], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES(0, 1, 2, 'settings', 'scripts', 'Scripts', 'folder.gif', 'folder_o.gif', 'umbraco', 'loadScripts') 
;
alter TABLE [cmsContentType]
add [thumbnail] nvarchar(255) NOT NULL CONSTRAINT
[DF_cmsContentType_thumbnail] DEFAULT ('folder.png')
;
*/
alter TABLE [cmsContentType]
add [description] nvarchar(1500) NULL
;
ALTER TABLE umbracoLog ALTER COLUMN logComment NVARCHAR(4000) NULL
;
!!!SET IDENTITY_INSERT [cmsDataTypePreValues] ON 
|insert into cmsDataTypePreValues (id, dataTypeNodeId, [value], sortorder, alias) values (3,-87,',code,undo,redo,cut,copy,mcepasteword,stylepicker,bold,italic,bullist,numlist,outdent,indent,mcelink,unlink,mceinsertanchor,mceimage,umbracomacro,mceinserttable,umbracoembed,mcecharmap,' + nchar(124) + '1' + nchar(124) + '1,2,3,' + nchar(124) + '0' + nchar(124) + '500,400' + nchar(124) + '1049,' + nchar(124) + 'true' + nchar(124) + '', 0, '')
|insert into cmsDataTypePreValues (id, dataTypeNodeId, [value], sortorder, alias) values (4,1041,'default', 0, 'group')
|SET IDENTITY_INSERT [cmsDataTypePreValues] OFF
;
/* 3.1 SQL changes 
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'developer', N'packager', 0, 1, 3, N'Packages', N'folder.gif', N'folder_o.gif', N'umbraco', N'loadPackager') 
;
INSERT INTO [umbracoAppTree] ([appAlias], [treeAlias], [treeSilent], [treeInitialize], [treeSortOrder], [treeTitle], [treeIconClosed], [treeIconOpen], [treeHandlerAssembly], [treeHandlerType]) VALUES (N'developer', N'packagerPackages', 0, 0, 1, N'Packager Packages', N'folder.gif', N'folder_o.gif', N'umbraco', N'loadPackages');
*/

/* Add ActionBrowse as a default permission to all user types that have ActionUpdate */
UPDATE umbracoUserType SET userTypeDefaultPermissions = userTypeDefaultPermissions + 'F' WHERE CHARINDEX('A',userTypeDefaultPermissions,0) >= 1
AND CHARINDEX('F',userTypeDefaultPermissions,0) < 1
;
/* Add ActionToPublish to all users types that have the alias 'writer' */
UPDATE umbracoUserType SET userTypeDefaultPermissions = userTypeDefaultPermissions + 'H' WHERE userTypeAlias = 'writer'
AND CHARINDEX('F',userTypeDefaultPermissions,0) < 1
;
/* Add ActionBrowse to all user permissions for nodes that have the ActionUpdate permission */
INSERT INTO umbracoUser2NodePermission (userID, nodeId, permission) 
SELECT userID, nodeId, 'F' FROM umbracoUser2NodePermission WHERE permission='A'
;
/* Add ActionToPublish permissions to all nodes for users that are of type 'writer' */
INSERT INTO umbracoUser2NodePermission (userID, nodeId, permission) 
SELECT DISTINCT userID, nodeId, 'H' FROM umbracoUser2NodePermission WHERE userId IN
(SELECT umbracoUser.id FROM umbracoUserType INNER JOIN umbracoUser ON umbracoUserType.id = umbracoUser.userType WHERE (umbracoUserType.userTypeAlias = 'writer'))
;
/* Add the contentRecycleBin tree type 
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 0, 0, 'content', 'contentRecycleBin', 'RecycleBin', 'folder.gif', 'folder_o.gif', 'umbraco', 'cms.presentation.Trees.ContentRecycleBin')
;
*/
/* Add the UserType tree type 
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 1, 1, 'users', 'userTypes', 'User Types', 'folder.gif', 'folder_o.gif', 'umbraco', 'cms.presentation.Trees.UserTypes')
;
*/
/* Add the User Permission tree type 
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 1, 2, 'users', 'userPermissions', 'User Permissions', 'folder.gif', 'folder_o.gif', 'umbraco', 'cms.presentation.Trees.UserPermissions');
 */
 
/* TRANSLATION RELATED SQL 
INSERT INTO [umbracoApp] ([appAlias], [sortOrder], [appIcon], [appName], [appInitWithTreeAlias]) VALUES (N'translation', 5, N'.traytranslation', N'Translation', NULL)
;
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 1, 1, 'translation','openTasks', 'Tasks assigned to you', '.sprTreeFolder', '.sprTreeFolder_o', 'umbraco', 'loadOpenTasks');
;
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 1, 2, 'translation','yourTasks', 'Tasks created by you', '.sprTreeFolder', '.sprTreeFolder_o', 'umbraco', 'loadYourTasks');
; 
*/
CREATE TABLE [cmsTagRelationship](
	[nodeId] [int] NOT NULL,
	[tagId] [int] NOT NULL)
;

CREATE TABLE [cmsTags](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[tag] [nvarchar](200) NULL,
	[parentId] [int] NULL,
	[group] [nvarchar](100) NULL)
;
alter TABLE [umbracoUser]
add [defaultToLiveEditing] bit NOT NULL CONSTRAINT
[DF_umbracoUser_defaultToLiveEditing] DEFAULT (0)
;

/* INSERT NEW MEDIA RECYCLE BIN NODE */
!!!SET IDENTITY_INSERT [umbracoNode] ON
|INSERT INTO umbracoNode (id, trashed, parentID, nodeUser, level, path, sortOrder, uniqueID, text, nodeObjectType) VALUES (-21, 0, -1, 0, 0, '-1,-21', 0, 'BF7C7CBC-952F-4518-97A2-69E9C7B33842', 'Recycle Bin', 'CF3D8E34-1C1C-41e9-AE56-878B57B32113')
|SET IDENTITY_INSERT [umbracoNode] OFF
;
/* Add the mediaRecycleBin tree type 
INSERT INTO umbracoAppTree (treeSilent, treeInitialize, treeSortOrder, appAlias, treeAlias, treeTitle, treeIconClosed, treeIconOpen, treeHandlerAssembly, treeHandlerType)
VALUES (0, 0, 0, 'media', 'mediaRecycleBin', 'RecycleBin', 'folder.gif', 'folder_o.gif', 'umbraco', 'cms.presentation.Trees.MediaRecycleBin')
;
*/

/* PREVIEW */
CREATE TABLE [cmsPreviewXml](
	[nodeId] [int] NOT NULL,
	[versionId] [uniqueidentifier] NOT NULL,
	[timestamp] [datetime] NOT NULL,
	[xml] [ntext] NOT NULL)

;


/***********************************************************************************************************************

ADD NEW PRIMARY KEYS, FOREIGN KEYS AND INDEXES FOR VERSION 4.1

IMPORTANT!!!!!
YOU MUST MAKE SURE THAT THE SCRIPT BELOW THIS MATCHES THE KeysIndexesAndConstraints.sql FILE FOR THE MANUAL UPGRADE

*/

/************************** CLEANUP ***********************************************/

/************************** CLEANUP END ********************************************/

/************************** RESEEDING NEEDED FOR SQL CE 4 ************************/
ALTER TABLE [umbracoNode] ALTER COLUMN id IDENTITY(1042,1)
;
ALTER TABLE [cmsContentType] ALTER COLUMN pk IDENTITY(535,1)
;
ALTER TABLE [umbracoUser] ALTER COLUMN id IDENTITY(1,1)
;
ALTER TABLE [umbracoUserType] ALTER COLUMN id IDENTITY(5,1)
;
ALTER TABLE [cmsMacroPropertyType] ALTER COLUMN id IDENTITY(26,1)
;
ALTER TABLE [cmsPropertyTypeGroup] ALTER COLUMN id IDENTITY(6,1)
;
ALTER TABLE [cmsPropertyType] ALTER COLUMN id IDENTITY(28,1)
;
ALTER TABLE [umbracoLanguage] ALTER COLUMN id IDENTITY(2,1)
;
ALTER TABLE [cmsDataType] ALTER COLUMN pk IDENTITY(39,1)
;
ALTER TABLE [cmsDataTypePreValues] ALTER COLUMN id IDENTITY(5,1)