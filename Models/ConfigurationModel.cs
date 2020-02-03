using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Mvc.Models;

namespace Nop.Plugin.Misc.Localized.Javascript.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Admin.Plugins.Misc.Localized.Javascript.Html.Code")]
        //[AllowHtml] 
        public string LocalizedJavascriptHtml { get; set; }

    }
}