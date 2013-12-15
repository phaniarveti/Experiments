using System;
using System.Linq;

namespace Umbraco.Core.PropertyEditors
{
	internal class DatePickerPropertyEditorValueConverter : IPropertyEditorValueConverter
	{
		public bool IsConverterFor(Guid propertyEditorId, string docTypeAlias, string propertyTypeAlias)
		{
			return (new[]
				{
					Guid.Parse("b6fb1622-afa5-4bbf-a3cc-d9672a442222"),
					Guid.Parse("23e93522-3200-44e2-9f29-e61a6fcbb79a")
				}).Contains(propertyEditorId);
		}

		/// <summary>
		/// return a DateTime object even if the value is a string
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public Attempt<object> ConvertPropertyValue(object value)
		{
			return value.TryConvertTo(typeof(DateTime));
		}
	}
}