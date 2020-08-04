using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models.Training
{
    /// <summary>
    /// A training program. Example: SPL-S (SPL, spilstart)
    /// </summary>
    public class Training2Program
    {
        [Key]
        public int Training2ProgramId { get; set; }
        [Required]
        public string ShortName { get; set; }
        [Required]
        public string Name { get; set; }
        public string Notes{ get; set; }
        public string Url{ get; set; }

        public virtual ICollection<Training2Lesson> Lessons { get; set; } // N:M

        public Training2Program(){}
        public Training2Program(string shortName, string name, string notes)
        {
            ShortName = shortName;
            Name = name;
            Notes = notes;
            Lessons = new HashSet<Training2Lesson>();
        }
    }
}