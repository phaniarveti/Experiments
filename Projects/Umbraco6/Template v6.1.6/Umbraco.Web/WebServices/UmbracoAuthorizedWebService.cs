﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using Umbraco.Core.Configuration;
using Umbraco.Web.Security;
using umbraco.BasePages;
using umbraco.BusinessLogic;
using Umbraco.Core;
using umbraco.businesslogic.Exceptions;

namespace Umbraco.Web.WebServices
{
    /// <summary>
    /// An abstract web service class that has the methods and properties to correct validate an Umbraco user
    /// </summary>
    public abstract class UmbracoAuthorizedWebService : UmbracoWebService
    {
        protected UmbracoAuthorizedWebService()
            : base()
        {
        }

        protected UmbracoAuthorizedWebService(UmbracoContext umbracoContext)
            : base(umbracoContext)
        {
        }

        private bool _hasValidated = false;

        /// <summary>
        /// Checks if the umbraco context id is valid
        /// </summary>
        /// <param name="currentUmbracoUserContextId"></param>
        /// <returns></returns>
        protected bool ValidateUserContextId(string currentUmbracoUserContextId)
        {
            return UmbracoContext.Security.ValidateUserContextId(currentUmbracoUserContextId);
        }

        /// <summary>
        /// Checks if the username/password credentials are valid
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        protected bool ValidateCredentials(string username, string password)
        {
            return UmbracoContext.Security.ValidateBackOfficeCredentials(username, password);            
        }

        /// <summary>
        /// Validates the user for access to a certain application
        /// </summary>
        /// <param name="app">The application alias.</param>
        /// <param name="throwExceptions">true if an exception should be thrown if authorization fails</param>
        /// <returns></returns>
        protected bool AuthorizeRequest(string app, bool throwExceptions = false)
        {
            //ensure we have a valid user first!
            if (!AuthorizeRequest(throwExceptions)) return false;
                
            //if it is empty, don't validate
            if (app.IsNullOrWhiteSpace())
            {
                return true;
            }
            var hasAccess = UserHasAppAccess(app, UmbracoUser);
            if (!hasAccess && throwExceptions)
                throw new UserAuthorizationException("The user does not have access to the required application");
            return hasAccess;
        }

        /// <summary>
        /// Checks if the specified user as access to the app
        /// </summary>
        /// <param name="app"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        protected bool UserHasAppAccess(string app, User user)
        {
            return Security.UserHasAppAccess(app, user);
        }

        /// <summary>
        /// Checks if the specified user by username as access to the app
        /// </summary>
        /// <param name="app"></param>
        /// <param name="username"></param>
        /// <returns></returns>
        protected bool UserHasAppAccess(string app, string username)
        {
            return Security.UserHasAppAccess(app, username);
        }

        /// <summary>
        /// Returns true if there is a valid logged in user and that ssl is enabled if required
        /// </summary>
        /// <param name="throwExceptions">true if an exception should be thrown if authorization fails</param>
        /// <returns></returns>
        protected bool AuthorizeRequest(bool throwExceptions = false)
        {
            var result = Security.AuthorizeRequest(new HttpContextWrapper(Context), throwExceptions);
            return result == ValidateRequestAttempt.Success;
        }

        /// <summary>
        /// Returns the current user
        /// </summary>
        protected User UmbracoUser
        {
            get
            {
                if (!_hasValidated)
                {
                    Security.ValidateCurrentUser(new HttpContextWrapper(Context));
                    _hasValidated = true;
                }
                return Security.CurrentUser;
            }
        }

    }
}
