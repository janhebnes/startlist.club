using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models.Training.Predefined
{
    public class CommentaryType
    {
        [Key]
        public int CommentaryTypeId { get; set; }
        public string CType { get; set; }
        public virtual ICollection<Commentary> Commentaries { get; set; }
    }
}