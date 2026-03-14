using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace BanditVoiceFix
{
    public class SubModule : MBSubModuleBase
    {
        internal static string LogPath;
        internal static Harmony HarmonyInstance;
        internal static int ConversationSequence;
        internal static int ActiveConversationId;
        internal static string ActiveConversationLabel;
        internal static bool LogSessionInitialized;

        internal static bool EnableVerboseLogging
        {
            get
            {
                try
                {
                    return BanditVoiceFixSettings.Instance != null && BanditVoiceFixSettings.Instance.EnableDebugLogging;
                }
                catch
                {
                    return false;
                }
            }
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

            try
            {
                LogPath = null;
                ConversationSequence = 0;
                ActiveConversationId = 0;
                ActiveConversationLabel = null;
                LogSessionInitialized = false;

                HarmonyInstance = new Harmony("BanditVoiceFix");
                HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

                Log("Harmony patches applied.", true);
            }
            catch (Exception ex)
            {
                try
                {
                    File.AppendAllText(
                        "BanditVoiceFix_fallback.log",
                        "\r\n[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] OnSubModuleLoad EXCEPTION\r\n" + ex + "\r\n"
                    );
                }
                catch
                {
                }
            }
        }

        protected override void OnSubModuleUnloaded()
        {
            try
            {
                EndConversationLog("OnSubModuleUnloaded");
                Log("OnSubModuleUnloaded", true);
                if (HarmonyInstance != null)
                {
                    HarmonyInstance.UnpatchAll("BanditVoiceFix");
                }
            }
            catch
            {
            }

            base.OnSubModuleUnloaded();
        }

        internal static void Log(string message, bool force = false)
        {
            if (!EnableVerboseLogging)
            {
                return;
            }

            try
            {
                if (!EnsureLogReady())
                {
                    return;
                }

                File.AppendAllText(LogPath, "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] " + message + "\r\n");
            }
            catch
            {
            }
        }

        internal static bool EnsureLogReady()
        {
            if (LogSessionInitialized && !string.IsNullOrEmpty(LogPath))
            {
                return true;
            }

            try
            {
                string dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string moduleRoot = Directory.GetParent(Directory.GetParent(dllDir).FullName).FullName;
                string logDir = Path.Combine(moduleRoot, "Logs");
                string currentLogPath = Path.Combine(logDir, "BanditVoiceFix.log");
                string previousLogPath = Path.Combine(logDir, "BanditVoiceFix.previous.log");

                Directory.CreateDirectory(logDir);

                if (File.Exists(currentLogPath))
                {
                    try
                    {
                        if (File.Exists(previousLogPath))
                        {
                            File.Delete(previousLogPath);
                        }

                        File.Move(currentLogPath, previousLogPath);
                    }
                    catch
                    {
                    }
                }

                LogPath = currentLogPath;

                File.WriteAllText(
                    LogPath,
                    "========== BanditVoiceFix startup " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ==========\r\n"
                );

                if (File.Exists(previousLogPath))
                {
                    File.AppendAllText(
                        LogPath,
                        "[INFO] Previous session log preserved as BanditVoiceFix.previous.log\r\n"
                    );
                }

                LogSessionInitialized = true;
                return true;
            }
            catch
            {
                LogPath = null;
                return false;
            }
        }

        internal static string Safe(Func<string> fn)
        {
            try
            {
                string result = fn();
                return result ?? "<null>";
            }
            catch (Exception ex)
            {
                return "<EX:" + ex.GetType().Name + ":" + ex.Message + ">";
            }
        }

        internal static string GetObjectId(object obj)
        {
            if (obj == null)
            {
                return "<null>";
            }

            try
            {
                Type t = obj.GetType();

                PropertyInfo p = t.GetProperty("StringId", BindingFlags.Public | BindingFlags.Instance);
                if (p != null)
                {
                    object value = p.GetValue(obj, null);
                    if (value != null)
                    {
                        return value.ToString();
                    }
                }

                p = t.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                if (p != null)
                {
                    object value = p.GetValue(obj, null);
                    if (value != null)
                    {
                        return value.ToString();
                    }
                }
            }
            catch
            {
            }

            return "<unknown>";
        }

        internal static string DescribeCharacter(CharacterObject character)
        {
            if (character == null)
            {
                return "<null>";
            }

            string persona = Safe(() =>
            {
                var p = character.GetPersona();
                return p != null ? p.Name.ToString() : "<null>";
            });

            string personaId = Safe(() =>
            {
                var p = character.GetPersona();
                return p != null ? GetObjectId(p) : "<null>";
            });

            string culture = Safe(() => character.Culture != null ? GetObjectId(character.Culture) : "<null>");
            string hero = Safe(() => character.HeroObject != null ? GetObjectId(character.HeroObject) : "<null>");
            string occupation = Safe(() => character.Occupation.ToString());
            string name = Safe(() => character.Name != null ? character.Name.ToString() : "<null>");
            string isHero = Safe(() => character.IsHero.ToString());
            string isFemale = Safe(() => character.IsFemale.ToString());
            string id = Safe(() => GetObjectId(character));

            return "Name='" + name +
                   "' Id='" + id +
                   "' Culture='" + culture +
                   "' Persona='" + persona +
                   "' PersonaId='" + personaId +
                   "' Hero='" + hero +
                   "' IsHero='" + isHero +
                   "' IsFemale='" + isFemale +
                   "' Occupation='" + occupation + "'";
        }

        internal static string DescribeAgent(IAgent agent)
        {
            if (agent == null)
            {
                return "<null>";
            }

            CharacterObject character = null;

            try
            {
                character = agent.Character as CharacterObject;
            }
            catch
            {
            }

            return "AgentActive=" + Safe(() => agent.IsActive().ToString()) +
                   " Character=[" + DescribeCharacter(character) + "]";
        }

        internal static string GuessConversationContext()
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                sb.Append("CampaignMission.Current=" + (CampaignMission.Current != null) + "; ");
            }
            catch
            {
            }

            try
            {
                sb.Append("Settlement.Current=" + (Settlement.CurrentSettlement != null ? GetObjectId(Settlement.CurrentSettlement) : "<null>") + "; ");
            }
            catch
            {
            }

            try
            {
                sb.Append("CurrentMapEvent=" + (MapEvent.PlayerMapEvent != null ? MapEvent.PlayerMapEvent.EventType.ToString() : "<null>") + "; ");
            }
            catch
            {
            }

            try
            {
                sb.Append("ConversationContext=" + (Campaign.Current != null ? Campaign.Current.CurrentConversationContext.ToString() : "<null>") + "; ");
            }
            catch
            {
            }

            try
            {
                ConversationManager cm = Campaign.Current != null ? Campaign.Current.ConversationManager : null;
                MobileParty party = cm != null ? cm.ConversationParty : null;

                sb.Append("ConversationParty=" + (party != null ? GetObjectId(party) : "<null>") + "; ");
                sb.Append("ConversationPartyBandit=" + (party != null && party.IsBandit) + "; ");
            }
            catch
            {
            }

            return sb.ToString();
        }

        internal static bool IsBanditCharacter(CharacterObject c)
        {
            if (c == null)
            {
                return false;
            }

            try
            {
                string id = GetObjectId(c).ToLowerInvariant();
                string occ = c.Occupation.ToString().ToLowerInvariant();
                string culture = c.Culture != null ? GetObjectId(c.Culture).ToLowerInvariant() : "";

                if (id.Contains("bandit") || occ.Contains("bandit"))
                {
                    return true;
                }

                if (culture.Contains("bandit") || culture == "looters" || culture == "sea_raiders")
                {
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        internal static bool ShouldLogConversation(ConversationManager manager)
        {
            try
            {
                MobileParty party = manager != null ? manager.ConversationParty : null;
                if (party != null && party.IsBandit)
                {
                    return true;
                }
            }
            catch
            {
            }

            try
            {
                CharacterObject c = manager != null ? manager.OneToOneConversationCharacter : null;
                if (IsBanditCharacter(c))
                {
                    return true;
                }
            }
            catch
            {
            }

            try
            {
                CharacterObject speaker = manager != null && manager.SpeakerAgent != null
                    ? manager.SpeakerAgent.Character as CharacterObject
                    : null;

                if (IsBanditCharacter(speaker))
                {
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        internal static bool ShouldLogVoiceModel(CharacterObject character)
        {
            if (IsBanditCharacter(character))
            {
                return true;
            }

            try
            {
                if (MobileParty.ConversationParty != null && MobileParty.ConversationParty.IsBandit)
                {
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        internal static string DumpVoicePaths(VoiceObject voiceObject)
        {
            if (voiceObject == null)
            {
                return "<null voiceObject>";
            }

            try
            {
                if (voiceObject.VoicePaths == null)
                {
                    return "<null VoicePaths>";
                }

                StringBuilder sb = new StringBuilder();
                int i = 0;
                foreach (string path in voiceObject.VoicePaths)
                {
                    sb.Append("[").Append(i).Append("] ").Append(path ?? "<null>").Append(" || ");
                    i++;
                }

                if (i == 0)
                {
                    return "<empty VoicePaths>";
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                return "<EX:" + ex.GetType().Name + ":" + ex.Message + ">";
            }
        }

        internal static string NormalizeVoicePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "";
            }

            string result = path.Replace("$PLATFORM", "PC");
            if (!result.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
            {
                result += ".ogg";
            }

            return result;
        }

        internal static string GetAccentClass(CharacterObject character)
        {
            if (character == null || Campaign.Current == null || Campaign.Current.Models == null || Campaign.Current.Models.VoiceOverModel == null)
            {
                return "<unknown>";
            }

            try
            {
                return Campaign.Current.Models.VoiceOverModel.GetAccentClass(
                    character.Culture,
                    TaleWorlds.CampaignSystem.Conversation.Tags.ConversationTagHelper.UsesHighRegister(character)
                ) ?? "<null>";
            }
            catch (Exception ex)
            {
                return "<EX:" + ex.GetType().Name + ":" + ex.Message + ">";
            }
        }

        internal static string GetCurrentConversationText()
        {
            try
            {
                ConversationManager manager = Campaign.Current != null ? Campaign.Current.ConversationManager : null;
                if (manager == null)
                {
                    return "<no_conversation_manager>";
                }

                string text = manager.CurrentSentenceText;
                return string.IsNullOrEmpty(text) ? "<null_or_empty>" : text;
            }
            catch (Exception ex)
            {
                return "<EX:" + ex.GetType().Name + ":" + ex.Message + ">";
            }
        }

        internal static string DescribeCharacterShort(CharacterObject character)
        {
            if (character == null)
            {
                return "<null>";
            }

            return Safe(() => character.Name != null ? character.Name.ToString() : "<null_name>") +
                   " (" + Safe(() => GetObjectId(character)) + ")";
        }

        internal static string GetVoiceTokenSummary(VoiceObject voiceObject)
        {
            if (voiceObject == null || voiceObject.VoicePaths == null)
            {
                return "<none>";
            }

            List<string> tokens = new List<string>();

            foreach (string path in voiceObject.VoicePaths)
            {
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                string normalized = NormalizeVoicePath(path).Replace('\\', '/');
                string fileName = Path.GetFileNameWithoutExtension(normalized);
                if (string.IsNullOrEmpty(fileName))
                {
                    continue;
                }

                string[] parts = fileName.Split('_');
                if (parts.Length < 3)
                {
                    continue;
                }

                string token = string.Join("_", parts, 0, parts.Length - 2) + "_";
                if (!tokens.Contains(token))
                {
                    tokens.Add(token);
                }
            }

            return tokens.Count == 0 ? "<none>" : string.Join(", ", tokens.ToArray());
        }

        internal static void StartConversationLog(string source, string label, params string[] details)
        {
            if (ActiveConversationId != 0)
            {
                if (string.Equals(ActiveConversationLabel, label, StringComparison.Ordinal))
                {
                    return;
                }

                EndConversationLog("ImplicitNewConversation");
            }

            ActiveConversationId = ++ConversationSequence;
            ActiveConversationLabel = label ?? "<unknown>";
            Log("===== Conversation #" + ActiveConversationId + " BEGIN (" + source + ") [" + ActiveConversationLabel + "] =====");

            if (details == null)
            {
                return;
            }

            foreach (string detail in details)
            {
                if (!string.IsNullOrEmpty(detail))
                {
                    Log(detail);
                }
            }
        }

        internal static void EndConversationLog(string source)
        {
            if (ActiveConversationId == 0)
            {
                return;
            }

            Log("===== Conversation #" + ActiveConversationId + " END (" + source + ") [" + (ActiveConversationLabel ?? "<unknown>") + "] =====");
            ActiveConversationId = 0;
            ActiveConversationLabel = null;
        }

        internal static string SelectBanditFallbackPath(CharacterObject character, VoiceObject voiceObject)
        {
            if (character == null || voiceObject == null || voiceObject.VoicePaths == null || Campaign.Current == null)
            {
                return "";
            }

            string accentClass = "";
            try
            {
                accentClass = Campaign.Current.Models.VoiceOverModel.GetAccentClass(
                    character.Culture,
                    TaleWorlds.CampaignSystem.Conversation.Tags.ConversationTagHelper.UsesHighRegister(character)
                );
            }
            catch
            {
                return "";
            }

            List<string> tokens = new List<string>();

            if (!string.IsNullOrEmpty(accentClass))
            {
                tokens.Add(accentClass + "_");
            }

            if (accentClass == "mountain_bandits" || accentClass == "forest_bandits")
            {
                tokens.Add("looters_");
            }

            List<string> matches = new List<string>();

            foreach (string token in tokens)
            {
                foreach (string path in voiceObject.VoicePaths)
                {
                    if (!string.IsNullOrEmpty(path) &&
                        path.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0 &&
                        !matches.Contains(path))
                    {
                        matches.Add(path);
                    }
                }

                if (matches.Count > 0)
                {
                    break;
                }
            }

            if (matches.Count == 0 && MobileParty.ConversationParty != null && MobileParty.ConversationParty.IsBandit)
            {
                string[] genericBanditTokens =
                {
                    "looters_",
                    "sea_raiders_",
                    "desert_bandits_",
                    "steppe_bandits_",
                    "forest_bandits_",
                    "mountain_bandits_"
                };

                foreach (string token in genericBanditTokens)
                {
                    foreach (string path in voiceObject.VoicePaths)
                    {
                        if (!string.IsNullOrEmpty(path) &&
                            path.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0 &&
                            !matches.Contains(path))
                        {
                            matches.Add(path);
                        }
                    }

                    if (matches.Count > 0)
                    {
                        break;
                    }
                }
            }

            if (matches.Count == 0)
            {
                return "";
            }

            string selected;
            if (character.IsHero && character.HeroObject != null)
            {
                selected = matches[character.HeroObject.RandomInt(matches.Count)];
            }
            else if (MobileParty.ConversationParty != null)
            {
                selected = matches[MobileParty.ConversationParty.RandomInt(matches.Count)];
            }
            else
            {
                selected = matches[0];
            }

            return NormalizeVoicePath(selected);
        }
    }

    [HarmonyPatch(typeof(ConversationManager), "SetupAndStartMapConversation")]
    public static class Patch_SetupAndStartMapConversation
    {
        public static void Prefix(MobileParty party, IAgent agent, IAgent mainAgent)
        {
            if (!SubModule.EnableVerboseLogging)
            {
                return;
            }

            try
            {
                bool isBandit = party != null && party.IsBandit;
                CharacterObject targetCharacter = agent != null ? agent.Character as CharacterObject : null;

                bool shouldLog = isBandit || SubModule.IsBanditCharacter(targetCharacter);
                if (!shouldLog)
                {
                    return;
                }

                string partyId = party != null ? SubModule.GetObjectId(party) : "<null_party>";
                SubModule.StartConversationLog(
                    "SetupAndStartMapConversation",
                    partyId
                );
            }
            catch (Exception ex)
            {
                SubModule.Log("SetupAndStartMapConversation Prefix EXCEPTION: " + ex, true);
            }
        }
    }

    [HarmonyPatch(typeof(ConversationManager), "BeginConversation")]
    public static class Patch_BeginConversation
    {
        public static void Postfix(ConversationManager __instance)
        {
            if (!SubModule.EnableVerboseLogging)
            {
                return;
            }

            try
            {
                if (!SubModule.ShouldLogConversation(__instance))
                {
                    return;
                }

                SubModule.StartConversationLog(
                    "BeginConversation",
                    SubModule.Safe(() => __instance.ConversationParty != null ? SubModule.GetObjectId(__instance.ConversationParty) : SubModule.GetObjectId(__instance.OneToOneConversationCharacter))
                );
            }
            catch (Exception ex)
            {
                SubModule.Log("BeginConversation Postfix EXCEPTION: " + ex, true);
            }
        }
    }

    [HarmonyPatch(typeof(ConversationManager), "EndConversation")]
    public static class Patch_EndConversation
    {
        public static void Prefix()
        {
            try
            {
                SubModule.EndConversationLog("EndConversation");
            }
            catch (Exception ex)
            {
                SubModule.Log("EndConversation Prefix EXCEPTION: " + ex, true);
            }
        }
    }

    [HarmonyPatch(typeof(ConversationManager), "OnConversationDeactivate")]
    public static class Patch_OnConversationDeactivate
    {
        public static void Prefix()
        {
            try
            {
                SubModule.EndConversationLog("OnConversationDeactivate");
            }
            catch (Exception ex)
            {
                SubModule.Log("OnConversationDeactivate Prefix EXCEPTION: " + ex, true);
            }
        }
    }

    [HarmonyPatch(typeof(DefaultVoiceOverModel), "GetSoundPathForCharacter")]
    public static class Patch_DefaultVoiceOverModel_GetSoundPathForCharacter
    {
        public static void Postfix(CharacterObject character, VoiceObject voiceObject, ref string __result)
        {
            try
            {
                if (!SubModule.ShouldLogVoiceModel(character))
                {
                    return;
                }

                if (!string.IsNullOrEmpty(__result))
                {
                    return;
                }

                if (voiceObject == null || voiceObject.VoicePaths == null)
                {
                    if (character != null && !character.IsHero)
                    {
                        SubModule.Log(
                            "Bandit voice line is unvoiced | Party=" + (SubModule.ActiveConversationLabel ?? "<unknown>") +
                            " | Speaker=" + SubModule.DescribeCharacterShort(character) +
                            " | Accent=" + SubModule.GetAccentClass(character) +
                            " | Line='" + SubModule.GetCurrentConversationText() + "'" +
                            " | VoiceTokens=<none>",
                            true
                        );
                    }
                    return;
                }

                string fallback = SubModule.SelectBanditFallbackPath(character, voiceObject);
                if (!string.IsNullOrEmpty(fallback))
                {
                    __result = fallback;
                    SubModule.Log(
                        "Bandit voice fallback selected | Party=" + (SubModule.ActiveConversationLabel ?? "<unknown>") +
                        " | Speaker=" + SubModule.DescribeCharacterShort(character) +
                        " | Accent=" + SubModule.GetAccentClass(character) +
                        " | Line='" + SubModule.GetCurrentConversationText() + "'" +
                        " | SoundPath='" + __result + "'",
                        true
                    );
                }
                else
                {
                    SubModule.Log(
                        "Bandit voice fallback found no matching path | Party=" + (SubModule.ActiveConversationLabel ?? "<unknown>") +
                        " | Speaker=" + SubModule.DescribeCharacterShort(character) +
                        " | Accent=" + SubModule.GetAccentClass(character) +
                        " | Line='" + SubModule.GetCurrentConversationText() + "'" +
                        " | VoiceTokens=" + SubModule.GetVoiceTokenSummary(voiceObject),
                        true
                    );
                }
            }
            catch (Exception ex)
            {
                SubModule.Log("DefaultVoiceOverModel.GetSoundPathForCharacter Postfix EXCEPTION: " + ex, true);
            }
        }
    }
}
