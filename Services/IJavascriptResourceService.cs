using System.Collections.Generic;
using Nop.Plugin.Misc.Localized.Javascript.Domain;

namespace Nop.Plugin.Misc.Localized.Javascript.Services
{
    /// <summary>
    /// Localization manager interface
    /// </summary>
    public interface IJavascriptResourceService
    {
        /// <summary>
        /// Deletes a locale string resource
        /// </summary>
        /// <param name="localeJsStringResource">Locale string resource</param>
        void DeleteJavaScriptResourceRecord(JavaScriptResourceRecord localeJsStringResource);

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="localeJsStringResourceId">Locale string resource identifier</param>
        /// <returns>Locale string resource</returns>
        JavaScriptResourceRecord GetJavaScriptResourceRecordById(int localeJsStringResourceId);

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="resourceName">A string representing a resource name</param>
        /// <returns>Locale string resource</returns>
        JavaScriptResourceRecord GetJavaScriptResourceRecordByName(string resourceName);

        /// <summary>
        /// Gets a locale string resource
        /// </summary>
        /// <param name="resourceName">A string representing a resource name</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Locale string resource</returns>
        JavaScriptResourceRecord GetJavaScriptResourceRecordByName(string resourceName, int languageId);

        /// <summary>
        /// Gets all locale string resources by language identifier
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Locale string resources</returns>
        Dictionary<string,string> GetAllResources(int languageId);

        /// <summary>
        /// Gets all locale string resources by language identifier
        /// </summary>
        /// <param name="languageId">Language identifier</param>
        /// <returns>Locale string resources</returns>
        Dictionary<string, KeyValuePair<int, string>> GetAllResourceValues(int languageId);

        /// <summary>
        /// Inserts a locale string resource
        /// </summary>
        /// <param name="localeJsStringResource">Locale string resource</param>
        void InsertJavaScriptResourceRecord(JavaScriptResourceRecord localeJsStringResource);

        /// <summary>
        /// Updates the locale string resource
        /// </summary>
        /// <param name="localeJsStringResource">Locale string resource</param>
        void UpdateJavaScriptResourceRecord(JavaScriptResourceRecord localeJsStringResource);

        /// <summary>
        /// Gets a resource string based on the specified ResourceKey property.
        /// </summary>
        /// <param name="resourceKey">A string representing a ResourceKey.</param>
        /// <returns>A string representing the requested resource string.</returns>
        string GetResource(string resourceKey);

        /// <summary>
        /// Gets a resource string based on the specified ResourceKey property.
        /// </summary>
        /// <param name="resourceKey">A string representing a ResourceKey.</param>
        /// <param name="languageId">Language identifier</param>
        /// <returns>A string representing the requested resource string.</returns>
        string GetResource(string resourceKey, int languageId);

    }
}
