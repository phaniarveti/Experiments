﻿using System.Collections.Generic;
using Umbraco.Core.Persistence.Mappers;

namespace Umbraco.Core.Models.Membership
{
    /// <summary>
    /// Defines the interface for a <see cref="User"/>
    /// </summary>
    /// <remarks>Will be left internal until a proper Membership implementation is part of the roadmap</remarks>
    internal interface IUser : IMembershipUser, IUserProfile
    {
        new object Id { get; set; }
        //string Name { get; set; }
        int SessionTimeout { get; set; }
        int StartContentId { get; set; }
        int StartMediaId { get; set; }
        string Language { get; set; }
        
        bool NoConsole { get; set; }
        IUserType UserType { get; }

        /// <summary>
        /// The default permissions for the user
        /// </summary>
        /// <remarks>
        /// The default permissions are assigned to the user object based on the user type's default permissions
        /// </remarks> 
        string DefaultPermissions { get; }
    }

    internal interface IUserProfile : IProfile
    {
        IEnumerable<string> AllowedSections { get; }
        void RemoveAllowedSection(string sectionAlias);
        void AddAllowedSection(string sectionAlias);
    }
}