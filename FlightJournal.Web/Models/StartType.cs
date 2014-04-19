using System;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models
{
    [Serializable]
    public class StartType
    {
        [Key]
        public int StartTypeId { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }

        public int? ClubId { get; set; }
        public virtual Club Club { get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
