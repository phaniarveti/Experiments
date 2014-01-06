using System;
using umbraco.BasePages;

namespace umbraco.cms.presentation.create.controls
{
    /// <summary>
	///		Summary description for simple.
	/// </summary>
	public partial class simple : System.Web.UI.UserControl
	{

		protected void Page_Load(object sender, System.EventArgs e)
		{
			sbmt.Text = ui.Text("create");
			// Put user code to initialize the page here
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		///		Required method for Designer support - do not modify
		///		the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{

		}
		#endregion

		protected void sbmt_Click(object sender, System.EventArgs e)
		{
			if (Page.IsValid) 
			{
				int nodeId;
			    if (int.TryParse(Request.QueryString["nodeId"], out nodeId) == false)
			        nodeId = -1;

				if (umbraco.helper.Request("nodeId") != "init")
					nodeId = int.Parse(umbraco.helper.Request("nodeId"));

				string returnUrl = umbraco.presentation.create.dialogHandler_temp.Create(
					umbraco.helper.Request("nodeType"),
					nodeId,
					rename.Text);


				BasePage.Current.ClientTools
					.ChangeContentFrameUrl(returnUrl)
					.ChildNodeCreated()
					.CloseModalWindow();

                
            }
		
		}
	}
}
