using Nop.Data.Mapping;
using Nop.Plugin.Misc.Localized.Javascript.Domain;

namespace Nop.Plugin.Misc.Localized.Javascript.Data
{
    public class JavaScriptResourceRecordMap : NopEntityTypeConfiguration<JavaScriptResourceRecord>
    {
        public JavaScriptResourceRecordMap()
        {
            this.ToTable("JavaScriptResources");
            this.HasKey(x => x.Id);
        }
    }
}