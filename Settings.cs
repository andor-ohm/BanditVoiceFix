using System.IO;
using System.Xml;

namespace BanditVoiceFix
{
    internal static class Settings
    {
        internal static bool Load(string path)
        {
            if (!File.Exists(path)) return false;
            var document = new XmlDocument { XmlResolver = null };
            using (var reader = XmlReader.Create(path, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            })) document.Load(reader);
            if (document.DocumentElement == null || document.DocumentElement.Name != "BanditVoiceFix")
                throw new InvalidDataException("Expected a BanditVoiceFix settings root.");
            var node = document.SelectSingleNode("/BanditVoiceFix/EnableDebugLogging");
            if (node == null) return false;
            bool enabled;
            if (!bool.TryParse(node.InnerText.Trim(), out enabled))
                throw new InvalidDataException("EnableDebugLogging must be true or false.");
            return enabled;
        }
    }
}
