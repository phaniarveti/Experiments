using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Defines the ContentService, which is an easy access to operations involving <see cref="IMacro"/>
    /// </summary>
    internal interface IMacroService : IService
    {
        //TODO Possibly create import macro method and ToXml?

        /// <summary>
        /// Gets an <see cref="IMacro"/> object by its alias
        /// </summary>
        /// <param name="alias">Alias to retrieve an <see cref="IMacro"/> for</param>
        /// <returns>An <see cref="IMacro"/> object</returns>
        IMacro GetByAlias(string alias);
        
        /// <summary>
        /// Gets a list all available <see cref="IMacro"/> objects
        /// </summary>
        /// <param name="aliases">Optional array of aliases to limit the results</param>
        /// <returns>An enumerable list of <see cref="IMacro"/> objects</returns>
        IEnumerable<IMacro> GetAll(params string[] aliases);

        /// <summary>
        /// Deletes an <see cref="IMacro"/>
        /// </summary>
        /// <param name="macro"><see cref="IMacro"/> to delete</param>
        /// <param name="userId">Optional id of the user deleting the macro</param>
        void Delete(IMacro macro, int userId = 0);

        /// <summary>
        /// Saves an <see cref="IMacro"/>
        /// </summary>
        /// <param name="macro"><see cref="IMacro"/> to save</param>
        /// <param name="userId">Optional id of the user saving the macro</param>
        void Save(IMacro macro, int userId = 0);

        /// <summary>
        /// Gets a list all available <see cref="IMacroPropertyType"/> plugins
        /// </summary>
        /// <returns>An enumerable list of <see cref="IMacroPropertyType"/> objects</returns>
        IEnumerable<IMacroPropertyType> GetMacroPropertyTypes();

        /// <summary>
        /// Gets an <see cref="IMacroPropertyType"/> by its alias
        /// </summary>
        /// <param name="alias">Alias to retrieve an <see cref="IMacroPropertyType"/> for</param>
        /// <returns>An <see cref="IMacroPropertyType"/> object</returns>
        IMacroPropertyType GetMacroPropertyTypeByAlias(string alias);
    }
}