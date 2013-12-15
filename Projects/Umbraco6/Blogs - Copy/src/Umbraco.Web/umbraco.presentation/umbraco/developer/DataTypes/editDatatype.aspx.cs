using System;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.presentation.Trees;
using umbraco.IO;

namespace umbraco.cms.presentation.developer
{
    public partial class editDatatype : BasePages.UmbracoEnsuredPage
    {
        public editDatatype()
        {
            CurrentApp = BusinessLogic.DefaultApps.developer.ToString();

        }
        protected ImageButton save;
        private cms.businesslogic.datatype.DataTypeDefinition dt;
        cms.businesslogic.datatype.controls.Factory f;
        private int _id = 0;
        private interfaces.IDataPrevalue _prevalue;

        protected void Page_Load(object sender, System.EventArgs e)
        {
            pp_name.Text = ui.Text("name");
            pp_renderControl.Text = ui.Text("renderControl");
            pane_settings.Text = ui.Text("settings");
            pp_guid.Text = ui.Text("guid");


            _id = int.Parse(Request.QueryString["id"]);
            dt = cms.businesslogic.datatype.DataTypeDefinition.GetDataTypeDefinition(_id);


            f = new cms.businesslogic.datatype.controls.Factory();

            if (!IsPostBack)
            {
                txtName.Text = dt.Text;

                SortedList datatypes = new SortedList();

                foreach (interfaces.IDataType df in f.GetAll())
                    datatypes.Add(df.DataTypeName + "|" + Guid.NewGuid().ToString(), df.Id);

                IDictionaryEnumerator ide = datatypes.GetEnumerator();

                string datatTypeId = dt.DataType != null ? dt.DataType.Id.ToString() : String.Empty;
                while (ide.MoveNext())
                {
                    ListItem li = new ListItem();
                    li.Text = ide.Key.ToString().Substring(0, ide.Key.ToString().IndexOf("|"));
                    li.Value = ide.Value.ToString();

                    if (!String.IsNullOrEmpty(datatTypeId) && li.Value.ToString() == datatTypeId) li.Selected = true;
                    ddlRenderControl.Items.Add(li);
                }

                ClientTools
                    .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadDataTypes>().Tree.Alias)
                    .SyncTree("-1,init," + _id.ToString(), false);

            }

            if (dt.DataType != null)
                litGuid.Text = dt.DataType.Id.ToString();
            Panel1.Text = umbraco.ui.Text("edit") + " datatype: " + dt.Text;
            insertPrevalueEditor();
        }

        private void insertPrevalueEditor()
        {
            try
            {
                if (ddlRenderControl.SelectedIndex >= 0)
                {
                    interfaces.IDataType o = f.DataType(new Guid(ddlRenderControl.SelectedValue));

                    o.DataTypeDefinitionId = dt.Id;
                    _prevalue = o.PrevalueEditor;

                    if (o.PrevalueEditor.Editor != null)
                        plcEditorPrevalueControl.Controls.Add(o.PrevalueEditor.Editor);
                }
                else
                {
                    plcEditorPrevalueControl.Controls.Add(new LiteralControl("No editor control selected"));
                }
            }
            catch { }

        }

        protected void save_click(object sender, ImageClickEventArgs e)
        {
            // save prevalues;
            if (_prevalue != null)
                _prevalue.Save();

            dt.Text = txtName.Text;

            dt.DataType = f.DataType(new Guid(ddlRenderControl.SelectedValue));
            dt.Save();

            System.Web.HttpRuntime.Cache.Remove(string.Format("UmbracoDataTypeDefinition{0}", dt.UniqueId));

            ClientTools.ShowSpeechBubble(BasePages.BasePage.speechBubbleIcon.save, ui.Text("speechBubbles", "dataTypeSaved", null), "");

            ClientTools.SyncTree(dt.Path, true);
        }


        #region Web Form Designer generated code


        override protected void OnInit(EventArgs e)
        {
            save = Panel1.Menu.NewImageButton();
            save.ID = "save";
            save.Click += new System.Web.UI.ImageClickEventHandler(save_click);
            save.ImageUrl = SystemDirectories.Umbraco + "/images/editor/save.gif";

            Panel1.hasMenu = true;

            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {

        }
        #endregion
    }
}
