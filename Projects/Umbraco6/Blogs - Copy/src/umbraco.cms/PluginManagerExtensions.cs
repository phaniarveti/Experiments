using System;
using System.Collections.Generic;
using Umbraco.Core;
using umbraco.BusinessLogic.Actions;
using umbraco.businesslogic;
using umbraco.cms.businesslogic.macro;
using umbraco.cms.businesslogic.media;
using umbraco.interfaces;

namespace umbraco.cms
{
	/// <summary>
	/// Extension methods for the PluginTypeResolver
	/// </summary>
	public static class PluginManagerExtensions
	{

		/// <summary>
		/// Returns all available IActionHandler in application
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveActionHandlers(this PluginManager resolver)
		{
			return resolver.ResolveTypes<IActionHandler>();
		}

		/// <summary>
		/// Returns all available IActions in application
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveActions(this PluginManager resolver)
		{
			return resolver.ResolveTypes<IAction>();
		}

		/// <summary>
		/// Returns all available IDataType in application
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveMacroEngines(this PluginManager resolver)
		{
			return resolver.ResolveTypes<IMacroEngine>();
		}

		/// <summary>
		/// Returns all available IMediaFactory in application
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<Type> ResolveMediaFactories(this PluginManager resolver)
		{
			return resolver.ResolveTypes<IMediaFactory>();
		}

		

	}
}