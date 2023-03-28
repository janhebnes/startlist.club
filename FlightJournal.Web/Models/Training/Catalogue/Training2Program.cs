using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Xml.Serialization;
using FlightJournal.Web.Translations;
using Newtonsoft.Json;

namespace FlightJournal.Web.Models.Training.Catalogue
{
    /// <summary>
    /// A training program. Example: SPL-S (SPL, spilstart)
    /// </summary>
    [JsonObject(MemberSerialization.OptOut)]
    public class Training2Program
    {
        [Key]
        [JsonIgnore]
        public int Training2ProgramId { get; set; }
        public Guid ProgramIdForExport { get; set; }


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

        [LocalizedDisplayName("EXPORT: Require UnionId to be defined for the pilot(s) in flights on this program")]
        public bool RequireUnionIdsForExport { get; set; } = true;

        [LocalizedDisplayName("URL")]
        public string Url{ get; set; }

        [LocalizedDisplayName("Official version")]
        public string Version { get; set; }

        [XmlIgnore]
        public virtual ICollection<Training2Lesson> Lessons { get; set; } // N:M

        [XmlIgnore]
        public virtual ICollection<Training.Flight.Training> TrainingProgram_Trainings { get; set; }

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