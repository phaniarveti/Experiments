using System.Web;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence
{
	/// <summary>
	/// The default implementation for the IDatabaseFactory
	/// </summary>
	/// <remarks>
	/// If we are running in an http context
	/// it will create one per context, otherwise it will be a global singleton object which is NOT thread safe
	/// since we need (at least) a new instance of the database object per thread.
	/// </remarks>
	internal class DefaultDatabaseFactory : DisposableObject, IDatabaseFactory
	{
		private readonly string _connectionStringName;
        private readonly string _connectionString;
		private readonly string _providerName;
		private static volatile UmbracoDatabase _globalInstance = null;
		private static readonly object Locker = new object();

		/// <summary>
		/// Default constructor initialized with the GlobalSettings.UmbracoConnectionName
		/// </summary>
		public DefaultDatabaseFactory() : this(GlobalSettings.UmbracoConnectionName)
		{
			
		}

		/// <summary>
		/// Constructor accepting custom connection string
		/// </summary>
		/// <param name="connectionStringName">Name of the connection string in web.config</param>
		public DefaultDatabaseFactory(string connectionStringName)
		{
			Mandate.ParameterNotNullOrEmpty(connectionStringName, "connectionStringName");
			_connectionStringName = connectionStringName;
		}

		/// <summary>
		/// Constructor accepting custom connectino string and provider name
		/// </summary>
		/// <param name="connectionString">Connection String to use with Database</param>
		/// <param name="providerName">Database Provider for the Connection String</param>
		public DefaultDatabaseFactory(string connectionString, string providerName)
		{
			Mandate.ParameterNotNullOrEmpty(connectionString, "connectionString");
			Mandate.ParameterNotNullOrEmpty(providerName, "providerName");
			_connectionString = connectionString;
			_providerName = providerName;
		}

		public UmbracoDatabase CreateDatabase()
		{
			//no http context, create the singleton global object
			if (HttpContext.Current == null)
			{
				if (_globalInstance == null)
				{
					lock (Locker)
					{
						//double check
						if (_globalInstance == null)
						{
						    _globalInstance = string.IsNullOrEmpty(_providerName) == false && string.IsNullOrEmpty(_providerName) == false
						                          ? new UmbracoDatabase(_connectionString, _providerName)
						                          : new UmbracoDatabase(_connectionStringName);
						}
					}
				}
				return _globalInstance;
			}

			//we have an http context, so only create one per request
			if (!HttpContext.Current.Items.Contains(typeof(DefaultDatabaseFactory)))
			{
				HttpContext.Current.Items.Add(typeof (DefaultDatabaseFactory),
                                              string.IsNullOrEmpty(_providerName) == false && string.IsNullOrEmpty(_providerName) == false
					                              ? new UmbracoDatabase(_connectionString, _providerName)
					                              : new UmbracoDatabase(_connectionStringName));
			}
			return (UmbracoDatabase)HttpContext.Current.Items[typeof(DefaultDatabaseFactory)];
		}

		protected override void DisposeResources()
		{
			if (HttpContext.Current == null)
			{
				_globalInstance.Dispose();
			}
			else
			{
				if (HttpContext.Current.Items.Contains(typeof(DefaultDatabaseFactory)))
				{
					((UmbracoDatabase)HttpContext.Current.Items[typeof(DefaultDatabaseFactory)]).Dispose();
				}
			}
		}
	}
}