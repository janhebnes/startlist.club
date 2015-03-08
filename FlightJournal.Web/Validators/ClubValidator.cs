using System.Linq;
using FlightJournal.Web.Models;

namespace FlightJournal.Web.Validators
{
    public interface IClubValidator
    {
        bool IsValid(string club);
    }

    public class ClubValidator : IClubValidator
    {
        public bool IsValid(string club)
        {
            using (var shortDb = new FlightContext())
            {
                return shortDb.Clubs.Any(d => d.ShortName == club);
            }
        }
    }
}