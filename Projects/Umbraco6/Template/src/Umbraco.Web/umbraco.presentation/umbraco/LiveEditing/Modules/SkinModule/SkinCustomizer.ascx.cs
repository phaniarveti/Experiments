using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Umbraco.Core.Logging;
using umbraco.cms.businesslogic.skinning;
using System.Xml;
using System.Text;
using umbraco.interfaces.skinning;
using umbraco.IO;
using umbraco.cms.businesslogic.template;
using umbraco.BusinessLogic;
using umbraco.NodeFactory;
using umbraco.cms.businesslogic.packager;
using System.IO;

namespace umbraco.presentation.LiveEditing.Modules.SkinModule
{

    public partial class SkinCustomizer : UserControl
    {
        // Fields

       
        private cms.businesslogic.packager.repositories.Repository repo;
        private cms.businesslogic.skinning.Skin ActiveSkin;

        private string repoGuid = "65194810-1f85-11dd-bd0b-0800200c9a66";

        private List<Dependency> sDependencies = new List<Dependency>();

        // Methods
        public SkinCustomizer()
        {
            this.repo = cms.businesslogic.packager.repositories.Repository.getByGuid(this.repoGuid);
        }

        protected void btnOk_Click(object sender, EventArgs e)
        {
            this.ActiveSkin.SaveOutput();

            //css vars
            SortedList<string, string> cssVars = new SortedList<string, string>();
            if (this.ActiveSkin.Css != null)
            {
                foreach (CssVariable cssVar in this.ActiveSkin.Css.Variables)
                {
                    cssVars.Add(cssVar.Name, cssVar.DefaultValue);
                }
            }


            foreach (Dependency dependency in this.sDependencies)
            {
                if (dependency.DependencyType.Values.Count > 0)
                {
                    string output = dependency.DependencyType.Values[0].ToString();
                    foreach (Task task in dependency.Tasks)
                    {
                        TaskExecutionDetails details = task.TaskType.Execute(this.ParsePlaceHolders(task.Value, output));
                        if (details.TaskExecutionStatus == TaskExecutionStatus.Completed)
                        {
                            this.ActiveSkin.AddTaskHistoryNode(task.TaskType.ToXml(details.OriginalValue, details.NewValue));
                        }
                    }

                    //css vars
                    if (!string.IsNullOrEmpty(dependency.Variable))
                    {
                        if(cssVars[dependency.Variable] != null)
                            cssVars[dependency.Variable] = output;
                    }
                }
            }

            if (this.ActiveSkin.Css != null && !string.IsNullOrEmpty(this.ActiveSkin.Css.Content) && !string.IsNullOrEmpty(this.ActiveSkin.Css.TargetFile))
            {
                string content = this.ActiveSkin.Css.Content;
                //css vars
                foreach (var var in cssVars)
                {
                    content = content.Replace("@" + var.Key, var.Value);
                }

                //save

                StreamWriter sw = File.AppendText(IO.IOHelper.MapPath(SystemDirectories.Css) + "/" + this.ActiveSkin.Css.TargetFile);
                sw.Write(content);
                sw.Close();
            }

            library.RefreshContent();
        }

        protected void LoadDependencies()
        {
            this.ph_dependencies.Controls.Clear();
            StringBuilder builder = new StringBuilder();

            //css vars default value
            string varquery = "";

            if (this.ActiveSkin.Css != null)
            {
              
                foreach (CssVariable cssVar in this.ActiveSkin.Css.Variables)
                {
                    builder.AppendLine(
                        string.Format("var cssvar{0} = '{1}';",cssVar.Name,cssVar.DefaultValue));

                    varquery += string.Format("+ '&{0}=' + cssvar{0}.replace('#','%23')", cssVar.Name);
                }
            }

            //preview css var change

            builder.Append(string.Format("function PreviewCssVariables(){{ var parsedcsscontent; jQuery.get('/umbraco/LiveEditing/Modules/SkinModule/CssParser.aspx?skinAlias={0}'{1}, function(data){{parsedcsscontent= data; jQuery('head').append('<style>' + parsedcsscontent + '</style>'); }}); }}",
                this.ActiveSkin.Alias,
                varquery));



            builder.Append("\r\n  var hasSetTasksClientScriptsRun = false; \r\n                function setTasksClientScripts(){ \r\n                    if(hasSetTasksClientScriptsRun == false){");

            int c = 0;
            foreach (Dependency dependency in this.ActiveSkin.Dependencies)
            {
                if (dependency.DependencyType != null)
                {
                    this.sDependencies.Add(dependency);
                    Control editor = dependency.DependencyType.Editor;
                    editor.ID = "depcontrol" + c;
                    this.ph_dependencies.addProperty(dependency.Label, editor);

                    if(!string.IsNullOrEmpty(dependency.Variable))
                    {
                        //this control is setting a css variable
                        builder.Append(dependency.DependencyType.CssVariablePreviewClientScript(editor.ClientID, "cssvar" + dependency.Variable));


                    }
                    foreach (Task task in dependency.Tasks)
                    {
                        builder.Append(task.TaskType.PreviewClientScript(editor.ClientID, dependency.DependencyType.ClientSidePreviewEventType(), dependency.DependencyType.ClientSideGetValueScript()));
                    }

                    c++;
                }
            }
            builder.Append("hasSetTasksClientScriptsRun = true; }}");
            ScriptManager.RegisterClientScriptBlock(this, base.GetType(), "TasksClientScripts", builder.ToString(), true);
        }

        protected void LoadSkins()
        {
            List<string> skinNames = new List<string>();

            Guid? nullable = Skinning.StarterKitGuid(Node.GetCurrent().template);

            if(nullable.HasValue){
                InstalledPackage p = InstalledPackage.GetByGuid(nullable.Value.ToString());
                if(p.Data.SkinRepoGuid != null && p.Data.SkinRepoGuid != Guid.Empty  && p.Data.SkinRepoGuid.ToString() != repoGuid)
                    this.repo = cms.businesslogic.packager.repositories.Repository.getByGuid(p.Data.SkinRepoGuid.ToString());
            }

            if (!(nullable.HasValue && Skinning.HasAvailableSkins(Node.GetCurrent().template)))
            {
                this.pChangeSkin.Visible = false;
            }
            else if (this.repo.HasConnection())
            {
                try
                {
                    var skins = this.repo.Webservice.Skins(nullable.ToString());
                    this.rep_starterKitDesigns.DataSource = skins;
                    this.rep_starterKitDesigns.DataBind();

                    foreach (var s in skins)
                    {
                        if(!skinNames.Contains(s.Text))
                            skinNames.Add(s.Text);
                    }
                }
                catch (Exception exception)
                {
                    LogHelper.Error<SkinCustomizer>("An error occurred", exception);
                }
            }
            else
            {
                this.ShowConnectionError();
            }

            //check for local skins

            List<string> localSkins = new List<string>();
            DirectoryInfo dirInfo = new DirectoryInfo(IOHelper.MapPath(SystemDirectories.Masterpages));
            foreach (DirectoryInfo subDur in dirInfo.GetDirectories())
            {
                var skinFile = subDur.GetFiles("skin.xml");

                if (skinFile.Length > 0)
                {
                    string c = Skin.GetSkinNameFromFile(skinFile[0].FullName);

                    if (!skinNames.Contains(c))
                        localSkins.Add(c);
                }
            }

            if (localSkins.Count > 0)
            {
                rep_starterKitDesignsLocal.DataSource = localSkins;
                rep_starterKitDesignsLocal.DataBind();
            }
            else
            {
                localSkinsContainer.Visible = false;
            }



        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (User.GetCurrent().GetApplications().Find(t => t.alias.ToLower() == "settings") == null)
            {
                throw new Exception("The current user can't edit skins as the user doesn't have access to the Settings section!");
            }
            
            NodeFactory.Node n = NodeFactory.Node.GetCurrent();
            ActiveSkin = Skin.CreateFromAlias( Skinning.GetCurrentSkinAlias(n.template) );

            pnl_connectionerror.Visible = false;

            //load dependencies
            if (ActiveSkin != null && ActiveSkin.Dependencies.Count > 0)
                LoadDependencies();
            else
            {
                //show skin selection
                pCustomizeSkin.Visible = false;
                ltCustomizeSkinStyle.Text = ltChangeSkinStyle.Text;
                ltChangeSkinStyle.Text = string.Empty;
            }

            LoadSkins();
        }

        private string ParsePlaceHolders(string value, string output)
        {
            value = value.Replace("${Output}", output);
            return value;
        }

        protected void rep_starterKitDesigns_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.DataItem != null)
            {
                cms.businesslogic.packager.repositories.Skin s = (cms.businesslogic.packager.repositories.Skin)e.Item.DataItem;

                if (Skinning.IsSkinInstalled(s.RepoGuid))
                {
                    Button inst = (Button)e.Item.FindControl("Button1");
                    inst.Text = "Apply";
                    inst.CommandName = "apply";
                    inst.CommandArgument = s.Text;
                    //inst.ID = s.Text;

                }

                if (ActiveSkin != null && ActiveSkin.Name == s.Text)
                {
                    Button inst = (Button)e.Item.FindControl("Button1");
                    inst.Text = "Rollback";
                    inst.CommandName = "remove";
                    inst.CommandArgument = s.Text;
                    //inst.ID = s.Text;
                }
            }

        }

        protected void SelectStarterKitDesign(object sender, EventArgs e)
        {
            if (((Button)sender).CommandName == "apply")
            {
                Skinning.ActivateAsCurrentSkin(Skin.CreateFromName(((Button)sender).CommandArgument));
                this.Page.Response.Redirect(library.NiceUrl(int.Parse(UmbracoContext.Current.PageId.ToString())) + "?umbSkinning=true");
            }
            else if (((Button)sender).CommandName == "remove")
            {
                Template template = new Template(Node.GetCurrent().template);
                Skinning.RollbackSkin(template.Id);
                this.Page.Response.Redirect( library.NiceUrl(int.Parse(UmbracoContext.Current.PageId.ToString())) +"?umbSkinning=true" );
            }
            else
            {
                Guid guid = new Guid(((Button)sender).CommandArgument);

                InstalledPackage p = InstalledPackage.GetByGuid(guid.ToString());
                if (p.Data.SkinRepoGuid != null && p.Data.SkinRepoGuid != Guid.Empty && p.Data.SkinRepoGuid.ToString() != repoGuid)
                    this.repo = cms.businesslogic.packager.repositories.Repository.getByGuid(p.Data.SkinRepoGuid.ToString());
                

                Installer installer = new Installer();
                if (this.repo.HasConnection())
                {
                    Installer installer2 = new Installer();
                    string tempDir = installer2.Import(this.repo.fetch(guid.ToString()));
                    installer2.LoadConfig(tempDir);
                    int packageId = installer2.CreateManifest(tempDir, guid.ToString(), this.repoGuid);
                    installer2.InstallFiles(packageId, tempDir);
                    installer2.InstallBusinessLogic(packageId, tempDir);
                    installer2.InstallCleanUp(packageId, tempDir);
                    library.RefreshContent();
                    Skinning.ActivateAsCurrentSkin(Skin.CreateFromName(((Button)sender).CommandName));
                    this.Page.Response.Redirect(library.NiceUrl(int.Parse(UmbracoContext.Current.PageId.ToString())) + "?umbSkinning=true");
                }
                else
                {
                    this.ShowConnectionError();
                }
            }
        }


        protected void rep_starterKitDesignsLocal_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.DataItem != null)
            {

                if (ActiveSkin != null && ActiveSkin.Name == e.Item.DataItem.ToString())
                {
                    Button inst = (Button)e.Item.FindControl("btnApply");
                    inst.Text = "Rollback";
                    inst.CommandName = "remove";
                }
            }

        }

        protected void SelectLocalStarterKitDesign(object sender, EventArgs e)
        {
            if (((Button)sender).CommandName == "apply")
            {
                Skinning.ActivateAsCurrentSkin(Skin.CreateFromName(((Button)sender).CommandArgument));
                this.Page.Response.Redirect(library.NiceUrl(int.Parse(UmbracoContext.Current.PageId.ToString())) + "?umbSkinning=true");
            }
            else if (((Button)sender).CommandName == "remove")
            {
                Template template = new Template(Node.GetCurrent().template);
                Skinning.RollbackSkin(template.Id);
                this.Page.Response.Redirect(library.NiceUrl(int.Parse(UmbracoContext.Current.PageId.ToString())) + "?umbSkinning=true");
            }
        }

        private void ShowConnectionError()
        {
            this.pnl_connectionerror.Visible = true;
        }
    }
}
