using System;
using Umbraco.Core;

namespace umbraco.editorControls.colorpicker
{
	/// <summary>
	/// Summary description for ColorPickerDataType.
	/// </summary>
	public class ColorPickerDataType : cms.businesslogic.datatype.BaseDataType, interfaces.IDataType
	{
		private interfaces.IDataEditor _Editor;
		private interfaces.IData _baseData;
		private KeyValuePrevalueEditor _prevalueeditor;

		public override interfaces.IDataEditor DataEditor 
		{
			get
			{
				if (_Editor == null) 
				{
					_Editor = new colorPicker(Data,((KeyValuePrevalueEditor)PrevalueEditor).PrevaluesAsKeyValuePairList);
				}
				return _Editor;
			}
		}

		public override interfaces.IData Data 
		{
			get 
			{
				if (_baseData == null)
					_baseData = new cms.businesslogic.datatype.DefaultData(this);
				return _baseData;
			}
		}
		public override string DataTypeName 
		{
			get {return "Color Picker";}
		}
		public override Guid Id 
		{
			get { return new Guid(Constants.PropertyEditors.ColorPicker); }
		}

		public override interfaces.IDataPrevalue PrevalueEditor 
		{
			get 
			{
				if (_prevalueeditor == null)
					_prevalueeditor = new KeyValuePrevalueEditor(this);
				return _prevalueeditor;
			}
		}
	}
}
