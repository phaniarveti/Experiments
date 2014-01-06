﻿using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using umbraco.uicontrols;
using umbraco.IO;
using umbraco.cms.helpers;
using umbraco.BusinessLogic;

namespace umbraco.cms.presentation
{
    /// <summary>
    /// Summary description for dashboard.
    /// </summary>
    public partial class dashboard : BasePages.UmbracoEnsuredPage
    {


        private string _section = "";

        protected void Page_Load(object sender, System.EventArgs e)
        {
            // Put user code to initialize the page here
            Panel2.Text = ui.Text("dashboard", "welcome", base.getUser()) + " " + this.getUser().Name;
        }

        private Control CreateDashBoardWrapperControl(Control control)
        {
            PlaceHolder placeHolder = new PlaceHolder();
            placeHolder.Controls.Add(new LiteralControl("<br/><fieldSet style=\"padding: 5px\">"));
            placeHolder.Controls.Add(control);
            placeHolder.Controls.Add(new LiteralControl("</fieldSet>"));
            return placeHolder;
        }

        override protected void OnInit(EventArgs e)
        {

            base.OnInit(e);
            // Load dashboard content
            if (helper.Request("app") != "")
                _section = helper.Request("app");
            else if (getUser().Applications.Length > 0)
                _section = "default";
            else
                _section = getUser().Applications[0].alias;

            XmlDocument dashBoardXml = new XmlDocument();
            dashBoardXml.Load(IOHelper.MapPath(SystemFiles.DashboardConfig));

            // test for new tab interface
            foreach (XmlNode section in dashBoardXml.DocumentElement.SelectNodes("//section [areas/area = '" + _section.ToLower() + "']"))
            {
                if (section != null && validateAccess(section))
                {
                    Panel2.Visible = false;
                    dashboardTabs.Visible = true;

                    foreach (XmlNode entry in section.SelectNodes("./tab"))
                    {
                        if (validateAccess(entry))
                        {
                            TabPage tab = dashboardTabs.NewTabPage(entry.Attributes.GetNamedItem("caption").Value);
                            tab.HasMenu = true;
                            tab.Style.Add("padding", "0 10px");

                            foreach (XmlNode uc in entry.SelectNodes("./control"))
                            {
                                if (validateAccess(uc))
                                {
                                    string control = getFirstText(uc).Trim(' ', '\r', '\n');
                                    string path = IOHelper.FindFile(control);


                                    try
                                    {
                                        Control c = LoadControl(path);

                                        // set properties
                                        Type type = c.GetType();
                                        if (type != null)
                                        {
                                            foreach (XmlAttribute att in uc.Attributes)
                                            {
                                                string attributeName = att.Name;
                                                string attributeValue = parseControlValues(att.Value).ToString();
                                                // parse special type of values


                                                PropertyInfo prop = type.GetProperty(attributeName);
                                                if (prop == null)
                                                {
                                                    continue;
                                                }

                                                prop.SetValue(c, Convert.ChangeType(attributeValue, prop.PropertyType),
                                                              null);

                                            }
                                        }

                                        //resolving files from dashboard config which probably does not map to a virtual fi
                                        tab.Controls.Add(AddPanel(uc, c));
                                    }
                                    catch (Exception ee)
                                    {
                                        tab.Controls.Add(
                                            new LiteralControl(
                                                "<p class=\"umbracoErrorMessage\">Could not load control: '" + path +
                                                "'. <br/><span class=\"guiDialogTiny\"><strong>Error message:</strong> " +
                                                ee.ToString() + "</span></p>"));
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {


                    foreach (
                        XmlNode entry in dashBoardXml.SelectNodes("//entry [@section='" + _section.ToLower() + "']"))
                    {
                        PlaceHolder placeHolder = new PlaceHolder();
                        if (entry == null || entry.FirstChild == null)
                        {
                            placeHolder.Controls.Add(
                                CreateDashBoardWrapperControl(new LiteralControl("Error loading DashBoard Content")));
                        }
                        else
                        {
                            string path = IOHelper.FindFile(entry.FirstChild.Value);

                            try
                            {
                                placeHolder.Controls.Add(CreateDashBoardWrapperControl(LoadControl(path)));
                            }
                            catch (Exception err)
                            {
                                Trace.Warn("Dashboard", string.Format("error loading control '{0}'",
                                                                      path), err);
                                placeHolder.Controls.Clear();
                                placeHolder.Controls.Add(CreateDashBoardWrapperControl(new LiteralControl(string.Format(
                                    "Error loading DashBoard Content '{0}'; {1}", path,
                                    err.Message))));
                            }
                        }
                        dashBoardContent.Controls.Add(placeHolder);
                    }
                }
            }
        }

        private object parseControlValues(string value)
        {
            if (!String.IsNullOrEmpty(value))
            {
                if (value.StartsWith("[#"))
                {
                    value = value.Substring(2, value.Length - 3).ToLower();
                    switch (value)
                    {
                        case "usertype":
                            return BusinessLogic.User.GetCurrent().UserType.Alias;
                        case "username":
                            return BusinessLogic.User.GetCurrent().Name;
                        case "userlogin":
                            return BusinessLogic.User.GetCurrent().LoginName;
                        case "usercontentstartnode":
                            return BusinessLogic.User.GetCurrent().StartNodeId;
                        case "usermediastartnode":
                            return BusinessLogic.User.GetCurrent().StartMediaId;
                        default:
                            return value;
                    }
                }
            }

            return value;
        }

        private Control AddPanel(XmlNode node, Control c)
        {
            LiteralControl hide = AddShowOnceLink(node);
            if (node.Attributes.GetNamedItem("addPanel") != null &&
                node.Attributes.GetNamedItem("addPanel").Value == "true")
            {
                Pane p = new Pane();
                PropertyPanel pp = new PropertyPanel();
                if (node.Attributes.GetNamedItem("panelCaption") != null &&
                    !String.IsNullOrEmpty(node.Attributes.GetNamedItem("panelCaption").Value))
                {
                    string panelCaption = node.Attributes.GetNamedItem("panelCaption").Value;
                    if (panelCaption.StartsWith("#"))
                    {
                        panelCaption = ui.Text(panelCaption.Substring(1));
                    }
                    pp.Text = panelCaption;
                }
                // check for hide in the future link
                if (!String.IsNullOrEmpty(hide.Text))
                {
                    pp.Controls.Add(hide);
                }
                pp.Controls.Add(c);
                p.Controls.Add(pp);
                return p;
            }

            if (!String.IsNullOrEmpty(hide.Text))
            {
                PlaceHolder ph = new PlaceHolder();
                ph.Controls.Add(hide);
                ph.Controls.Add(c);
                return ph;
            }
            else
            {
                return c;
            }
        }

        private LiteralControl AddShowOnceLink(XmlNode node)
        {
            LiteralControl onceLink = new LiteralControl();
            if (node.Attributes.GetNamedItem("showOnce") != null &&
                node.Attributes.GetNamedItem("showOnce").Value.ToLower() == "true")
            {
                onceLink.Text = "<a class=\"dashboardHideLink\" onclick=\"if(confirm('Are you sure you want remove this dashboard item?')){jQuery.cookie('" + generateCookieKey(node) + "','true'); jQuery(this).closest('.propertypane').fadeOut();return false;}\">" + ui.Text("dashboard", "dontShowAgain") + "</a>";
            }
            return onceLink;
        }

        private string getFirstText(XmlNode node)
        {
            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.NodeType == XmlNodeType.Text)
                    return n.Value;
            }

            return "";
        }

        private string generateCookieKey(XmlNode node)
        {
            string key = String.Empty;
            if (node.Name.ToLower() == "control")
            {
                key = node.FirstChild.Value + "_" + generateCookieKey(node.ParentNode);
            }
            else if (node.Name.ToLower() == "tab")
            {
                key = node.Attributes.GetNamedItem("caption").Value;
            }

            return Casing.SafeAlias(key.ToLower());
        }

        private bool validateAccess(XmlNode node)
        {
            // check if this area should be shown at all
            string onlyOnceValue = StateHelper.GetCookieValue(generateCookieKey(node));
            if (!String.IsNullOrEmpty(onlyOnceValue))
            {
                return false;
            }

            // the root user can always see everything
            if (CurrentUser.IsRoot())
            {
                return true;
            }
            else if (node != null)
            {
                XmlNode accessRules = node.SelectSingleNode("access");
                bool retVal = true;
                if (accessRules != null && accessRules.HasChildNodes)
                {
                    string currentUserType = CurrentUser.UserType.Alias.ToLowerInvariant();
                    
                    //Update access rules so we'll be comparing lower case to lower case always

                    var denies = accessRules.SelectNodes("deny");
                    foreach (XmlNode deny in denies)
                    {
                        deny.InnerText = deny.InnerText.ToLowerInvariant();
                    }

                    var grants = accessRules.SelectNodes("grant");
                    foreach (XmlNode grant in grants)
                    {
                        grant.InnerText = grant.InnerText.ToLowerInvariant();
                    }

                    string allowedSections = ",";
                    foreach (BusinessLogic.Application app in CurrentUser.Applications)
                    {
                        allowedSections += app.alias.ToLower() + ",";
                    }
                    XmlNodeList grantedTypes = accessRules.SelectNodes("grant");
                    XmlNodeList grantedBySectionTypes = accessRules.SelectNodes("grantBySection");
                    XmlNodeList deniedTypes = accessRules.SelectNodes("deny");

                    // if there's a grant type, everyone who's not granted is automatically denied
                    if (grantedTypes.Count > 0 || grantedBySectionTypes.Count > 0)
                    {
                        retVal = false;
                        if (grantedBySectionTypes.Count > 0 && accessRules.SelectSingleNode(String.Format("grantBySection [contains('{0}', concat(',',.,','))]", allowedSections)) != null)
                        {
                            retVal = true;
                        }
                        else if (grantedTypes.Count > 0 && accessRules.SelectSingleNode(String.Format("grant [. = '{0}']", currentUserType)) != null)
                        {
                            retVal = true;
                        }
                    }
                    // if the current type of user is denied we'll say nay
                    if (deniedTypes.Count > 0 && accessRules.SelectSingleNode(String.Format("deny [. = '{0}']", currentUserType)) != null)
                    {
                        retVal = false;
                    }

                }

                return retVal;
            }
            return false;
        }

        /// <summary>
        /// Panel2 control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.UmbracoPanel Panel2;

        /// <summary>
        /// dashBoardContent control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::System.Web.UI.WebControls.PlaceHolder dashBoardContent;

        /// <summary>
        /// dashboardTabs control.
        /// </summary>
        /// <remarks>
        /// Auto-generated field.
        /// To modify move field declaration from designer file to code-behind file.
        /// </remarks>
        protected global::umbraco.uicontrols.TabView dashboardTabs;

    }
}
