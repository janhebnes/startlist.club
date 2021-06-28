using System.Configuration;

namespace FlightJournal.Web.Configuration
{
    public class AutoExportConfigurationSection : ConfigurationSection
    {
        public AutoExportConfigurationSection() { }


        [ConfigurationProperty("", IsDefaultCollection = true)]
        public AutoExportConfigCollection AutoExports
        {
            get
            {
                AutoExportConfigCollection autoExportConfigCollection = (AutoExportConfigCollection)base[""];
                return autoExportConfigCollection;
            }
        }
    }


    public class AutoExportConfigCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new AutoExportConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as AutoExportConfigElement)?.Name ?? "????";
        }
        protected override string ElementName => "exportConfig";

        public override ConfigurationElementCollectionType CollectionType => ConfigurationElementCollectionType.BasicMap;
    }

    public class AutoExportConfigElement : ConfigurationElement
    {
        [ConfigurationProperty("username")]
        public string Username
        {
            get { return (string)this["username"]; }
            set { this["username"] = value; }
        }
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }
        [ConfigurationProperty("password")]
        public string Password
        {
            get { return (string)this["password"]; }
            set { this["password"] = value; }
        }
        [ConfigurationProperty("tokenurl")]
        public string TokenUrl
        {
            get { return (string)this["tokenurl"]; }
            set { this["tokenurl"] = value; }
        }
        [ConfigurationProperty("posturl")]
        public string PostUrl
        {
            get { return (string)this["posturl"]; }
            set { this["posturl"] = value; }
        }
        [ConfigurationProperty("intervalMinutes")]
        public int IntervalMinutes
        {
            get { return (int)this["intervalMinutes"]; }
            set { this["intervalMinutes"] = value; }
        }

    }
}