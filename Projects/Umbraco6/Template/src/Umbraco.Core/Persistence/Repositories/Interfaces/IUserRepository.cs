﻿using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Persistence.Repositories
{
    internal interface IUserRepository : IRepositoryQueryable<int, IUser>
    {
        IProfile GetProfileById(int id);
    }
}