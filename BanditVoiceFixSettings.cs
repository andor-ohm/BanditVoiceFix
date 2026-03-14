using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Base.Global;
using MCM.Common;

namespace BanditVoiceFix
{
    public class BanditVoiceFixSettings : AttributeGlobalSettings<BanditVoiceFixSettings>
    {
        public override string Id => "BanditVoiceFix_v1";
        public override string DisplayName => "Bandit Voice Fix";
        public override string FolderName => "BanditVoiceFix";
        public override string FormatType => "json";

        [SettingPropertyBool("Enable Debug Logging", HintText = "Writes detailed diagnostic conversation logs to Modules\\BanditVoiceFix\\Logs. Default is off.", RequireRestart = false, Order = 0)]
        [SettingPropertyGroup("General")]
        public bool EnableDebugLogging { get; set; } = false;
    }
}