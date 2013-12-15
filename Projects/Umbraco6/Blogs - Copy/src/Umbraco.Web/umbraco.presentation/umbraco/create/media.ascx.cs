using System.Linq;

namespace umbraco.cms.presentation.create.controls
{
    using System;
    using System.Web.UI.WebControls;
    using umbraco.BasePages;

    /// <summary>
    ///		Summary description for media.
    /// </summary>
    public partial class media : System.Web.UI.UserControl
    {


        protected void Page_Load(object sender, System.EventArgs e)
		{
			sbmt.Text = ui.Text("create");
			int NodeId = int.Parse(Request["nodeID"]);
			
			int[] allowedIds = new int[0];
			if (NodeId > 2) 
			{
				cms.businesslogic.Content c = new cms.businesslogic.media.Media(NodeId);
				allowedIds = c.ContentType.AllowedChildContentTypeIDs;
			}

		    var documentTypeList = businesslogic.media.MediaType.GetAllAsList().ToList();
		    foreach (var dt in documentTypeList)
			{
				ListItem li = new ListItem();
				li.Text = dt.Text;
				li.Value = dt.Id.ToString();
				
				if (NodeId > 2) 
				{
					foreach (int i in allowedIds) if (i == dt.Id) nodeType.Items.Add(li);
				}
                // The Any check is here for backwards compatibility, if none are allowed at root, then all are allowed
                else if (documentTypeList.Any(d => d.AllowAtRoot) == false || dt.AllowAtRoot)
					nodeType.Items.Add(li);
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

        protected void sbmt_Click(object sender, System.EventArgs e)
        {
            if (Page.IsValid)
            {
                string returnUrl = umbraco.presentation.create.dialogHandler_temp.Create(
                    umbraco.helper.Request("nodeType"),
                    int.Parse(nodeType.SelectedValue),
                    int.Parse(Request["nodeID"]),
                    rename.Text);

                BasePage.Current.ClientTools
                    .ChangeContentFrameUrl(returnUrl)
                    .CloseModalWindow();

            }
        }
    }
}
