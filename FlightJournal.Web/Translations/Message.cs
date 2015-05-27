using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightJournal.Web.Translations
{
    /// <summary>
    /// A piece of text that may have been translated into another language.
    /// </summary>
    public class Message
    {
        /// <summary>
        /// The original text of the message, as scraped from source code.
        /// </summary>
        public string MsgID { get; set; }

        /// <summary>
        /// The translated text, or an empty string if the message has not yet been translated.
        /// </summary>
        public string MsgStr { get; set; }

        /// <summary>
        /// A list of comments found with this message in its .po file.
        /// Will only be populated if the loadComments flag is set while loading the Localization.
        /// </summary>
        public List<string> Comments { get; set; }

        /// <summary>
        /// A list of filename:linenumber pairs representing where this message
        /// was found in the source code.
        /// Will only be populated if the loadComments flag is set while loading the Localization.
        /// </summary>
        public List<string> Contexts { get; set; }

        // A temporary short ID that can be used for lookups.
        // (but don't store it since it will change depending on the order in which messages are loaded)
        public int AutoID { get; set; }


        private static int _nextAutoID;

        /// <summary>
        /// Create a new, empty Message
        /// </summary>
        public Message()
        {
            AutoID = _nextAutoID++;
            MsgID = "";
            MsgStr = "";
            Contexts = new List<string>();
            Comments = new List<string>();
        }

        /// <summary>
        /// Create a new Message, setting the msgID to the supplied value
        /// </summary>
        /// <param name="msgID"></param>
        public Message(string msgID)
            : this()
        {
            MsgID = msgID;
        }

        /// <summary>
        /// Create a new Message, setting the supplied values
        /// </summary>
        /// <param name="msgID"></param>
        /// <param name="msgStr"></param>
        public Message(string msgID, string msgStr)
            : this()
        {
            MsgID = msgID;
            MsgStr = msgStr;
        }

        /// <summary>
        /// Create a new Message, setting the supplied values
        /// </summary>
        /// <param name="msgID"></param>
        /// <param name="msgStr"></param>
        /// <param name="firstContext"></param>
        public Message(string msgID, string msgStr, string firstContext)
            : this()
        {
            MsgID = msgID;
            MsgStr = msgStr;
            Contexts.Add(firstContext);
        }

        /// <summary>
        /// Return the translated version of this message. 
        /// If no translation is available, return the message itself
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (String.IsNullOrEmpty(MsgStr)) ? MsgID : MsgStr;
        }

        /// <summary>
        /// Return this message, formatted as it would appear in a .po file
        /// </summary>
        /// <returns></returns>
        public string ToPOBlock()
        {
            StringBuilder builder = new StringBuilder();
            foreach (string comment in Comments)
            {
                builder.AppendLine(comment);
            }

            return String.Format(@"
#: {0}
msgid ""{1}""
msgstr ""{2}""
"
                , builder
                , MsgID
                , MsgStr);
        }
    }
}
