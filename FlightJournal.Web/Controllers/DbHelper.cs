using System.Collections.Generic;
using System;
using System.Linq;
using FlightJournal.Web.Models;

namespace FlightJournal.Web.Controllers
{
    public class DbHelper
    {
        public static HashSet<Guid> IdsOfTrainingFlights(FlightContext db)
        {
            return db.AppliedExercises
                .Select(x => x.FlightId)
                .Distinct()
                .ToHashSet();
        }


        public static HashSet<Guid> IdsOfTrainingFlightsWithProgramId(FlightContext db, int id)
        {
            return db.AppliedExercises.Where(x => x.Program.Training2ProgramId == id)
                .Select(x => x.FlightId)
                .Distinct()
                .ToHashSet();
        }

        public static HashSet<Guid> IdsOfTrainingFlightsWithLesson(FlightContext db, int id)
        {
            return db.AppliedExercises.Where(x => x.Lesson.Training2LessonId == id)
                .Select(x => x.FlightId)
                .Distinct()
                .ToHashSet();
        }

    }
}