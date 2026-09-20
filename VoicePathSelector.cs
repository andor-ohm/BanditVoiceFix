using System;
using System.Collections.Generic;

namespace BanditVoiceFix
{
    internal static class VoicePathSelector
    {
        // Compatibility preference, not a claim about historical vanilla behavior.
        private static readonly string[] FallbackAccents =
            { "looters", "sea_raiders", "mountain_bandits", "forest_bandits", "desert_bandits", "steppe_bandits" };

        internal static List<string> GetCandidates(IEnumerable<string> paths, string culture,
            bool isBandit, bool isDeserter, bool isFemale, bool isHero)
        {
            var result = new List<string>();
            // Legacy untagged recordings are the original male bandit voices.
            // Tagged/persona-specific formats remain the vanilla model's responsibility.
            if (paths == null || isFemale || isHero || (!isBandit && !isDeserter))
                return result;

            string accent = isBandit && IsBanditCulture(culture) ? culture : null;
            if (accent == null && !isDeserter)
                return result;

            var materialized = new List<string>(paths);
            if (accent != null)
                AddMatches(materialized, accent, result);
            // Only borrow from this dialogue line's own VoiceObject, never another sentence.
            // Looter-first preserves existing forest/mountain/deserter substitutions.
            // Stop at one accent so XML ordering cannot change the preferred voice group.
            foreach (string fallbackAccent in FallbackAccents)
            {
                if (result.Count != 0) break;
                AddMatches(materialized, fallbackAccent, result);
            }
            return result;
        }

        private static bool IsBanditCulture(string culture)
        {
            return culture == "looters" || culture == "sea_raiders" || culture == "desert_bandits" ||
                culture == "steppe_bandits" || culture == "forest_bandits" || culture == "mountain_bandits";
        }

        private static void AddMatches(IEnumerable<string> paths, string accent, List<string> result)
        {
            foreach (string path in paths)
            {
                if (string.IsNullOrEmpty(path)) continue;
                string normalized = path.Replace('\\', '/');
                string name = normalized.Substring(normalized.LastIndexOf('/') + 1);
                if (name.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
                    name = name.Substring(0, name.Length - 4);
                string prefix = accent + "_";
                if (!name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) continue;
                string suffix = name.Substring(prefix.Length);
                // The installed format is <accent>_<two-digit actor>_<line id>.
                if (suffix.Length < 4 || suffix[0] < '0' || suffix[0] > '9' ||
                    suffix[1] < '0' || suffix[1] > '9' || suffix[2] != '_') continue;
                bool valid = true;
                for (int i = 3; i < suffix.Length; i++)
                {
                    char c = suffix[i];
                    if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9')))
                    { valid = false; break; }
                }
                if (!valid) continue;
                string output = Normalize(path).Replace('\\', '/');
                if (!result.Exists(existing => string.Equals(Normalize(existing).Replace('\\', '/'), output,
                    StringComparison.OrdinalIgnoreCase))) result.Add(path);
            }
        }

        internal static string Normalize(string path)
        {
            string result = path.Replace("$PLATFORM", "PC");
            return result.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase) ? result : result + ".ogg";
        }
    }
}
