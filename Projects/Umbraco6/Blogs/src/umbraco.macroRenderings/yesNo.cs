using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace umbraco.macroRenderings
{
	/// <summary>
	/// Generates a radiolist of yes and no for boolean fields
	/// </summary>
	[DefaultProperty("Text"), 
	ToolboxData("<{0}:yesNo runat=server Version=\"\" Alias=\"\"></{0}:numberField>")]
	public class yesNo : System.Web.UI.WebControls.CheckBox, interfaces.IMacroGuiRendering
	{
		string _value = "";

		public bool ShowCaption 
		{
			get {return true;}
		}


		[Bindable(true),
		Category("Umbraco"),
		DefaultValue(""), 
		Browsable(true)]
		public String Value
		{
			get {
				if (base.Checked)
					return "1";

				return "0";
			}

			set {_value = value;}
		}

		protected override void OnInit(EventArgs e)
		{
            if (_value != null)
            {
                if (_value == "1")
                    this.Checked = true;
                else
                    this.Checked = false;
            }

			this.Text = "Yes";
			base.OnInit (e);
		}

	}
}
