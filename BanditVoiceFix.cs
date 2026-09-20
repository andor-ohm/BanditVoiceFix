using HarmonyLib;
using System;
using System.IO;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace BanditVoiceFix
{
    public class SubModule : MBSubModuleBase
    {
        private const string HarmonyId = "BanditVoiceFix";
        private static readonly FileLogger Logger = new FileLogger(() => Path.Combine(ModuleDirectory, "Logs"));
        private Harmony harmony;
        internal static bool DebugLogging;
        private static string ModuleDirectory => Directory.GetParent(Path.GetDirectoryName(typeof(SubModule).Assembly.Location)).Parent.FullName;

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            DebugLogging = false;
            try
            {
                DebugLogging = Settings.Load(Path.Combine(ModuleDirectory, "BanditVoiceFix.settings.xml"));
            }
            catch (Exception ex)
            {
                LogError("Reading settings (using error-only logging)", ex);
            }

            try
            {
                harmony = new Harmony(HarmonyId);
                harmony.PatchAll(typeof(SubModule).Assembly);
                LogDebug("Voice patch applied; version " + typeof(SubModule).Assembly.GetName().Version);
            }
            catch (Exception ex)
            {
                LogError("Applying voice patch", ex);
            }
        }

        protected override void OnSubModuleUnloaded()
        {
            try { harmony?.UnpatchAll(HarmonyId); }
            catch (Exception ex) { LogError("Removing voice patch", ex); }
            base.OnSubModuleUnloaded();
        }

        internal static void LogError(string context, Exception exception)
        {
            Logger.Write(context + " | " + exception, true, false);
        }

        internal static void LogDebug(string message)
        {
            Logger.Write(message, false, DebugLogging);
        }
    }

    [HarmonyPatch(typeof(DefaultVoiceOverModel), nameof(DefaultVoiceOverModel.GetSoundPathForCharacter),
        new Type[] { typeof(CharacterObject), typeof(VoiceObject) })]
    public static class VoicePathPatch
    {
        public static void Postfix(CharacterObject character, VoiceObject voiceObject, ref string __result)
        {
            try
            {
                // Avoid even querying campaign state for successful vanilla results or quest heroes.
                if (!string.IsNullOrEmpty(__result) || character == null || character.IsHero || character.IsFemale ||
                    voiceObject == null || voiceObject.VoicePaths == null || voiceObject.VoicePaths.Count == 0)
                    return;
                string culture = character.Culture?.StringId;
                bool bandit = character.Occupation == Occupation.Bandit;
                MobileParty party = MobileParty.ConversationParty;
                bool deserter = party != null && party.IsBandit && party.ActualClan?.StringId == "deserters";
                var candidates = VoicePathSelector.GetCandidates(voiceObject.VoicePaths, culture, bandit, deserter,
                    character.IsFemale, character.IsHero);
                if (candidates.Count == 0)
                {
                    if (SubModule.DebugLogging && (bandit || deserter))
                        SubModule.LogDebug("No supported legacy voice | Speaker=" + character.StringId + " | Culture=" + culture +
                            " | Deserter=" + deserter + " | AvailablePaths=" + string.Join("; ", voiceObject.VoicePaths));
                    return;
                }

                int index = party == null ? 0 : party.RandomInt(candidates.Count);
                __result = VoicePathSelector.Normalize(candidates[index]);
                if (SubModule.DebugLogging)
                    SubModule.LogDebug("Legacy voice selected | Speaker=" + character.StringId +
                        " | Culture=" + culture + " | Deserter=" + deserter + " | Path=" + __result +
                        " | AvailablePaths=" + string.Join("; ", voiceObject.VoicePaths));
            }
            catch (Exception ex)
            {
                SubModule.LogError("Selecting legacy bandit voice", ex);
            }
        }
    }
}
