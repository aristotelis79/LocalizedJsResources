using Nop.Core;

namespace Nop.Plugin.Misc.Localized.Javascript.Domain
{
    public class JavaScriptResourceRecord : BaseEntity
    {

        /// <summary>
        /// Gets or sets the language identifier
        /// </summary>
        public int LanguageId { get; set; }

        /// <summary>
        /// Gets or sets the resource name
        /// </summary>
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or sets the resource value
        /// </summary>
        public string ResourceValue { get; set; }
        
        ///// <summary>
        ///// Gets or sets the language
        ///// </summary>
        //public virtual Language Language { get; set; }
    }
}