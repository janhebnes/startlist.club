using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Xml.Serialization;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Models.Training.Catalogue
{
    /// <summary>
    /// A training program. Example: SPL-S (SPL, spilstart)
    /// </summary>
    public class Training2Program
    {
        [Key]
        public int Training2ProgramId { get; set; }
        [Required]
        [LocalizedDisplayName("Short Name")]
        public string ShortName { get; set; }
        [Required]
        [LocalizedDisplayName("Name")]
        public string Name { get; set; }
        [LocalizedDisplayName("Notes")]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public string Notes{ get; set; }
        [LocalizedDisplayName("URL")]
        public string Url{ get; set; }

        [XmlIgnore]
        public virtual ICollection<Training2Lesson> Lessons { get; set; } // N:M

        public Training2Program()
        {
            Lessons = new HashSet<Training2Lesson>();
        }
        public Training2Program(string shortName, string name, string notes)
        {
            ShortName = shortName;
            Name = name;
            Notes = notes;
            Lessons = new HashSet<Training2Lesson>();
        }
    }
}