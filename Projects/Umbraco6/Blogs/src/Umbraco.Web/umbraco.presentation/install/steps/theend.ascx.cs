using System.IO;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using umbraco.BusinessLogic;
using System;
using System.Data;
using System.Drawing;
using System.Web;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace umbraco.presentation.install.steps
{
    

    /// <summary>
    ///		Summary description for theend.
    /// </summary>
    public partial class theend : System.Web.UI.UserControl
    {

        protected void Page_Load(object sender, System.EventArgs e)
        {
            // Update configurationStatus
            try
            {

                GlobalSettings.ConfigurationStatus = UmbracoVersion.Current.ToString(3);
                Application["umbracoNeedConfiguration"] = false;
            }
            catch (Exception)
            {
                //errorLiteral.Text = ex.ToString();
            }

            // Update ClientDependency version
            var clientDependencyConfig = new ClientDependencyConfiguration();
            var clientDependencyUpdated = clientDependencyConfig.IncreaseVersionNumber();

            if (!cms.businesslogic.skinning.Skinning.IsStarterKitInstalled())
                customizeSite.Visible = false;

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
