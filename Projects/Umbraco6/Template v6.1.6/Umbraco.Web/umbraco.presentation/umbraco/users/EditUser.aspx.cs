using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using Umbraco.Core.Logging;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using umbraco.businesslogic.Exceptions;
using umbraco.cms.businesslogic.media;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.channels.businesslogic;
using umbraco.uicontrols;
using umbraco.providers;
using umbraco.cms.presentation.Trees;
using umbraco.IO;
using Umbraco.Core;

namespace umbraco.cms.presentation.user
{
    /// <summary>
    /// Summary description for EditUser.
    /// </summary>
    public partial class EditUser : UmbracoEnsuredPage
    {
        public EditUser()
        {
            CurrentApp = BusinessLogic.DefaultApps.users.ToString();
        }
        protected HtmlTable macroProperties;
        protected TextBox uname = new TextBox();
        protected TextBox lname = new TextBox();
        protected PlaceHolder passw = new PlaceHolder();
        protected CheckBoxList lapps = new CheckBoxList();
        protected TextBox email = new TextBox();
        protected DropDownList userType = new DropDownList();
        protected DropDownList userLanguage = new DropDownList();
        protected CheckBox NoConsole = new CheckBox();
        protected CheckBox Disabled = new CheckBox();
        protected CheckBox DefaultToLiveEditing = new CheckBox();


        protected controls.ContentPicker mediaPicker = new umbraco.controls.ContentPicker();
        protected controls.ContentPicker contentPicker = new umbraco.controls.ContentPicker();

        protected TextBox cName = new TextBox();
        protected CheckBox cFulltree = new CheckBox();
        protected DropDownList cDocumentType = new DropDownList();
        protected DropDownList cDescription = new DropDownList();
        protected DropDownList cCategories = new DropDownList();
        protected DropDownList cExcerpt = new DropDownList();
        protected controls.ContentPicker cMediaPicker = new umbraco.controls.ContentPicker();
        protected controls.ContentPicker cContentPicker = new umbraco.controls.ContentPicker();
        protected CustomValidator sectionValidator = new CustomValidator();

        protected Pane pp = new Pane();

        private User u;


        protected void Page_Load(object sender, EventArgs e)
        {
            //if the current user is not an admin they cannot edit a user at all
            if (CurrentUser.IsAdmin() == false)
            {
                throw new UserAuthorizationException("Access denied");
            }

            int UID = int.Parse(Request.QueryString["id"]);
            u = BusinessLogic.User.GetUser(UID);

            //the true admin can only edit the true admin
            if (u.Id == 0 && CurrentUser.Id != 0)
            {
                throw new Exception("Only the root user can edit the 'root' user (id:0)");
            }

            //only another admin can edit another admin (who is not the true admin)
            if (u.IsAdmin() && CurrentUser.IsAdmin() == false)
            {
                throw new Exception("Admin users can only be edited by admins");
            }
            
            // check if canvas editing is enabled
            DefaultToLiveEditing.Visible = UmbracoSettings.EnableCanvasEditing;

            // Populate usertype list
            foreach (UserType ut in UserType.getAll)
            {
                if (CurrentUser.IsAdmin() || ut.Alias != "admin")
                {
                    ListItem li = new ListItem(ui.Text("user", ut.Name.ToLower(), base.getUser()), ut.Id.ToString());
                    if (ut.Id == u.UserType.Id)
                        li.Selected = true;

                    userType.Items.Add(li);
                }
            }

            // Populate ui language lsit
            foreach (
                string f in
                    Directory.GetFiles(IOHelper.MapPath(SystemDirectories.Umbraco + "/config/lang"), "*.xml")
                )
            {
                XmlDocument x = new XmlDocument();
                x.Load(f);
                ListItem li =
                    new ListItem(x.DocumentElement.Attributes.GetNamedItem("intName").Value,
                                 x.DocumentElement.Attributes.GetNamedItem("alias").Value);
                if (x.DocumentElement.Attributes.GetNamedItem("alias").Value == u.Language)
                    li.Selected = true;

                userLanguage.Items.Add(li);
            }

            // Console access and disabling
            NoConsole.Checked = u.NoConsole;
            Disabled.Checked = u.Disabled;
            DefaultToLiveEditing.Checked = u.DefaultToLiveEditing;

            PlaceHolder medias = new PlaceHolder();
            mediaPicker.AppAlias = Constants.Applications.Media;
            mediaPicker.TreeAlias = "media";

            if (u.StartMediaId > 0)
                mediaPicker.Value = u.StartMediaId.ToString();
            else
                mediaPicker.Value = "-1";

            medias.Controls.Add(mediaPicker);

            PlaceHolder content = new PlaceHolder();
            contentPicker.AppAlias = Constants.Applications.Content;
            contentPicker.TreeAlias = "content";

            if (u.StartNodeId > 0)
                contentPicker.Value = u.StartNodeId.ToString();
            else
                contentPicker.Value = "-1";

            content.Controls.Add(contentPicker);


            // Add password changer
            passw.Controls.Add(new UserControl().LoadControl(SystemDirectories.Umbraco + "/controls/passwordChanger.ascx"));

            pp.addProperty(ui.Text("user", "username", base.getUser()), uname);
            pp.addProperty(ui.Text("user", "loginname", base.getUser()), lname);
            pp.addProperty(ui.Text("user", "password", base.getUser()), passw);
            pp.addProperty(ui.Text("email", base.getUser()), email);
            pp.addProperty(ui.Text("user", "usertype", base.getUser()), userType);
            pp.addProperty(ui.Text("user", "language", base.getUser()), userLanguage);

            //Media  / content root nodes
            Pane ppNodes = new Pane();
            ppNodes.addProperty(ui.Text("user", "startnode", base.getUser()), content);
            ppNodes.addProperty(ui.Text("user", "mediastartnode", base.getUser()), medias);

            //Generel umrbaco access
            Pane ppAccess = new Pane();
            if (UmbracoSettings.EnableCanvasEditing)
            {
                ppAccess.addProperty(ui.Text("user", "defaultToLiveEditing", base.getUser()), DefaultToLiveEditing);
            }
            ppAccess.addProperty(ui.Text("user", "noConsole", base.getUser()), NoConsole);
            ppAccess.addProperty(ui.Text("user", "disabled", base.getUser()), Disabled);

            //access to which modules... 
            Pane ppModules = new Pane();
            ppModules.addProperty(ui.Text("user", "modules", base.getUser()), lapps);
            ppModules.addProperty(" ", sectionValidator);

            TabPage userInfo = UserTabs.NewTabPage(u.Name);

            userInfo.Controls.Add(pp);
            userInfo.Controls.Add(ppNodes);
            userInfo.Controls.Add(ppAccess);
            userInfo.Controls.Add(ppModules);
            userInfo.Style.Add("text-align", "center");

            userInfo.HasMenu = true;

            ImageButton save = userInfo.Menu.NewImageButton();
            save.ImageUrl = SystemDirectories.Umbraco + "/images/editor/save.gif";
            save.Click += new ImageClickEventHandler(saveUser_Click);

            sectionValidator.ServerValidate += new ServerValidateEventHandler(sectionValidator_ServerValidate);
            sectionValidator.ControlToValidate = lapps.ID;
            sectionValidator.ErrorMessage = ui.Text("errorHandling", "errorMandatoryWithoutTab", ui.Text("user", "modules", base.getUser()), base.getUser());
            sectionValidator.CssClass = "error";

            setupForm();
            setupChannel();

            ClientTools
                .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadUsers>().Tree.Alias)
                .SyncTree(UID.ToString(), IsPostBack);
        }


        void sectionValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            args.IsValid = false;

            if (lapps.SelectedIndex >= 0)
                args.IsValid = true;
        }

        private void setupChannel()
        {
            Channel userChannel;
            try
            {
                userChannel =
                    new Channel(u.Id);
            }
            catch
            {
                userChannel = new Channel();
            }

            // Populate dropdowns
            foreach (DocumentType dt in DocumentType.GetAllAsList())
                cDocumentType.Items.Add(
                    new ListItem(dt.Text, dt.Alias)
                    );

            // populate fields
            ArrayList fields = new ArrayList();
            cDescription.ID = "cDescription";
            cCategories.ID = "cCategories";
            cExcerpt.ID = "cExcerpt";
            cDescription.Items.Add(new ListItem(ui.Text("choose"), ""));
            cCategories.Items.Add(new ListItem(ui.Text("choose"), ""));
            cExcerpt.Items.Add(new ListItem(ui.Text("choose"), ""));

            foreach (PropertyType pt in PropertyType.GetAll())
            {
                if (!fields.Contains(pt.Alias))
                {
                    cDescription.Items.Add(new ListItem(string.Format("{0} ({1})", pt.Name, pt.Alias), pt.Alias));
                    cCategories.Items.Add(new ListItem(string.Format("{0} ({1})", pt.Name, pt.Alias), pt.Alias));
                    cExcerpt.Items.Add(new ListItem(string.Format("{0} ({1})", pt.Name, pt.Alias), pt.Alias));
                    fields.Add(pt.Alias);
                }
            }

            // Handle content and media pickers

            PlaceHolder medias = new PlaceHolder();
            cMediaPicker.AppAlias = Constants.Applications.Media;
            cMediaPicker.TreeAlias = "media";

            if (userChannel.MediaFolder > 0)
                cMediaPicker.Value = userChannel.MediaFolder.ToString();
            else
                cMediaPicker.Value = "-1";

            medias.Controls.Add(cMediaPicker);

            PlaceHolder content = new PlaceHolder();
            cContentPicker.AppAlias = Constants.Applications.Content;
            cContentPicker.TreeAlias = "content";

            if (userChannel.StartNode > 0)
                cContentPicker.Value = userChannel.StartNode.ToString();
            else
                cContentPicker.Value = "-1";

            content.Controls.Add(cContentPicker);


            // Setup the panes
            Pane ppInfo = new Pane();
            ppInfo.addProperty(ui.Text("name", base.getUser()), cName);
            ppInfo.addProperty(ui.Text("user", "startnode", base.getUser()), content);
            ppInfo.addProperty(ui.Text("user", "searchAllChildren", base.getUser()), cFulltree);
            ppInfo.addProperty(ui.Text("user", "mediastartnode", base.getUser()), medias);

            Pane ppFields = new Pane();
            ppFields.addProperty(ui.Text("user", "documentType", base.getUser()), cDocumentType);
            ppFields.addProperty(ui.Text("user", "descriptionField", base.getUser()), cDescription);
            ppFields.addProperty(ui.Text("user", "categoryField", base.getUser()), cCategories);
            ppFields.addProperty(ui.Text("user", "excerptField", base.getUser()), cExcerpt);


            TabPage channelInfo = UserTabs.NewTabPage(ui.Text("user", "contentChannel", base.getUser()));

            channelInfo.Controls.Add(ppInfo);
            channelInfo.Controls.Add(ppFields);

            channelInfo.HasMenu = true;
            ImageButton save = channelInfo.Menu.NewImageButton();
            save.ImageUrl = SystemDirectories.Umbraco + "/images/editor/save.gif";
            save.Click += new ImageClickEventHandler(saveUser_Click);
            save.ID = "save";
            if (!IsPostBack)
            {
                cName.Text = userChannel.Name;
                cDescription.SelectedValue = userChannel.FieldDescriptionAlias;
                cCategories.SelectedValue = userChannel.FieldCategoriesAlias;
                cExcerpt.SelectedValue = userChannel.FieldExcerptAlias;
                cDocumentType.SelectedValue = userChannel.DocumentTypeAlias;
                cFulltree.Checked = userChannel.FullTree;
            }
        }

        /// <summary>
        /// Setups the form.
        /// </summary>
        private void setupForm()
        {

            if (!IsPostBack)
            {
                MembershipUser user = Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].GetUser(u.LoginName, true);
                uname.Text = u.Name;
                lname.Text = (user == null) ? u.LoginName : user.UserName;
                email.Text = (user == null) ? u.Email : user.Email;

                // Prevent users from changing information if logged in through active directory membership provider
                // active directory-mapped accounts have empty passwords by default... so set update user fields to read only
                // this will not be a security issue because empty passwords are not allowed in membership provider. 
                // This might change in version 4.0
                if (string.IsNullOrEmpty(u.GetPassword()))
                {
                    uname.ReadOnly = true;
                    lname.ReadOnly = true;
                    email.ReadOnly = true;
                    passw.Visible = false;
                }

                contentPicker.Value = u.StartNodeId.ToString();
                mediaPicker.Value = u.StartMediaId.ToString();

                // get the current users applications
                string currentUserApps = ";";
                foreach (Application a in CurrentUser.Applications)
                    currentUserApps += a.alias + ";";

                Application[] uapps = u.Applications;
                foreach (Application app in BusinessLogic.Application.getAll())
                {
                    if (CurrentUser.IsRoot() || currentUserApps.Contains(";" + app.alias + ";"))
                    {
                        ListItem li = new ListItem(ui.Text("sections", app.alias), app.alias);
                        if (!IsPostBack) foreach (Application tmp in uapps) if (app.alias == tmp.alias) li.Selected = true;
                        lapps.Items.Add(li);
                    }
                }
            }
        }
        
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            //lapps.SelectionMode = ListSelectionMode.Multiple;
            lapps.RepeatLayout = RepeatLayout.Flow;
            lapps.RepeatDirection = RepeatDirection.Vertical;
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/CMSNode.asmx"));
            //      ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("../webservices/legacyAjaxCalls.asmx"));
            
        }

        /// <summary>
        /// Handles the Click event of the saveUser control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.ImageClickEventArgs"/> instance containing the event data.</param>
        private void saveUser_Click(object sender, ImageClickEventArgs e)
        {
            if (base.IsValid)
            {
                try
                {
                    MembershipUser user = Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].GetUser(u.LoginName, true);


                    string tempPassword = ((controls.passwordChanger)passw.Controls[0]).Password;
                    if (!string.IsNullOrEmpty(tempPassword.Trim()))
                    {
                        // make sure password is not empty
                        if (string.IsNullOrEmpty(u.Password)) u.Password = "default";
                        user.ChangePassword(u.Password, tempPassword);
                    }

                    // Is it using the default membership provider
                    if (Membership.Providers[UmbracoSettings.DefaultBackofficeProvider] is UsersMembershipProvider)
                    {
                        // Save user in membership provider
                        UsersMembershipUser umbracoUser = user as UsersMembershipUser;
                        umbracoUser.FullName = uname.Text.Trim();
                        umbracoUser.Language = userLanguage.SelectedValue;
                        umbracoUser.UserType = UserType.GetUserType(int.Parse(userType.SelectedValue));
                        Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].UpdateUser(umbracoUser);

                        // Save user details
                        u.Email = email.Text.Trim();
                        u.Language = userLanguage.SelectedValue;
                    }
                    else
                    {
                        u.Name = uname.Text.Trim();
                        u.Language = userLanguage.SelectedValue;
                        u.UserType = UserType.GetUserType(int.Parse(userType.SelectedValue));
                        if (!(Membership.Providers[UmbracoSettings.DefaultBackofficeProvider] is ActiveDirectoryMembershipProvider)) Membership.Providers[UmbracoSettings.DefaultBackofficeProvider].UpdateUser(user);
                    }


                    u.LoginName = lname.Text;
                    //u.StartNodeId = int.Parse(startNode.Value);


                    int startNode;
                    if (!int.TryParse(contentPicker.Value, out startNode))
                    {
                        //set to default if nothing is choosen
                        if (u.StartNodeId > 0)
                            startNode = u.StartNodeId;
                        else
                            startNode = -1;
                    }
                    u.StartNodeId = startNode;


                    u.Disabled = Disabled.Checked;
                    u.DefaultToLiveEditing = DefaultToLiveEditing.Checked;
                    u.NoConsole = NoConsole.Checked;
                    //u.StartMediaId = int.Parse(mediaStartNode.Value);


                    int mstartNode;
                    if (!int.TryParse(mediaPicker.Value, out mstartNode))
                    {
                        //set to default if nothing is choosen
                        if (u.StartMediaId > 0)
                            mstartNode = u.StartMediaId;
                        else
                            mstartNode = -1;
                    }
                    u.StartMediaId = mstartNode;

                    u.clearApplications();

                    foreach (ListItem li in lapps.Items)
                    {
                        if (li.Selected) u.addApplication(li.Value);
                    }

                    u.Save();

                    // save data
                    if (cName.Text != "")
                    {
                        Channel c;
                        try
                        {
                            c = new Channel(u.Id);
                        }
                        catch
                        {
                            c = new Channel();
                            c.User = u;
                        }

                        c.Name = cName.Text;
                        c.FullTree = cFulltree.Checked;
                        c.StartNode = int.Parse(cContentPicker.Value);
                        c.MediaFolder = int.Parse(cMediaPicker.Value);
                        c.FieldCategoriesAlias = cCategories.SelectedValue;
                        c.FieldDescriptionAlias = cDescription.SelectedValue;
                        c.FieldExcerptAlias = cExcerpt.SelectedValue;
                        c.DocumentTypeAlias = cDocumentType.SelectedValue;

                        //
                        c.MediaTypeAlias = Constants.Conventions.MediaTypes.Image; // [LK:2013-03-22] This was previously lowercase; unsure if using const will cause an issue.
                        c.MediaTypeFileProperty = Constants.Conventions.Media.File;
                        c.ImageSupport = true;

                        c.Save();

                    }

                    ClientTools.ShowSpeechBubble(speechBubbleIcon.save, ui.Text("speechBubbles", "editUserSaved", base.getUser()), "");
                }
                catch (Exception ex)
                {
                    ClientTools.ShowSpeechBubble(speechBubbleIcon.error, ui.Text("speechBubbles", "editUserError", base.getUser()), "");
                    LogHelper.Error<EditUser>("Exception", ex);
                }
            }
            else
            {
                ClientTools.ShowSpeechBubble(speechBubbleIcon.error, ui.Text("speechBubbles", "editUserError", base.getUser()), "");
            }
        }
    }
}
