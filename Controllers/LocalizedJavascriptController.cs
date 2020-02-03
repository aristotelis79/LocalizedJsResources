using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using Nop.Core.Domain.Localization;
using Nop.Plugin.Misc.Localized.Javascript.Domain;
using Nop.Plugin.Misc.Localized.Javascript.Models;
using Nop.Plugin.Misc.Localized.Javascript.Services;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Extensions;
using Nop.Web.Areas.Admin.Models.Localization;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Extensions;
using Nop.Web.Framework.Kendoui;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.Localized.Javascript.Controllers
{
    public class LocalizedJavascriptController : BasePluginController
    {
        #region fields
        private readonly IJavascriptResourceService _javascriptResourceService;
        private readonly LocalizedJavascriptSettings _localizedJavascriptSettings;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly ILanguageService _languageService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;
        private readonly ICurrencyService _currencyService;

        #endregion

        #region ctor
        public LocalizedJavascriptController(IJavascriptResourceService javascriptResourceService, LocalizedJavascriptSettings localizedJavascriptSettings, ISettingService settingService, IPermissionService permissionService, ILanguageService languageService, IStoreMappingService storeMappingService, IStoreService storeService, ICurrencyService currencyService)
        {
            _javascriptResourceService = javascriptResourceService;
            _localizedJavascriptSettings = localizedJavascriptSettings;
            _settingService = settingService;
            _permissionService = permissionService;
            _languageService = languageService;
            _storeMappingService = storeMappingService;
            _storeService = storeService;
            _currencyService = currencyService;
        }
        #endregion

        #region configuration
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel
            {
                LocalizedJavascriptHtml = _localizedJavascriptSettings.LocalizedJavascriptHtml,
            };
            return View("~/Plugins/Localized.Javascript/Views/Configure.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost]
        public ActionResult Configure(ConfigurationModel model)
        {
            _localizedJavascriptSettings.LocalizedJavascriptHtml = model.LocalizedJavascriptHtml;

            _settingService.SaveSetting(_localizedJavascriptSettings);

            SuccessNotification(_javascriptResourceService.GetResource("Admin.Plugins.Localized.Javascript.Save.Config.Success"));

            return View("~/Plugins/Localized.Javascript/Views/Configure.cshtml", model);
        }
        #endregion

        #region Languages
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual IActionResult List()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            return View("~/Plugins/Localized.Javascript/Views/List.cshtml");
        }

        [HttpPost]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual IActionResult List(DataSourceRequest command)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedKendoGridJson();

            var languages = _languageService.GetAllLanguages(true, loadCacheableCopy: false);
            var gridModel = new DataSourceResult
            {
                Data = languages.Select(x => x.ToModel()),
                Total = languages.Count()
            };

            return Json(gridModel);
        }
        
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual IActionResult Edit(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageLanguages))
                return AccessDeniedView();

            var language = _languageService.GetLanguageById(id, false);
            if (language == null)
                //No language found with the specified id
                return RedirectToAction("List");
            
            var model = language.ToModel();
            //Stores
            PrepareStoresMappingModel(model, language, false);
            //currencies
            PrepareCurrenciesModel(model);

            return View("~/Plugins/Localized.Javascript/Views/Edit.cshtml",model);
        }
        
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        protected virtual void PrepareStoresMappingModel(LanguageModel model, Language language, bool excludeProperties)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (!excludeProperties && language != null)
                model.SelectedStoreIds = _storeMappingService.GetStoresIdsWithAccess(language).ToList();

            var allStores = _storeService.GetAllStores();
            foreach (var store in allStores)
            {
                model.AvailableStores.Add(new SelectListItem
                {
                    Text = store.Name,
                    Value = store.Id.ToString(),
                    Selected = model.SelectedStoreIds.Contains(store.Id)
                });
            }
        }
        
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        protected virtual void PrepareCurrenciesModel(LanguageModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //templates
            model.AvailableCurrencies.Add(new SelectListItem
            {
                Text = "---",
                Value = "0"
            });
            var currencies = _currencyService.GetAllCurrencies(true);
            foreach (var currency in currencies)
            {
                model.AvailableCurrencies.Add(new SelectListItem
                {
                    Text = currency.Name,
                    Value = currency.Id.ToString()
                });
            }
        }

        #endregion

        #region Resources

        [HttpPost]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual IActionResult Resources(int languageId, DataSourceRequest command, LanguageResourcesListModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedKendoGridJson();

            var query = _javascriptResourceService
                .GetAllResourceValues(languageId)
                .OrderBy(x => x.Key)
                .AsQueryable();

            if (!string.IsNullOrEmpty(model.SearchResourceName))
                query = query.Where(l => l.Key.ToLowerInvariant().Contains(model.SearchResourceName.ToLowerInvariant()));
            if (!string.IsNullOrEmpty(model.SearchResourceValue))
                query = query.Where(l => l.Value.Value.ToLowerInvariant().Contains(model.SearchResourceValue.ToLowerInvariant()));

            var resources = query
                .Select(x => new LanguageResourceModel
                {
                    LanguageId = languageId,
                    Id = x.Value.Key,
                    Name = x.Key,
                    Value = x.Value.Value,
                });

            var gridModel = new DataSourceResult
            {
                Data = resources.AsEnumerable().PagedForCommand(command),
                Total = resources.Count()
            };

            return Json(gridModel);
        }

        [HttpPost]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual IActionResult ResourceUpdate(LanguageResourceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();
            if (model.Value != null)
                model.Value = model.Value.Trim();

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var resource = _javascriptResourceService.GetJavaScriptResourceRecordById(model.Id);
            // if the resourceName changed, ensure it isn't being used by another resource
            if (!resource.ResourceName.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase))
            {
                var res = _javascriptResourceService.GetJavaScriptResourceRecordByName(model.Name, model.LanguageId);
                if (res != null && res.Id != resource.Id)
                {
                    return Json(new DataSourceResult { Errors = string.Format(_javascriptResourceService.GetResource("Admin.Configuration.Languages.Resources.NameAlreadyExists"), res.ResourceName) });
                }
            }

            resource.ResourceName = model.Name;
            resource.ResourceValue = model.Value;
            _javascriptResourceService.UpdateJavaScriptResourceRecord(resource);

            return new NullJsonResult();
        }

        [HttpPost]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual IActionResult ResourceAdd(int languageId,LanguageResourceModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();
            if (model.Value != null)
                model.Value = model.Value.Trim();

            if (!ModelState.IsValid)
            {
                return Json(new DataSourceResult { Errors = ModelState.SerializeErrors() });
            }

            var res = _javascriptResourceService.GetJavaScriptResourceRecordByName(model.Name, model.LanguageId);
            if (res == null)
            {
                var resource = new JavaScriptResourceRecord
                {
                    LanguageId = languageId,
                    ResourceName = model.Name,
                    ResourceValue = model.Value
                };
                _javascriptResourceService.InsertJavaScriptResourceRecord(resource);
            }
            else
            {
                return Json(new DataSourceResult { Errors = string.Format(_javascriptResourceService.GetResource("Admin.Configuration.Languages.Resources.NameAlreadyExists"), model.Name) });
            }

            return new NullJsonResult();
        }

        [HttpPost]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual IActionResult ResourceDelete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var resource = _javascriptResourceService.GetJavaScriptResourceRecordById(id);
            if (resource == null)
                throw new ArgumentException("No resource found with the specified id");
            _javascriptResourceService.DeleteJavaScriptResourceRecord(resource);

            return new NullJsonResult();
        }


        public IActionResult GetAllResourceValues(int languageId)
        {
            var allResources = _javascriptResourceService
                .GetAllResources(languageId);

            var allResourcesString = "{";

            foreach (var resource in allResources)
            {
                allResourcesString += $"'{resource.Key}':'{resource.Value}',";
            }

            //remove last ,
            allResourcesString = allResourcesString.Remove(allResourcesString.Length-1);
            //add }
            allResourcesString += "}";

            var json = JObject.Parse(allResourcesString);
            
            return Json(json);
        }
        #endregion

    }
}