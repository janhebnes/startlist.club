namespace FlightJournal.Web.Models
{
    public class Location
    {
        public int LocationId { get; set; }
        public string Name {get; set; }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
