using System;

namespace umbraco.macroRenderings
{
	/// <summary>
	/// 
	/// </summary>
	public class text : System.Web.UI.WebControls.TextBox, interfaces.IMacroGuiRendering
	{
		string _value = "";

		public bool ShowCaption 
		{
			get {return true;}
		}

		public string Value 
		{
			get {
				return base.Text;
			}
			set {
				this.Text = value;
				_value = value;
			}
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit (e);
			this.CssClass = "guiInputTextStandard";
		}


		public text()
		{
			//
			// TODO: Add constructor logic here
			//
		}


	}
}
