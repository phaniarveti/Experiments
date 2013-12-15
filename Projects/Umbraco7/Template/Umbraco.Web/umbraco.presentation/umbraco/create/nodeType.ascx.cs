using Umbraco.Core;
using Umbraco.Web.UI;

namespace umbraco.cms.presentation.create.controls
{
	using System;
	using System.Data;
	using System.Drawing;
	using System.Web;
	using System.Web.UI.WebControls;
	using System.Web.UI.HtmlControls;
	using umbraco.cms.helpers;
	using umbraco.BasePages;
    using umbraco.cms.businesslogic.web;

	/// <summary>
	///		Summary description for nodeType.
	/// </summary>
	public partial class nodeType : System.Web.UI.UserControl
	{


		protected void Page_Load(object sender, System.EventArgs e)
		{
			sbmt.Text = ui.Text("create");
            pp_name.Text = ui.Text("name");

            if (!IsPostBack)
            {
                string nodeId = umbraco.helper.Request("nodeId");
                if (String.IsNullOrEmpty(nodeId) || nodeId == "init")
                {
                    masterType.Items.Add(new ListItem(ui.Text("none") + "...", "0"));
                    foreach (cms.businesslogic.web.DocumentType dt in cms.businesslogic.web.DocumentType.GetAllAsList())
                    {
                        //                    if (dt.MasterContentType == 0)
                        masterType.Items.Add(new ListItem(dt.Text, dt.Id.ToString()));
                    }
                }
                else
                {
                    // there's already a master doctype defined
                    masterType.Visible = false;
                    masterTypePreDefined.Visible = true;
                    masterTypePreDefined.Text = "<h3>" + new DocumentType(int.Parse(nodeId)).Text + "</h3>";
                }
            }
		}

        protected void validationDoctypeName(object sender, ServerValidateEventArgs e) {
            if (DocumentType.GetByAlias(rename.Text) != null)
                e.IsValid = false;
        }

        protected void validationDoctypeAlias(object sender, ServerValidateEventArgs e)
        {
            if (string.IsNullOrEmpty(rename.Text.ToSafeAlias()))
                e.IsValid = false;
        }

		protected void sbmt_Click(object sender, System.EventArgs e)
		{
			if (Page.IsValid) 
			{
				var createTemplateVal = 0;
			    if (createTemplate.Checked)
					createTemplateVal = 1;

                // check master type
                var masterTypeVal = String.IsNullOrEmpty(umbraco.helper.Request("nodeId")) || umbraco.helper.Request("nodeId") == "init" ? masterType.SelectedValue : umbraco.helper.Request("nodeId");

                var returnUrl = LegacyDialogHandler.Create(
                    new HttpContextWrapper(Context),
                    BasePage.Current.getUser(),
                    helper.Request("nodeType"),
                    createTemplateVal,
					rename.Text,
                    int.Parse(masterTypeVal));

				BasePage.Current.ClientTools
					.ChangeContentFrameUrl(returnUrl)
					.ChildNodeCreated()
					.CloseModalWindow();

			}
		
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
	}
}
