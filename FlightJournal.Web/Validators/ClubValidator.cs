using FlightJournal.Web.Repositories;
using System.Web;

namespace FlightJournal.Web.Validators
{
    public interface IClubValidator
    {
        bool IsValid(string club);
    }

    public class ClubValidator : IClubValidator
    {
        private readonly IClubRepository _clubRepository;
        public ClubValidator(IClubRepository clubRepository)
        {
            _clubRepository = clubRepository;
        }

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
                return _clubRepository.ClubExists(s);
            }
        }
    }
}