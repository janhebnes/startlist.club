using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Models;
using FlightJournal.Web.Models.Export;
using FlightJournal.Web.Models.Training.Flight;
using FlightJournal.Web.Models.Training.Predefined;

namespace FlightJournal.Web.FlightExport
{
    public interface IFlightExportModelCreator
    {
        public TrainingFlightHistoryExportViewModel CreateTrainingFlightsExportModel(IEnumerable<Flight> flights, IPrincipal? user = null);
        //TODO: possibly other creator methods - one without training info, maybe ?
    }



    public class TrainingFlightExportModelCreator : IFlightExportModelCreator
    {
        private readonly FlightContext _db;

        public TrainingFlightExportModelCreator(FlightContext db)
        {
            _db = db;
        }

        public TrainingFlightHistoryExportViewModel CreateTrainingFlightsExportModel(IEnumerable<Flight> flights, IPrincipal? user = null)
        {
            var flightModels = new List<TrainingFlightExportViewModel>();
            foreach (var f in flights)
            {
                var ae = _db.AppliedExercises.Where(x => x.FlightId == f.FlightId).Where(x => x.Grading != null && x.Grading.Value > 0).ToList();
                var program = ae.FirstOrDefault()?.Program;
                var annotation = _db.TrainingFlightAnnotations.FirstOrDefault(x => x.FlightId == f.FlightId);

                var partialExercises = ae.Select(x => new TrainingFlightPartialExerciseExportViewModel(x)).ToList();
                var flightPhaseComments = annotation.CommentsForFlight()
                    .Where(x => x.Value.Any())
                    .Select(x => new CommentInFlightPhaseExportViewModel(x.Key, x.Value));
                var maneuvers = annotation != null
                    ? annotation.Manouvres?.Select(man => new ManeuverExportViewModel(man))
                    : Enumerable.Empty<ManeuverExportViewModel>();
                var instructor = ae.FirstOrDefault(x => x.Instructor != null)?.Instructor;
                var m = new TrainingFlightExportViewModel
                {
                    FlightId = f.FlightId.ToString(),
                    Timestamp = f.Date.ToString("yyyy-MM-dd HH:mm"),
                    Registration = f.Plane.Registration,
                    Seats = f.Plane.Seats,
                    CompetitionId = f.Plane.CompetitionId,
                    FrontSeatOccupantName = f.Pilot.Name,
                    FrontSeatOccupantClubId = f.Pilot.MemberId,
                    FrontSeatOccupantUnionId = f.Pilot.UnionId,
                    BackSeatOccupantName = f.PilotBackseat?.Name,
                    BackSeatOccupantClubId = f.PilotBackseat?.MemberId,
                    BackSeatOccupantUnionId = f.PilotBackseat?.UnionId,
                    InstructorName = instructor?.Name,
                    InstructorClubId = instructor?.MemberId,
                    InstructorUnionId = instructor?.UnionId,
                    Airfield = f.StartedFrom.Name,
                    Duration = f.Duration.ToString("hh\\:mm"),
                    DurationInMinutes = Math.Round(f.Duration.TotalMinutes),
                    TrainingProgramName = program?.Name,
                    TrainingProgramId = program?.ProgramIdForExport.ToString(),
                    PartialExercises = partialExercises,
                    FlightPhaseComments = flightPhaseComments,
                    Maneuvers = maneuvers,
                    Note = annotation?.Note
                };
                flightModels.Add(m);
            }
            return new TrainingFlightHistoryExportViewModel { Flights = flightModels, Timestamp = DateTimeOffset.Now, ExportingUser = user?.Identity?.Name };
        }
    }


    public static class TrainingFlightExtensions
    {
        public static Dictionary<CommentaryType, IEnumerable<Commentary>> CommentsForFlight(this TrainingFlightAnnotation annotation)
        {
            var db = new FlightContext();
            var commentsForPhasesInThisFlight = annotation?
                                                    .TrainingFlightAnnotationCommentCommentTypes?
                                                    .GroupBy(e => e.CommentaryType, e => e.Commentary, (phase, comments) => new { phase, comments })
                                                    .ToDictionary(
                                                        x => x.phase,
                                                        x => x.comments)
                                                ?? new Dictionary<CommentaryType, IEnumerable<Commentary>>();

            var commentsForAllPhases =
                db.CommentaryTypes
                    .OrderBy(c => c.DisplayOrder)
                    .ToDictionary(x => x, x => commentsForPhasesInThisFlight.GetOrDefault(x, Enumerable.Empty<Commentary>()));

            return commentsForAllPhases;
        }

    }
}