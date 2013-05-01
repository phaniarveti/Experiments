using System;
using System.Web;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Represents the UserService, which is an easy access to operations involving <see cref="IProfile"/> and eventually Users and Members.
    /// </summary>
    internal class UserService : IUserService
    {
	    private readonly RepositoryFactory _repositoryFactory;
        private readonly IDatabaseUnitOfWorkProvider _uowProvider;

        public UserService(RepositoryFactory repositoryFactory) : this(new PetaPocoUnitOfWorkProvider(), repositoryFactory)
        {}

        public UserService(IDatabaseUnitOfWorkProvider provider) : this(provider, new RepositoryFactory())
        {}

        public UserService(IDatabaseUnitOfWorkProvider provider, RepositoryFactory repositoryFactory)
        {
			_repositoryFactory = repositoryFactory;
			_uowProvider = provider;
        }

        #region Implementation of IUserService

        /// <summary>
        /// Gets an <see cref="IProfile"/> for the current BackOffice User.
        /// </summary>
        /// <param name="httpContext">HttpContext to fetch the user through</param>
        /// <returns><see cref="IProfile"/> containing the Name and Id of the logged in BackOffice User</returns>
        public IProfile GetCurrentBackOfficeUser(HttpContextBase httpContext)
        {
            Mandate.That(httpContext != null,
                         () =>
                         new ArgumentException(
                             "The HttpContext which is used to retrieve information about the currently logged in backoffice user was null and can therefor not be used",
                             "HttpContextBase"));
            if (httpContext == null) return null;

            var cookie = httpContext.Request.Cookies["UMB_UCONTEXT"];
            Mandate.That(cookie != null, () => new ArgumentException("The Cookie containing the UserContext Guid Id was null", "Cookie"));
            if (cookie == null) return null;

            string contextId = cookie.Value;
            string cacheKey = string.Concat("UmbracoUserContext", contextId);

            int userId = 0;

            if(HttpRuntime.Cache[cacheKey] == null)
            {
                using (var uow = _uowProvider.GetUnitOfWork())
                {
					userId =
						uow.Database.ExecuteScalar<int>(
							"select userID from umbracoUserLogins where contextID = @ContextId",
							new { ContextId = new Guid(contextId) });

					HttpRuntime.Cache.Insert(cacheKey, userId,
											 null,
											 System.Web.Caching.Cache.NoAbsoluteExpiration,
											 new TimeSpan(0, (int)(Umbraco.Core.Configuration.GlobalSettings.TimeOutInMinutes / 10), 0));    
                }                
            }
            else
            {
                userId = (int) HttpRuntime.Cache[cacheKey];
            }

            var profile = GetProfileById(userId);
            return profile;
        }

        /// <summary>
        /// Gets an <see cref="IProfile"/> for the current BackOffice User.
        /// </summary>
        /// <remarks>
        /// Requests the current HttpContext, so this method will only work in a web context.
        /// </remarks>
        /// <returns><see cref="IProfile"/> containing the Name and Id of the logged in BackOffice User</returns>
        public IProfile GetCurrentBackOfficeUser()
        {
            var context = HttpContext.Current;
            Mandate.That<Exception>(context != null);

            var wrapper = new HttpContextWrapper(context);
            return GetCurrentBackOfficeUser(wrapper);
        }

        /// <summary>
        /// Gets an IProfile by User Id.
        /// </summary>
        /// <param name="id">Id of the User to retrieve</param>
        /// <returns><see cref="IProfile"/></returns>
        public IProfile GetProfileById(int id)
        {
            using (var repository = _repositoryFactory.CreateUserRepository(_uowProvider.GetUnitOfWork()))
            {
                return repository.GetProfileById(id);
            }
        }

        #endregion
    }
}