using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FlightJournal.Web.Models
{
    public class ListenerArea
    {
        [Key]
        public int ListenerAreaId { get; set; }

        [Required]
        public double Latitude { get; set; }
        [Required]
        public double Longitude { get; set; }
        // radius in km
        [Required]
        public double Radius { get; set; }
        public string Note { get; set; }
        public bool IsValid => Radius > 0
                               && Longitude >= -180 && Longitude <= 180
                               && Latitude >= -90 && Latitude <= 90;

        public override string ToString()
        {
            return
                $"({Latitude.ToString(CultureInfo.InvariantCulture)}, {Longitude.ToString(CultureInfo.InvariantCulture)}), radius {Radius.ToString(CultureInfo.InvariantCulture)} km";
        }
    }

}