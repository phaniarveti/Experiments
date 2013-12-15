using System;
using System.Data;
using System.Web;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using Umbraco.Core;

namespace umbraco.BasePages
{
    /// <summary>
    /// umbraco.BasePages.BasePage is the default page type for the umbraco backend.
    /// The basepage keeps track of the current user and the page context. But does not 
    /// Restrict access to the page itself.
    /// The keep the page secure, the umbracoEnsuredPage class should be used instead
    /// </summary>
    public class BasePage : System.Web.UI.Page
    {
        private User _user;
        private bool _userisValidated = false;
        private ClientTools _clientTools;

        // ticks per minute 600,000,000 
        private const long TicksPrMinute = 600000000;
        private static readonly int UmbracoTimeOutInMinutes = GlobalSettings.TimeOutInMinutes;

        /// <summary>
        /// The path to the umbraco root folder
        /// </summary>
        protected string UmbracoPath = SystemDirectories.Umbraco;

        /// <summary>
        /// The current user ID
        /// </summary>
        protected int uid = 0;

        /// <summary>
        /// The page timeout in seconds.
        /// </summary>
        protected long timeout = 0;

        /// <summary>
        /// Gets the SQL helper.
        /// </summary>
        /// <value>The SQL helper.</value>
        protected static ISqlHelper SqlHelper
        {
            get { return umbraco.BusinessLogic.Application.SqlHelper; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePage"/> class.
        /// </summary>
        public BasePage()
        {
        }

        /// <summary>
        /// Returns the current BasePage for the current request. 
        /// This assumes that the current page is a BasePage, otherwise, returns null;
        /// </summary>
        public static BasePage Current
        {
            get
            {
                return HttpContext.Current.CurrentHandler as BasePage;
            }
        }

	    private UrlHelper _url;
		/// <summary>
		/// Returns a UrlHelper
		/// </summary>
		/// <remarks>
		/// This URL helper is created without any route data and an empty request context
		/// </remarks>
	    public UrlHelper Url
	    {
		    get { return _url ?? (_url = new UrlHelper(new RequestContext(new HttpContextWrapper(Context), new RouteData()))); }
	    }

        /// <summary>
        /// Returns a refernce of an instance of ClientTools for access to the pages client API
        /// </summary>
        public ClientTools ClientTools
        {
            get
            {
                if (_clientTools == null)
                    _clientTools = new ClientTools(this);
                return _clientTools;
            }
        }

        [Obsolete("Use ClientTools instead")]
        public void RefreshPage(int Seconds)
        {
            ClientTools.RefreshAdmin(Seconds);
        }

        private void ValidateUser()
        {
            if ((umbracoUserContextID != ""))
            {
                uid = GetUserId(umbracoUserContextID);
                timeout = GetTimeout(umbracoUserContextID);

                if (timeout > DateTime.Now.Ticks)
                {
                    _user = BusinessLogic.User.GetUser(uid);

                    // Check for console access
                    if (_user.Disabled || (_user.NoConsole && GlobalSettings.RequestIsInUmbracoApplication(HttpContext.Current) && !GlobalSettings.RequestIsLiveEditRedirector(HttpContext.Current)))
                    {
                        throw new ArgumentException("You have no priviledges to the umbraco console. Please contact your administrator");
                    }
                    else
                    {
                        _userisValidated = true;
                        UpdateLogin();
                    }

                }
                else
                {
                    throw new ArgumentException("User has timed out!!");
                }
            }
            else
            {
                throw new InvalidOperationException("The user has no umbraco contextid - try logging in");
            }

        }

        /// <summary>
        /// Gets the user id.
        /// </summary>
        /// <param name="umbracoUserContextID">The umbraco user context ID.</param>
        /// <returns></returns>
        public static int GetUserId(string umbracoUserContextID)
        {

            Guid contextId;
            if (!Guid.TryParse(umbracoUserContextID, out contextId))
            {
                return -1;
            }

            try
            {
                if (HttpRuntime.Cache["UmbracoUserContext" + umbracoUserContextID] == null)
                {
                    var uId = SqlHelper.ExecuteScalar<int?>(
                        "select userID from umbracoUserLogins where contextID = @contextId",
                        SqlHelper.CreateParameter("@contextId", new Guid(umbracoUserContextID)));
                    if (!uId.HasValue)
                    {
                        return -1;
                    }

                    HttpRuntime.Cache.Insert(
                        "UmbracoUserContext" + umbracoUserContextID,
                        uId.Value,
                        null,
                        System.Web.Caching.Cache.NoAbsoluteExpiration,
                        new TimeSpan(0, (int) (UmbracoTimeOutInMinutes/10), 0));
                }

                return (int)HttpRuntime.Cache["UmbracoUserContext" + umbracoUserContextID];

            }
            catch
            {
                return -1;
            }
        }


        // Added by NH to use with webservices authentications
        /// <summary>
        /// Validates the user context ID.
        /// </summary>
        /// <param name="currentUmbracoUserContextID">The umbraco user context ID.</param>
        /// <returns></returns>
        public static bool ValidateUserContextID(string currentUmbracoUserContextID)
        {
            if (!currentUmbracoUserContextID.IsNullOrWhiteSpace())
            {
                var uid = GetUserId(currentUmbracoUserContextID);
                var timeout = GetTimeout(currentUmbracoUserContextID);

                if (timeout > DateTime.Now.Ticks)
                {
                    return true;
                }
	            var user = BusinessLogic.User.GetUser(uid);
                //TODO: We don't actually log anyone out here, not sure why we're logging ??
				LogHelper.Info<BasePage>("User {0} (Id:{1}) logged out", () => user.Name, () => user.Id);
            }
            return false;
        }

        private static long GetTimeout(string umbracoUserContextID)
        {
            if (System.Web.HttpRuntime.Cache["UmbracoUserContextTimeout" + umbracoUserContextID] == null)
            {
                System.Web.HttpRuntime.Cache.Insert(
                    "UmbracoUserContextTimeout" + umbracoUserContextID,
                        GetTimeout(true),
                    null,
                    DateTime.Now.AddMinutes(UmbracoTimeOutInMinutes / 10), System.Web.Caching.Cache.NoSlidingExpiration);


            }

            object timeout = HttpRuntime.Cache["UmbracoUserContextTimeout" + umbracoUserContextID];
            if (timeout != null)
                return (long)timeout;

            return 0;

        }

        public static long GetTimeout(bool byPassCache)
        {
            if (UmbracoSettings.KeepUserLoggedIn)
                RenewLoginTimeout();

            if (byPassCache)
            {
                return SqlHelper.ExecuteScalar<long>("select timeout from umbracoUserLogins where contextId=@contextId",
                                                          SqlHelper.CreateParameter("@contextId", new Guid(umbracoUserContextID))
                                        );
            }
            else
                return GetTimeout(umbracoUserContextID);
        }

        // Changed to public by NH to help with webservice authentication
        /// <summary>
        /// Gets or sets the umbraco user context ID.
        /// </summary>
        /// <value>The umbraco user context ID.</value>
        public static string umbracoUserContextID
        {
            get
            {
                // zb-00004 #29956 : refactor cookies names & handling
                if (StateHelper.Cookies.HasCookies && StateHelper.Cookies.UserContext.HasValue)
                    return StateHelper.Cookies.UserContext.GetValue();
                else
                {
                    try
                    {
                        string encTicket = StateHelper.Cookies.UserContext.GetValue();
                        if (!String.IsNullOrEmpty(encTicket))
                            return FormsAuthentication.Decrypt(encTicket).UserData;
                    }
                    catch (HttpException ex)
                    {
                        // we swallow this type of exception as it happens if a legacy (pre 4.8.1) cookie is set
                    }
                    catch (ArgumentException ex)
                    {
                        // we swallow this one because it's 99.99% certaincy is legacy based. We'll still log it, though
                        LogHelper.Error<BasePage>("An error occurred reading auth cookie value", ex);

                    }
                }

                return "";
            }
            set
            {
                // zb-00004 #29956 : refactor cookies names & handling
                if (StateHelper.Cookies.HasCookies)
                {
                    // Clearing all old cookies before setting a new one.
                    if (StateHelper.Cookies.UserContext.HasValue)
                        StateHelper.Cookies.ClearAll();

                    if (!String.IsNullOrEmpty(value))
                    {
                        FormsAuthenticationTicket ticket = new FormsAuthenticationTicket(1,
                        value,
                        DateTime.Now,
                        DateTime.Now.AddDays(1),
                        false,
                        value,
                        FormsAuthentication.FormsCookiePath);

                        // Encrypt the ticket.
                        string encTicket = FormsAuthentication.Encrypt(ticket);


                        // Create new cookie.
                    StateHelper.Cookies.UserContext.SetValue(value, 1);
                        

                    } else
                    {
                        StateHelper.Cookies.UserContext.Clear();
                    }
                }
            }
        }


        /// <summary>
        /// Clears the login.
        /// </summary>
        public void ClearLogin()
        {
            DeleteLogin();
            umbracoUserContextID = "";
        }

        private void DeleteLogin()
        {
            // Added try-catch in case login doesn't exist in the database
            // Either due to old cookie or running multiple sessions on localhost with different port number
            try
            {
                SqlHelper.ExecuteNonQuery(
                "DELETE FROM umbracoUserLogins WHERE contextId = @contextId",
                SqlHelper.CreateParameter("@contextId", umbracoUserContextID));
            }
            catch (Exception ex)
            {
                LogHelper.Error<BasePage>(string.Format("Login with contextId {0} didn't exist in the database", umbracoUserContextID), ex);
            }
        }

        private void UpdateLogin()
        {
            // only call update if more than 1/10 of the timeout has passed
            if (timeout - (((TicksPrMinute * UmbracoTimeOutInMinutes) * 0.8)) < DateTime.Now.Ticks)
                SqlHelper.ExecuteNonQuery(
                    "UPDATE umbracoUserLogins SET timeout = @timeout WHERE contextId = @contextId",
                    SqlHelper.CreateParameter("@timeout", DateTime.Now.Ticks + (TicksPrMinute * UmbracoTimeOutInMinutes)),
                    SqlHelper.CreateParameter("@contextId", umbracoUserContextID));
        }

        public static void RenewLoginTimeout()
        {
            // only call update if more than 1/10 of the timeout has passed
            SqlHelper.ExecuteNonQuery(
                "UPDATE umbracoUserLogins SET timeout = @timeout WHERE contextId = @contextId",
                SqlHelper.CreateParameter("@timeout", DateTime.Now.Ticks + (TicksPrMinute * UmbracoTimeOutInMinutes)),
                SqlHelper.CreateParameter("@contextId", umbracoUserContextID));
        }

        /// <summary>
        /// Logs a user in.
        /// </summary>
        /// <param name="u">The user</param>
        public static void doLogin(User u)
        {
            Guid retVal = Guid.NewGuid();
            SqlHelper.ExecuteNonQuery(
                                      "insert into umbracoUserLogins (contextID, userID, timeout) values (@contextId,'" + u.Id + "','" +
                                      (DateTime.Now.Ticks + (TicksPrMinute * UmbracoTimeOutInMinutes)).ToString() +
                                      "') ",
                                      SqlHelper.CreateParameter("@contextId", retVal));
            umbracoUserContextID = retVal.ToString();

			LogHelper.Info<BasePage>("User {0} (Id: {1}) logged in", () => u.Name, () => u.Id);

        }


        /// <summary>
        /// Gets the user.
        /// </summary>
        /// <returns></returns>
        public User getUser()
        {
            if (!_userisValidated) ValidateUser();
            return _user;
        }

        /// <summary>
        /// Ensures the page context.
        /// </summary>
        public void ensureContext()
        {
            ValidateUser();
        }

        [Obsolete("Use ClientTools instead")]
        public void speechBubble(speechBubbleIcon i, string header, string body)
        {
            ClientTools.ShowSpeechBubble(i, header, body);
        }

        //[Obsolete("Use ClientTools instead")]
        //public void reloadParentNode() 
        //{
        //    ClientTools.ReloadParentNode(true);
        //}

        /// <summary>
        /// a collection of available speechbubble icons
        /// </summary>
        public enum speechBubbleIcon
        {
            /// <summary>
            /// Save icon
            /// </summary>
            save,
            /// <summary>
            /// Info icon
            /// </summary>
            info,
            /// <summary>
            /// Error icon
            /// </summary>
            error,
            /// <summary>
            /// Success icon
            /// </summary>
            success,
            /// <summary>
            /// Warning icon
            /// </summary>
            warning
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"></see> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"></see> object that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Request.IsSecureConnection && GlobalSettings.UseSSL)
            {
                string serverName = HttpUtility.UrlEncode(Request.ServerVariables["SERVER_NAME"]);
                Response.Redirect(string.Format("https://{0}{1}", serverName, Request.FilePath));
            }
        }

        /// <summary>
        /// Override client target.
        /// </summary>
        [Obsolete("This is no longer supported")]
        public bool OverrideClientTarget = false;
    }
}
