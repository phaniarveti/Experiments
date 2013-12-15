﻿using System.Configuration;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    internal class SecurityElement : ConfigurationElement, ISecuritySection
    {
        [ConfigurationProperty("keepUserLoggedIn")]
        internal InnerTextConfigurationElement<bool> KeepUserLoggedIn
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>)this["keepUserLoggedIn"],
                    //set the default
                    true);   
            }
        }

        [ConfigurationProperty("hideDisabledUsersInBackoffice")]
        internal InnerTextConfigurationElement<bool> HideDisabledUsersInBackoffice
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<bool>(
                    (InnerTextConfigurationElement<bool>)this["hideDisabledUsersInBackoffice"],
                    //set the default
                    false);                          
            }
        }

        [ConfigurationProperty("authCookieName")]
        internal InnerTextConfigurationElement<string> AuthCookieName
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<string>(
                    (InnerTextConfigurationElement<string>)this["authCookieName"],
                    //set the default
                    "UMB_UCONTEXT");                
            }
        }

        [ConfigurationProperty("authCookieDomain")]
        internal InnerTextConfigurationElement<string> AuthCookieDomain
        {
            get
            {
                return new OptionalInnerTextConfigurationElement<string>(
                    (InnerTextConfigurationElement<string>)this["authCookieDomain"],
                    //set the default
                    null);                    
            }
        }

        bool ISecuritySection.KeepUserLoggedIn
        {
            get { return KeepUserLoggedIn; }
        }

        bool ISecuritySection.HideDisabledUsersInBackoffice
        {
            get { return HideDisabledUsersInBackoffice; }
        }

        string ISecuritySection.AuthCookieName
        {
            get { return AuthCookieName; }
        }

        string ISecuritySection.AuthCookieDomain
        {
            get { return AuthCookieDomain; }
        }
    }
}