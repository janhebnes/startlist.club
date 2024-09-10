using System;
using System.Linq;
using System.Windows.Forms;
using FlightJournal.Web.Models;

namespace FlightJournal.Web.Repositories
{
    public interface IClubRepository
    {
        bool ClubExists(string shortName);
        /// <summary>
        /// Retrieving a club by its short name and creating a new Club object (a clone) and populating its properties with the values from the original club found in the database
        /// </summary>
        /// <param name="shortName"></param>
        /// <returns></returns>
        Club CloneClubByShortName(string shortName);
    }

    public class ClubRepository : IClubRepository
    {
        private readonly Func<FlightContext> _dbContextFactory; // Factory function to create FlightContext instances

        public ClubRepository(Func<FlightContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public bool ClubExists(string shortName)
        {
            using (var dbContext = _dbContextFactory()) // Create and dispose of DbContext within 'using'
            {
                return dbContext.Clubs.AsNoTracking().Any(c => c.ShortName == shortName);
            }
        }

        public Club CloneClubByShortName(string shortName)
        {
            using (var dbContext = _dbContextFactory())
            {
                var originalClub = dbContext.Clubs.AsNoTracking().SingleOrDefault(d => d.ShortName == shortName);
                if (originalClub == null) return null;

                var clubCopy = new Club
                {
                    ClubId = originalClub.ClubId,
                    Name = originalClub.Name,
                    ShortName = originalClub.ShortName,
                    LocationId = originalClub.LocationId,
                    Location = originalClub.Location,
                    ContactInformation = originalClub.ContactInformation,
                    Website = originalClub.Website
                };

                return clubCopy;
            }
        }
    }
}