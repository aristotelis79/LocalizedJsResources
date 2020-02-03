using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.Localized.Javascript.Components
{
    [ViewComponent(Name = "LocalizedJavascriptHtml")]
    public class LocalizedJavascriptHtmlViewComponent : NopViewComponent
    {
        private readonly LocalizedJavascriptSettings _localizedJavascriptSettings;

        public LocalizedJavascriptHtmlViewComponent(LocalizedJavascriptSettings localizedJavascriptSettings)
        {
            _localizedJavascriptSettings = localizedJavascriptSettings;
        }

    
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            return View("~/Plugins/Localized.Javascript/Views/PublicInfo.cshtml", _localizedJavascriptSettings.LocalizedJavascriptHtml);
        }
    }
}
