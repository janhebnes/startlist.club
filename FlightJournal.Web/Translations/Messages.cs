using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Ajax.Utilities;
using RestSharp.Deserializers;

namespace FlightJournal.Web.Translations
{
    /// <summary>
    /// Handles message translation storing and fetching
    /// Inspired by Babel from https://github.com/skylines-project/skylines/blob/master/skylines/frontend/translations/messages.pot 
    /// Considered https://github.com/fsateler/gettext-cs-utils but have done this instead as a first implementation
    /// </summary>
    public sealed class Messages
    {
        // Singleton 
        private static volatile Messages instance;
        private static object syncRoot = new Object();
        
        // Internal storage
        private Dictionary<string, TranslationFile> _translationStore = null;
        private string _translationDirectoryFullPath = null;

        // Const
        private const string TranslationPath = "Translations"; 
        private const string TranslationFileName = "messages.xml";

        private Messages(){}

        public static Messages Instance
        {
            get 
            {
                if (instance == null) 
                {
                lock (syncRoot) 
                {
                    if (instance == null)
                        instance = new Messages();
                }
                }

                return instance;
            }
        }

        /// <summary>
        /// Based on accessible translation files
        /// </summary>
        public string[] SupportedLanguageIsoCodes
        {
            get
            {
                return TranslationStore.Keys.ToArray();
            }
        }

        public string GetText(string messageId)
        {
            return GetText(messageId, UserLanguages.DefaultLanguage());
        }

        public string GetText(string messageId, string languageIsoCode)
        {
            var translation = TranslationStore[languageIsoCode].Messages.FirstOrDefault(f => f.MsgId == messageId);
            if (translation != null)
            {
                return translation.MsgStr.IsNullOrWhiteSpace() ? messageId : translation.MsgStr;
            }
            else
            {
                // Generate ressource messageId id througout the store, when a missing file entry is detected
                // This approach might be to primitive, bable is much prettier, but this is less code to the project...
                foreach (var store in TranslationStore)
                {
                    var messages = TranslationStore[store.Key].Messages.ToList();
                    if (messages.Exists(t => t.MsgId == messageId))
                    {
                        continue;
                    }

                    messages.Add(new Translation() { MsgId = messageId, MsgStr = string.Empty });
                    TranslationStore[store.Key].Messages = messages.OrderBy(f=>f.MsgId).ToArray();
                    var langFullPath = Path.Combine(TranslationDirectoryFullPath, store.Key, TranslationFileName);
                    var serializer = new XmlSerializer(typeof(Messages.TranslationFile));
                    using (StreamWriter writer = new StreamWriter(langFullPath))
                    {
                        serializer.Serialize(writer, TranslationStore[store.Key]);
                    }
                }
                
            }
            return messageId;
        }

        public Dictionary<string, TranslationFile> TranslationStore
        {
            get
            {
                if (_translationStore == null)
                {
                    LoadMessages();
                }
                return _translationStore;
            }
        }

        public string TranslationDirectoryFullPath
        {
            get
            {
                if (_translationDirectoryFullPath != null || HttpContext.Current == null)
                {
                    return _translationDirectoryFullPath;
                }

                string appPath = HttpContext.Current.Request.ApplicationPath ?? "/";
                _translationDirectoryFullPath = HttpContext.Current.Request.MapPath(Path.Combine(appPath, TranslationPath));
                return _translationDirectoryFullPath;
            }
            set { _translationDirectoryFullPath = value; }
        }

        private void LoadMessages()
        {
            _translationStore = new Dictionary<string, TranslationFile>();
            if (!Directory.Exists(TranslationDirectoryFullPath))
            {
                return;
            }

            foreach (var translationLanguageDirectory in Directory.GetDirectories(TranslationDirectoryFullPath))
            {
                var langFullPath = Path.Combine(translationLanguageDirectory, TranslationFileName);
                var langName = translationLanguageDirectory.Replace(TranslationDirectoryFullPath + "\\", string.Empty);
                if (File.Exists(langFullPath))
                {
                    var serializer = new XmlSerializer(typeof(TranslationFile));
                    TranslationFile file;
                    using (XmlReader reader = XmlReader.Create(langFullPath))
                    {
                        file = serializer.Deserialize(reader) as TranslationFile;
                    }

                    if (file != null)
                    {
                        _translationStore.Add(langName, file);
                    }
                    else
                    {
                        _translationStore.Add(langName, new TranslationFile() { Messages = new Translation[] { } });
                    }
                }
                else // File not exists
                {
                    // Language folder present but missing message file
                    _translationStore.Add(langName, new TranslationFile() { Messages = new Translation[] {} });
                }
            }
        }

        public class TranslationFile
        {
            public Translation[] Messages;
        }

        public class Translation
        {
            public string MsgId;
            public string MsgStr;
        }
    }
}
