using System.Linq;
using System.Web;
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
            if (System.Web.HttpContext.Current == null)
            {
                return Any(club);
            }
            else
            {
                // Return Request Cache if set
                var context = new HttpContextWrapper(System.Web.HttpContext.Current);
                
                if (context.Items[$"ClubIsValid_{club}"] != null)
                    return (bool) context.Items[$"ClubIsValid_{club}"];

                context.Items.Add($"ClubIsValid_{club}", Any(club));
                return (bool) context.Items[$"ClubIsValid_{club}"];
            }

            bool Any(string s)
            {
                using (var shortDb = new FlightContext())
                {
                    return (shortDb.Clubs.Any(d => d.ShortName == s));
                }
            }
        }
    }
}