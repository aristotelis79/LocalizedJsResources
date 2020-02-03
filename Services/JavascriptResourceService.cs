using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Localization;
using Nop.Plugin.Misc.Localized.Javascript.Domain;
using Nop.Services.Events;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.Localized.Javascript.Services
{
    public class JavascriptResourceService :IJavascriptResourceService
    {
        #region Constants

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// </remarks>
        private const string LOCALJAVASCRIPTSTRINGRESOURCES_ALL_KEY = "Nop.ljsr.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// </remarks>
        private const string LOCALJAVASCRIPTSTRINGRESOURCES_ALL_RESOURCES= "Nop.ljsr.all.resources-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : resource key
        /// </remarks>
        private const string LOCALJAVASCRIPTSTRINGRESOURCES_BY_RESOURCENAME_KEY = "Nop.ljsr.{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        private const string LOCALJAVASCRIPTSTRINGRESOURCES_PATTERN_KEY = "Nop.ljsr.";


        #endregion

        #region Fields

        private readonly IRepository<JavaScriptResourceRecord> _lsrRepository;
        private readonly IWorkContext _workContext;
        private readonly ILogger _logger;
        private readonly IStaticCacheManager _cacheManager;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Ctor

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="cacheManager">Static cache manager</param>
        /// <param name="logger">Logger</param>
        /// <param name="workContext">Work context</param>
        /// <param name="lsrRepository">Locale string resource repository</param>
        /// <param name="localizationSettings">Localization settings</param>
        /// <param name="eventPublisher">Event published</param>
        public JavascriptResourceService(IStaticCacheManager cacheManager,
            ILogger logger,
            IWorkContext workContext,
            IRepository<JavaScriptResourceRecord> lsrRepository,
            LocalizationSettings localizationSettings, 
            IEventPublisher eventPublisher)
        {
            this._cacheManager = cacheManager;
            this._logger = logger;
            this._workContext = workContext;
            this._lsrRepository = lsrRepository;
            this._localizationSettings = localizationSettings;
            this._eventPublisher = eventPublisher;
        }
        #endregion

        public void DeleteJavaScriptResourceRecord(JavaScriptResourceRecord localeJsStringResource)
        {
            if (localeJsStringResource == null)
                throw new ArgumentNullException(nameof(localeJsStringResource));

            _lsrRepository.Delete(localeJsStringResource);

            //cache
            _cacheManager.RemoveByPattern(LOCALJAVASCRIPTSTRINGRESOURCES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityDeleted(localeJsStringResource);
        }

        public JavaScriptResourceRecord GetJavaScriptResourceRecordById(int localeJsStringResourceId)
        {
            if (localeJsStringResourceId == 0)
                return null;

            return _lsrRepository.GetById(localeJsStringResourceId);

        }

        public JavaScriptResourceRecord GetJavaScriptResourceRecordByName(string resourceName)
        {
            if (_workContext.WorkingLanguage != null)
                return GetJavaScriptResourceRecordByName(resourceName, _workContext.WorkingLanguage.Id);

            return null;
        }

        public JavaScriptResourceRecord GetJavaScriptResourceRecordByName(string resourceName, int languageId)
        {
            var query = from lsr in _lsrRepository.Table
                orderby lsr.ResourceName
                where lsr.LanguageId == languageId && lsr.ResourceName == resourceName
                select lsr;
            var localeStringResource = query.FirstOrDefault();

            if (localeStringResource == null)
                _logger.Warning($"Resource string ({resourceName}) not found. Language ID = {languageId}");
            return localeStringResource;
        }

        public Dictionary<string,string> GetAllResources(int languageId)
        {
            var key = string.Format(LOCALJAVASCRIPTSTRINGRESOURCES_ALL_RESOURCES, languageId);

            var allResources = _cacheManager.Get(key, () =>
            {
                return _lsrRepository.TableNoTracking
                                        .Where(x => x.LanguageId == languageId)
                                        .ToDictionary(x => x.ResourceName, x => x.ResourceValue);
            });

            //remove separated resource 
            //_cacheManager.Remove(string.Format(LOCALJAVASCRIPTSTRINGRESOURCES_ALL_RESOURCES, languageId));

            return allResources;
        }

        public Dictionary<string, KeyValuePair<int, string>> GetAllResourceValues(int languageId)
        {
            var key = string.Format(LOCALJAVASCRIPTSTRINGRESOURCES_ALL_KEY, languageId);

            var rez = _cacheManager.Get(key, () =>
            {
                //we use no tracking here for performance optimization
                //anyway records are loaded only for read-only operations
                var query = from l in _lsrRepository.TableNoTracking
                    orderby l.ResourceName
                    where l.LanguageId == languageId
                    select l;

                return ResourceValuesToDictionary(query);
            });

            return rez;
        }

        private static Dictionary<string, KeyValuePair<int, string>> ResourceValuesToDictionary(IEnumerable<JavaScriptResourceRecord> locales)
        {
            //format: <name, <id, value>>
            var dictionary = new Dictionary<string, KeyValuePair<int, string>>();
            foreach (var locale in locales)
            {
                var resourceName = locale.ResourceName.ToLowerInvariant();
                if (!dictionary.ContainsKey(resourceName))
                    dictionary.Add(resourceName, new KeyValuePair<int, string>(locale.Id, locale.ResourceValue));
            }
            return dictionary;
        }

        public void InsertJavaScriptResourceRecord(JavaScriptResourceRecord localeJsStringResource)
        {
            if (localeJsStringResource == null)
                throw new ArgumentNullException(nameof(localeJsStringResource));
            
            _lsrRepository.Insert(localeJsStringResource);

            //cache
            _cacheManager.RemoveByPattern(LOCALJAVASCRIPTSTRINGRESOURCES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityInserted(localeJsStringResource);
        }

        public void UpdateJavaScriptResourceRecord(JavaScriptResourceRecord localeJsStringResource)
        {
            if (localeJsStringResource == null)
                throw new ArgumentNullException(nameof(localeJsStringResource));

            _lsrRepository.Update(localeJsStringResource);

            //cache
            _cacheManager.RemoveByPattern(LOCALJAVASCRIPTSTRINGRESOURCES_PATTERN_KEY);

            //event notification
            _eventPublisher.EntityUpdated(localeJsStringResource);

        }

        public string GetResource(string resourceKey)
        {
            if (_workContext.WorkingLanguage != null)
                return GetResource(resourceKey, _workContext.WorkingLanguage.Id);
            
            return "";
        }

        public string GetResource(string resourceKey, int languageId)
        {
            var result = string.Empty;
            if (resourceKey == null)
                resourceKey = string.Empty;
            resourceKey = resourceKey.Trim().ToLowerInvariant();
            if (_localizationSettings.LoadAllLocaleRecordsOnStartup)
            {
                //load all records (we know they are cached)
                var resources = GetAllResourceValues(languageId);
                if (resources.ContainsKey(resourceKey))
                {
                    result = resources[resourceKey].Value;
                }
            }
            else
            {
                //gradual loading
                var key = string.Format(LOCALJAVASCRIPTSTRINGRESOURCES_BY_RESOURCENAME_KEY, languageId, resourceKey);
                var lsr = _cacheManager.Get(key, () =>
                {
                    var query = from l in _lsrRepository.Table
                        where l.ResourceName == resourceKey
                              && l.LanguageId == languageId
                        select l.ResourceValue;
                    return query.FirstOrDefault();
                });

                if (lsr != null) 
                    result = lsr;
            }
            return result ?? string.Empty;
        }
    }
}