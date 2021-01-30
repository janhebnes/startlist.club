using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models.Training.Predefined
{
    public class Commentary
    {
        [Key]
        public int CommentaryId { get; set; }
        public string Comment { get; set; }
        public int CommentaryTypeId { get; set; }
        public virtual ICollection<CommentaryType> CommentaryTypes { get; set; }
    }
}