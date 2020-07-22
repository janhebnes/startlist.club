using System;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models.Training
{
    /// <summary>
    /// A training program. Example: SPL-S (SPL, spilstart)
    /// </summary>
    public class Training2Program
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public string ShortName { get; set; }
        [Required]
        public string Name { get; set; }
        public string Notes{ get; set; }
        public string Url{ get; set; }

        public Training2Program(){}
        public Training2Program(string shortName, string name, string notes)
        {
            Id = Guid.NewGuid();
            ShortName = shortName;
            Name = name;
            Notes = notes;
        }
    }
}