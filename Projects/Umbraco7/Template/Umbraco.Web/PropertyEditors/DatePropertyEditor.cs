using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.DateAlias, "Date", "DATE", "datepicker")]
    public class DatePropertyEditor : PropertyEditor
    {
        public DatePropertyEditor()
        {
            _defaultPreVals = new Dictionary<string, object>
                {
                    {"format", "yyyy-MM-dd"},
                    {"pickTime", false}
                };
        }

        private IDictionary<string, object> _defaultPreVals;

        /// <summary>
        /// Overridden because we ONLY support Date (no time) format and we don't have pre-values in the db.
        /// </summary>
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return _defaultPreVals; }
            set { _defaultPreVals = value; }
        }

        protected override PropertyValueEditor CreateValueEditor()
        {
            return new DatePropertyValueEditor(base.CreateValueEditor());
        }

        /// <summary>
        /// CUstom value editor so we can serialize with the correct date format (excluding time)
        /// and includes the date validator
        /// </summary>
        internal class DatePropertyValueEditor : PropertyValueEditorWrapper
        {
            public DatePropertyValueEditor(PropertyValueEditor wrapped) : base(wrapped)
            {
                Validators.Add(new DateTimeValidator());
            }

            public override object ConvertDbToEditor(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
            {
                var date = property.Value.TryConvertTo<DateTime?>();
                if (date.Success == false || date.Result == null)
                {
                    return string.Empty;
                }
                //Dates will be formatted as yyyy-MM-dd
                return date.Result.Value.ToString("yyyy-MM-dd");                
            }

        }
    }
}