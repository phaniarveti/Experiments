using System.Globalization;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public interface ILanguage : IAggregateRoot
    {
        /// <summary>
        /// Gets or sets the Iso Code for the Language
        /// </summary>
        [DataMember]
        string IsoCode { get; set; }

        /// <summary>
        /// Gets or sets the Culture Name for the Language
        /// </summary>
        [DataMember]
        string CultureName { get; set; }

        /// <summary>
        /// Returns a <see cref="CultureInfo"/> object for the current Language
        /// </summary>
        [IgnoreDataMember]
        CultureInfo CultureInfo { get; }
    }
}