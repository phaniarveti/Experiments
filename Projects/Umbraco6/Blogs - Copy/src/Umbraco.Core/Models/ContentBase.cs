﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Web;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents an abstract class for base Content properties and methods
    /// </summary>
    public abstract class ContentBase : Entity, IContentBase
    {
        protected IContentTypeComposition ContentTypeBase;
        private Lazy<int> _parentId;
        private string _name;//NOTE Once localization is introduced this will be the localized Name of the Content/Media.
        private int _sortOrder;
        private int _level;
        private string _path;
        private int _creatorId;
        private bool _trashed;
        private int _contentTypeId;
        private PropertyCollection _properties;

        /// <summary>
        /// Protected constructor for ContentBase (Base for Content and Media)
        /// </summary>
        /// <param name="name">Localized Name of the entity</param>
        /// <param name="parentId"></param>
        /// <param name="contentType"></param>
        /// <param name="properties"></param>
        protected ContentBase(string name, int parentId, IContentTypeComposition contentType, PropertyCollection properties)
        {
            Mandate.ParameterCondition(parentId != 0, "parentId");
            Mandate.ParameterNotNull(contentType, "contentType");
            Mandate.ParameterNotNull(properties, "properties");

            ContentTypeBase = contentType;
            Version = Guid.NewGuid();

            _parentId = new Lazy<int>(() => parentId);
            _name = name;
            _contentTypeId = int.Parse(contentType.Id.ToString(CultureInfo.InvariantCulture));
            _properties = properties;
            _properties.EnsurePropertyTypes(PropertyTypes);
        }

        /// <summary>
        /// Protected constructor for ContentBase (Base for Content and Media)
        /// </summary>
        /// <param name="name">Localized Name of the entity</param>
        /// <param name="parent"></param>
        /// <param name="contentType"></param>
        /// <param name="properties"></param>
        protected ContentBase(string name, IContentBase parent, IContentTypeComposition contentType, PropertyCollection properties)
		{
			Mandate.ParameterNotNull(parent, "parent");
			Mandate.ParameterNotNull(contentType, "contentType");
			Mandate.ParameterNotNull(properties, "properties");

            ContentTypeBase = contentType;
            Version = Guid.NewGuid();

			_parentId = new Lazy<int>(() => parent.Id);
            _name = name;
			_contentTypeId = int.Parse(contentType.Id.ToString(CultureInfo.InvariantCulture));
			_properties = properties;
			_properties.EnsurePropertyTypes(PropertyTypes);
		}

	    private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<ContentBase, string>(x => x.Name);
        private static readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.ParentId);
        private static readonly PropertyInfo SortOrderSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.SortOrder);
        private static readonly PropertyInfo LevelSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.Level);
        private static readonly PropertyInfo PathSelector = ExpressionHelper.GetPropertyInfo<ContentBase, string>(x => x.Path);
        private static readonly PropertyInfo CreatorIdSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.CreatorId);
        private static readonly PropertyInfo TrashedSelector = ExpressionHelper.GetPropertyInfo<ContentBase, bool>(x => x.Trashed);
        private static readonly PropertyInfo DefaultContentTypeIdSelector = ExpressionHelper.GetPropertyInfo<ContentBase, int>(x => x.ContentTypeId);
        private readonly static PropertyInfo PropertyCollectionSelector = ExpressionHelper.GetPropertyInfo<ContentBase, PropertyCollection>(x => x.Properties);

        protected void PropertiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(PropertyCollectionSelector);
        }

        /// <summary>
        /// Gets or sets the Id of the Parent entity
        /// </summary>
        [DataMember]
        public virtual int ParentId
        {
            get
            {
	            var val = _parentId.Value;
				if (val == 0)
				{
					throw new InvalidOperationException("The ParentId cannot have a value of 0. Perhaps the parent object used to instantiate this object has not been persisted to the data store.");
				}
				return val;
            }
            set
            {
                _parentId = new Lazy<int>(() => value);
                OnPropertyChanged(ParentIdSelector);
            }
        }

        /// <summary>
        /// Gets or sets the name of the entity
        /// </summary>
        [DataMember]
        public virtual string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged(NameSelector);
            }
        }

        /// <summary>
        /// Gets or sets the sort order of the content entity
        /// </summary>
        [DataMember]
        public virtual int SortOrder
        {
            get { return _sortOrder; }
            set
            {
                _sortOrder = value;
                OnPropertyChanged(SortOrderSelector);
            }
        }

        /// <summary>
        /// Gets or sets the level of the content entity
        /// </summary>
        [DataMember]
        public virtual int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                OnPropertyChanged(LevelSelector);
            }
        }

        /// <summary>
        /// Gets or sets the path
        /// </summary>
        [DataMember]
        public virtual string Path //Setting this value should be handled by the class not the user
        {
            get { return _path; }
            set
            {
                _path = value;
                OnPropertyChanged(PathSelector);
            }
        }

        /// <summary>
        /// Profile of the user who created this Content
        /// </summary>
        [DataMember]
        public virtual int CreatorId
        {
            get { return _creatorId; }
            set
            {
                _creatorId = value;
                OnPropertyChanged(CreatorIdSelector);
            }
        }
        
        /// <summary>
        /// Boolean indicating whether this Content is Trashed or not.
        /// If Content is Trashed it will be located in the Recyclebin.
        /// </summary>
        /// <remarks>When content is trashed it should be unpublished</remarks>
        [DataMember]
        public virtual bool Trashed //Setting this value should be handled by the class not the user
        {
            get { return _trashed; }
            internal set
            {
                _trashed = value;
                OnPropertyChanged(TrashedSelector);
            }
        }

        /// <summary>
        /// Guid Id of the curent Version
        /// </summary>
        [DataMember]
        public Guid Version { get; internal set; }

        /// <summary>
        /// Integer Id of the default ContentType
        /// </summary>
        [DataMember]
        public virtual int ContentTypeId
        {
            get { return _contentTypeId; }
            protected set
            {
                _contentTypeId = value;
                OnPropertyChanged(DefaultContentTypeIdSelector);
            }
        }

        /// <summary>
        /// Collection of properties, which make up all the data available for this Content object
        /// </summary>
        [DataMember]
        public virtual PropertyCollection Properties
        {
            get { return _properties; }
            set
            {
                _properties = value;
                _properties.CollectionChanged += PropertiesChanged;
            }
        }

        /// <summary>
        /// List of PropertyGroups available on this Content object
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyGroup> PropertyGroups { get { return ContentTypeBase.CompositionPropertyGroups; } }

        /// <summary>
        /// List of PropertyTypes available on this Content object
        /// </summary>
        [IgnoreDataMember]
        public IEnumerable<PropertyType> PropertyTypes { get { return ContentTypeBase.CompositionPropertyTypes; } }

        /// <summary>
        /// Indicates whether the content object has a property with the supplied alias
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns>True if Property with given alias exists, otherwise False</returns>
        public virtual bool HasProperty(string propertyTypeAlias)
        {
            return Properties.Contains(propertyTypeAlias);
        }

        /// <summary>
        /// Gets the value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns><see cref="Property"/> Value as an <see cref="object"/></returns>
        public virtual object GetValue(string propertyTypeAlias)
        {
            return Properties[propertyTypeAlias].Value;
        }

        /// <summary>
        /// Gets the value of a Property
        /// </summary>
        /// <typeparam name="TPassType">Type of the value to return</typeparam>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <returns><see cref="Property"/> Value as a <see cref="TPassType"/></returns>
        public virtual TPassType GetValue<TPassType>(string propertyTypeAlias)
        {
            return (TPassType)Properties[propertyTypeAlias].Value;
        }

        /// <summary>
        /// Sets the <see cref="System.Object"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetValue(string propertyTypeAlias, object value)
        {
            if (value == null)
            {
                SetValueOnProperty(propertyTypeAlias, value);
                return;
            }

            // .NET magic to call one of the 'SetPropertyValue' handlers with matching signature 
            ((dynamic)this).SetPropertyValue(propertyTypeAlias, (dynamic)value);
        }

        /// <summary>
        /// Sets the <see cref="System.String"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetPropertyValue(string propertyTypeAlias, string value)
        {
            SetValueOnProperty(propertyTypeAlias, value);
        }

        /// <summary>
        /// Sets the <see cref="System.Int32"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetPropertyValue(string propertyTypeAlias, int value)
        {
            SetValueOnProperty(propertyTypeAlias, value);
        }

        /// <summary>
        /// Sets the <see cref="System.Int64"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetPropertyValue(string propertyTypeAlias, long value)
        {
            string val = value.ToString();
            SetValueOnProperty(propertyTypeAlias, val);
        }

        /// <summary>
        /// Sets the <see cref="System.Boolean"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetPropertyValue(string propertyTypeAlias, bool value)
        {
            int val = Convert.ToInt32(value);
            SetValueOnProperty(propertyTypeAlias, val);
        }

        /// <summary>
        /// Sets the <see cref="System.DateTime"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetPropertyValue(string propertyTypeAlias, DateTime value)
        {
            SetValueOnProperty(propertyTypeAlias, value);
        }

        /// <summary>
        /// Sets the <see cref="System.Web.HttpPostedFile"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetPropertyValue(string propertyTypeAlias, HttpPostedFile value)
        {
            ContentExtensions.SetValue(this, propertyTypeAlias, value);
        }

        /// <summary>
        /// Sets the <see cref="System.Web.HttpPostedFileBase"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetPropertyValue(string propertyTypeAlias, HttpPostedFileBase value)
        {
            ContentExtensions.SetValue(this, propertyTypeAlias, value);
        }

        /// <summary>
        /// Sets the <see cref="System.Web.HttpPostedFileWrapper"/> value of a Property
        /// </summary>
        /// <param name="propertyTypeAlias">Alias of the PropertyType</param>
        /// <param name="value">Value to set for the Property</param>
        public virtual void SetPropertyValue(string propertyTypeAlias, HttpPostedFileWrapper value)
        {
            ContentExtensions.SetValue(this, propertyTypeAlias, value);
        }

        /// <summary>
        /// Private method to set the value of a property
        /// </summary>
        /// <param name="propertyTypeAlias"></param>
        /// <param name="value"></param>
        private void SetValueOnProperty(string propertyTypeAlias, object value)
        {
            if (Properties.Contains(propertyTypeAlias))
            {
                Properties[propertyTypeAlias].Value = value;
                return;
            }

            var propertyType = PropertyTypes.FirstOrDefault(x => x.Alias == propertyTypeAlias);
            if (propertyType == null)
            {
                throw new Exception(String.Format("No PropertyType exists with the supplied alias: {0}", propertyTypeAlias));
            }
            Properties.Add(propertyType.CreatePropertyFromValue(value));
        }

        /// <summary>
        /// Boolean indicating whether the content and its properties are valid
        /// </summary>
        /// <returns>True if content is valid otherwise false</returns>
        public virtual bool IsValid()
        {
            return Properties.Any(property => !property.IsValid()) == false;
        }

        public abstract void ChangeTrashedState(bool isTrashed, int parentId = -20);
    }
}