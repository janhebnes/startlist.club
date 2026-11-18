using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using FlightJournal.Web.Models.Export;

namespace FlightJournal.Web.Models.Training
{
    /// <summary>Which training flights to load. Values are captured by the caller; nothing here reads request state.</summary>
    public sealed class TrainingHistoryFilter
    {
        public DateTime? From { get; set; }           // inclusive, Flight.Date
        public DateTime? To { get; set; }             // inclusive
        public int? PilotId { get; set; }             // front OR back seat
        public int? ProgramId { get; set; }           // exercise filter (PilotLessons)
        public int? LessonId { get; set; }
        public ClubScope Club { get; set; }           // null = unfiltered; values captured, never the static
    }

    /// <summary>Bounds a requested date range, so no single request reads unlimited history.</summary>
    public static class TrainingHistoryRange
    {
        /// <summary>Longest date range served in one request; longer ranges are clamped.</summary>
        public const int MaxDays = 366;

        /// <summary>Limits the range to <see cref="MaxDays"/> by moving its start (the end date is kept); returns true when it did.</summary>
        public static bool Clamp(DateTime fromDate, DateTime toDate, out DateTime clampedFrom)
        {
            clampedFrom = fromDate;
            if ((toDate - fromDate).TotalDays <= MaxDays)
                return false;
            clampedFrom = toDate.AddDays(-MaxDays);
            return true;
        }
    }

    public sealed class ClubScope
    {
        public int ClubId { get; set; }
        public int LocationId { get; set; }
    }

    public sealed class FlightRowData
    {
        public Guid FlightId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? Departure { get; set; }
        public DateTime? Landing { get; set; }
        public int LandingCount { get; set; }
        public string PlaneCompetitionId { get; set; }
        public string PlaneRegistration { get; set; }
        public string PilotName { get; set; }
        public string PilotMemberId { get; set; }
        public string PilotUnionId { get; set; }
        public int? PilotClubId { get; set; }
        public bool HasBackseat { get; set; }
        public string BackseatUnionId { get; set; }
        public int? BackseatClubId { get; set; }
        public int? BetalerClubId { get; set; }
        public string StartedFromName { get; set; }
    }

    public sealed class ExerciseRow
    {
        public int AppliedExerciseId { get; set; }
        public Guid FlightId { get; set; }
        public string ProgramShortName { get; set; }
        public bool ProgramRequiresUnionIds { get; set; }
        public int LessonId { get; set; }
        public string LessonName { get; set; }
        public int LessonDisplayOrder { get; set; }
        public bool LessonCanDual { get; set; }
        public bool LessonCanSolo { get; set; }
        public string ExerciseName { get; set; }
        public int ExerciseDisplayOrder { get; set; }
        public int? GradingValue { get; set; }
        public bool HasInstructor { get; set; }
        public string InstructorName { get; set; }
        public string InstructorClubShortName { get; set; }
        public string InstructorUnionId { get; set; }
    }

    public sealed class AnnotationRow
    {
        public Guid FlightId { get; set; }
        public int AnnotationId { get; set; }
        public string Note { get; set; }
    }

    public sealed class PhaseCommentRow
    {
        public Guid FlightId { get; set; }
        public int AnnotationId { get; set; }
        public int PhaseId { get; set; }
        public string PhaseCType { get; set; }
        public int PhaseDisplayOrder { get; set; }
        public int CommentaryId { get; set; }
        public string Comment { get; set; }
    }

    public sealed class ManoeuvreRow
    {
        public Guid FlightId { get; set; }
        public int AnnotationId { get; set; }
        public int ManouvreId { get; set; }
        public string IconCssClass { get; set; }
        public string ManouvreItem { get; set; }
    }

    /// <summary>One training flight, flattened for the history list.</summary>
    public sealed class TrainingFlightRow
    {
        public Guid FlightId { get; set; }
        public DateTime Date { get; set; }
        public string Timestamp { get; set; }
        public string Plane { get; set; }
        public string FrontSeatOccupant { get; set; }
        public string Instructor { get; set; }
        public string Airfield { get; set; }
        public string Duration { get; set; }
        public int LandingCount { get; set; }
        public string TrainingProgramName { get; set; }
        public string PrimaryLessonName { get; set; }
        public string ExercisesFull { get; set; }
        public string ExercisesShort { get; set; }
        public string Manouvres { get; set; }
        public string Note { get; set; }
        public bool PassesValidation { get; set; }
        public IReadOnlyList<string> Violations { get; set; }
        public bool IsCurrentClubPilots { get; set; }
    }

    /// <summary>
    /// The training-history read model. Loads flat rows in a fixed number of queries, independent of the
    /// number of flights: no id lists, no lazy loading, all filters in SQL.
    /// </summary>
    public sealed class TrainingHistoryQuery
    {
        private readonly FlightContext _db;

        public TrainingHistoryQuery(FlightContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Training flights matching the filter. A flight is a training flight when it has applied exercises
        /// (an EXISTS on the indexed AppliedExercises.FlightId); Flights.HasTrainingData is not reliable.
        /// </summary>
        public IQueryable<Models.Flight> TrainingFlights(TrainingHistoryFilter filter)
        {
            IQueryable<Flight.AppliedExercise> exercises = _db.AppliedExercises.AsNoTracking();
            if (filter.ProgramId.HasValue)
            {
                var programId = filter.ProgramId.Value;
                exercises = exercises.Where(ae => ae.Program.Training2ProgramId == programId);
            }
            if (filter.LessonId.HasValue)
            {
                var lessonId = filter.LessonId.Value;
                exercises = exercises.Where(ae => ae.Lesson.Training2LessonId == lessonId);
            }

            // Tracking is left to the caller: Load projects (untracked), the exports still read entities
            var flights = _db.Flights
                .Where(f => f.Deleted == null && exercises.Any(ae => ae.FlightId == f.FlightId));

            if (filter.From.HasValue)
            {
                var from = filter.From.Value;
                flights = flights.Where(f => f.Date >= from);
            }
            if (filter.To.HasValue)
            {
                var to = filter.To.Value;
                flights = flights.Where(f => f.Date <= to);
            }
            if (filter.PilotId.HasValue)
            {
                var pilotId = filter.PilotId.Value;
                flights = flights.Where(f => f.PilotId == pilotId || f.PilotBackseatId == pilotId);
            }
            if (filter.Club != null)
            {
                var locationId = filter.Club.LocationId;
                var clubId = filter.Club.ClubId;
                flights = flights.Where(f =>
                    f.StartedFromId == locationId
                    || f.LandedOnId == locationId
                    || f.Pilot.ClubId == clubId
                    || f.PilotBackseat.ClubId == clubId
                    || f.Betaler.ClubId == clubId);
            }
            return flights;
        }

        /// <param name="filter">Which flights to load.</param>
        /// <param name="currentClub">The selected club, used only for the IsCurrentClubPilots flag (null = no club selected).</param>
        public IReadOnlyList<TrainingFlightRow> Load(TrainingHistoryFilter filter, ClubScope currentClub)
        {
            var flights = TrainingFlights(filter).AsNoTracking();

            var flightRows = flights
                .OrderBy(f => f.Date).ThenBy(f => f.FlightId)
                .Select(f => new FlightRowData
                {
                    FlightId = f.FlightId,
                    Date = f.Date,
                    Departure = f.Departure,
                    Landing = f.Landing,
                    LandingCount = f.LandingCount,
                    PlaneCompetitionId = f.Plane.CompetitionId,
                    PlaneRegistration = f.Plane.Registration,
                    PilotName = f.Pilot.Name,
                    PilotMemberId = f.Pilot.MemberId,
                    PilotUnionId = f.Pilot.UnionId,
                    PilotClubId = (int?)f.Pilot.ClubId,
                    HasBackseat = f.PilotBackseatId != null,
                    BackseatUnionId = f.PilotBackseat.UnionId,
                    BackseatClubId = (int?)f.PilotBackseat.ClubId,
                    BetalerClubId = (int?)f.Betaler.ClubId,
                    StartedFromName = f.StartedFrom.Name
                })
                .ToList();
            if (flightRows.Count == 0)
                return new List<TrainingFlightRow>();

            // Each detail query joins on the same flight predicate; ordering follows the tables' keys, as the lazy loads did
            var exerciseRows = (
                    from ae in _db.AppliedExercises.AsNoTracking()
                    join f in flights on ae.FlightId equals f.FlightId
                    orderby ae.AppliedExerciseId
                    select new ExerciseRow
                    {
                        AppliedExerciseId = ae.AppliedExerciseId,
                        FlightId = ae.FlightId,
                        ProgramShortName = ae.Program.ShortName,
                        ProgramRequiresUnionIds = ae.Program.RequireUnionIdsForExport,
                        LessonId = ae.Lesson.Training2LessonId,
                        LessonName = ae.Lesson.Name,
                        LessonDisplayOrder = ae.Lesson.DisplayOrder,
                        LessonCanDual = ae.Lesson.CanHaveDualFlightDuration,
                        LessonCanSolo = ae.Lesson.CanHaveSoloFlightDuration,
                        ExerciseName = ae.Exercise.Name,
                        ExerciseDisplayOrder = ae.Exercise.DisplayOrder,
                        GradingValue = (int?)ae.Grading.Value,
                        HasInstructor = ae.Instructor != null,
                        InstructorName = ae.Instructor.Name,
                        InstructorClubShortName = ae.Instructor.Club.ShortName,
                        InstructorUnionId = ae.Instructor.UnionId
                    })
                .ToList();

            var annotationRows = (
                    from a in _db.TrainingFlightAnnotations.AsNoTracking()
                    join f in flights on a.FlightId equals f.FlightId
                    orderby a.TrainingFlightAnnotationId
                    select new AnnotationRow { FlightId = a.FlightId, AnnotationId = a.TrainingFlightAnnotationId, Note = a.Note })
                .ToList();

            var commentRows = (
                    from c in _db.TrainingFlightAnnotationCommentCommentTypes.AsNoTracking()
                    join f in flights on c.TrainingFlightAnnotation.FlightId equals f.FlightId
                    orderby c.TrainingFlightAnnotationId, c.CommentaryId, c.CommentaryTypeId
                    select new PhaseCommentRow
                    {
                        FlightId = c.TrainingFlightAnnotation.FlightId,
                        AnnotationId = c.TrainingFlightAnnotationId,
                        PhaseId = c.CommentaryTypeId,
                        PhaseCType = c.CommentaryType.CType,
                        PhaseDisplayOrder = c.CommentaryType.DisplayOrder,
                        CommentaryId = c.CommentaryId,
                        Comment = c.Commentary.Comment
                    })
                .ToList();

            var manoeuvreRows = (
                    from a in _db.TrainingFlightAnnotations.AsNoTracking()
                    join f in flights on a.FlightId equals f.FlightId
                    from m in a.Manouvres
                    orderby a.TrainingFlightAnnotationId, m.ManouvreId
                    select new ManoeuvreRow
                    {
                        FlightId = a.FlightId,
                        AnnotationId = a.TrainingFlightAnnotationId,
                        ManouvreId = m.ManouvreId,
                        IconCssClass = m.IconCssClass,
                        ManouvreItem = m.ManouvreItem
                    })
                .ToList();

            return TrainingHistoryRowBuilder.Build(flightRows, exerciseRows, annotationRows, commentRows, manoeuvreRows, currentClub);
        }
    }

    /// <summary>Pure in-memory assembly of history rows; formats exactly as the entity-based version did.</summary>
    public static class TrainingHistoryRowBuilder
    {
        public static IReadOnlyList<TrainingFlightRow> Build(
            IEnumerable<FlightRowData> flights,
            IEnumerable<ExerciseRow> exercises,
            IEnumerable<AnnotationRow> annotations,
            IEnumerable<PhaseCommentRow> comments,
            IEnumerable<ManoeuvreRow> manoeuvres,
            ClubScope currentClub)
        {
            var exercisesByFlight = exercises.OrderBy(x => x.AppliedExerciseId).ToLookup(x => x.FlightId);
            // A flight may have more than one annotation row; the first (lowest id) is the one shown, as before
            var annotationByFlight = annotations
                .GroupBy(x => x.FlightId)
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.AnnotationId).First());
            var commentsByAnnotation = comments.ToLookup(x => x.AnnotationId);
            var manoeuvresByAnnotation = manoeuvres.ToLookup(x => x.AnnotationId);

            var rows = new List<TrainingFlightRow>();
            foreach (var f in flights)
            {
                var flightExercises = exercisesByFlight[f.FlightId].ToList();
                annotationByFlight.TryGetValue(f.FlightId, out var annotation);
                var flightComments = annotation != null ? commentsByAnnotation[annotation.AnnotationId] : Enumerable.Empty<PhaseCommentRow>();
                var flightManoeuvres = annotation != null ? manoeuvresByAnnotation[annotation.AnnotationId] : Enumerable.Empty<ManoeuvreRow>();

                var departure = OnFlightDate(f.Date, f.Departure);
                var landing = OnFlightDate(f.Date, f.Landing);
                var duration = departure.HasValue && landing.HasValue && landing > departure ? landing.Value - departure.Value : TimeSpan.Zero;

                var validator = TrainingFlightExportValidator.Validate(ValidationInput(f, duration, flightExercises));

                var instructor = flightExercises.FirstOrDefault(x => x.HasInstructor);
                var allComments = new List<string> { annotation?.Note };
                allComments.AddRange(PhaseComments(flightComments));

                rows.Add(new TrainingFlightRow
                {
                    FlightId = f.FlightId,
                    Date = f.Date,
                    Timestamp = (landing ?? f.Date).ToString("yyyy-MM-dd HH:mm"),
                    Plane = $"{f.PlaneCompetitionId} ({f.PlaneRegistration})",
                    FrontSeatOccupant = $"{f.PilotName} ({f.PilotMemberId})",
                    Instructor = instructor != null ? $"{instructor.InstructorName} ({instructor.InstructorClubShortName})" : "",
                    Airfield = f.StartedFromName,
                    Duration = duration.ToString("hh\\:mm"),
                    LandingCount = f.LandingCount,
                    TrainingProgramName = string.Join(", ", flightExercises.Select(x => x.ProgramShortName).Distinct()),
                    PrimaryLessonName = PrimaryLessonName(flightExercises),
                    ExercisesFull = string.Join(", ", flightExercises
                        .OrderBy(x => x.LessonDisplayOrder)
                        .ThenBy(x => x.ExerciseDisplayOrder)
                        .Select(x => $"{x.LessonName}-{x.ExerciseName} ({x.GradingValue})")),
                    ExercisesShort = string.Join(", ", flightExercises
                        .OrderBy(x => x.LessonDisplayOrder)
                        .Select(x => x.LessonName)
                        .Distinct()),
                    Manouvres = string.Join(", ", flightManoeuvres
                        .OrderBy(x => x.ManouvreId)
                        .Select(x => $"<i class='{x.IconCssClass}'></i>{x.ManouvreItem}")),
                    Note = string.Join(" - ", allComments.Where(x => !string.IsNullOrEmpty(x))),
                    PassesValidation = validator.IsValid,
                    Violations = validator.Violations,
                    IsCurrentClubPilots = IsCurrentClubPilots(f, currentClub)
                });
            }
            return rows;
        }

        /// <summary>The Flight entity's setters move departure and landing onto the flight's date; mirror that for raw values.</summary>
        private static DateTime? OnFlightDate(DateTime date, DateTime? time)
        {
            if (!time.HasValue || time.Value.Date == date.Date)
                return time;
            return new DateTime(date.Year, date.Month, date.Day, time.Value.Hour, time.Value.Minute, time.Value.Second);
        }

        /// <summary>The most-graded lesson; ties go to the highest display order (the last one seen on equal order).</summary>
        private static string PrimaryLessonName(IReadOnlyList<ExerciseRow> exercises)
        {
            var lessons = exercises
                .GroupBy(x => x.LessonId)
                .Select(g => new { First = g.First(), Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();
            if (lessons.Count == 0)
                return "";
            return lessons
                .Where(x => x.Count == lessons[0].Count)
                .OrderBy(x => x.First.LessonDisplayOrder)
                .Last()
                .First.LessonName;
        }

        /// <summary>"Phase: comment,comment" per phase that has comments, in phase display order.</summary>
        private static IEnumerable<string> PhaseComments(IEnumerable<PhaseCommentRow> comments)
        {
            return comments
                .GroupBy(x => x.PhaseId)
                .OrderBy(g => g.First().PhaseDisplayOrder)
                .ThenBy(g => g.Key)
                .Select(g => $"{g.First().PhaseCType}: {string.Join(",", g.OrderBy(x => x.CommentaryId).Select(x => x.Comment))}");
        }

        private static TrainingFlightValidationInput ValidationInput(FlightRowData f, TimeSpan duration, IReadOnlyList<ExerciseRow> exercises)
        {
            return new TrainingFlightValidationInput
            {
                DurationMinutes = duration.TotalMinutes,
                HasBackseat = f.HasBackseat,
                PilotUnionIdMissing = f.PilotUnionId == null,
                BackseatUnionIdMissing = f.HasBackseat && f.BackseatUnionId == null,
                Exercises = exercises.Select(x => new TrainingExerciseValidationInput
                {
                    RequiresUnionIds = x.ProgramRequiresUnionIds,
                    InstructorUnionIdMissing = x.HasInstructor && x.InstructorUnionId == null,
                    CanDual = x.LessonCanDual,
                    CanSolo = x.LessonCanSolo
                }).ToList()
            };
        }

        private static bool IsCurrentClubPilots(FlightRowData f, ClubScope club)
        {
            return club == null
                || f.PilotClubId == club.ClubId
                || f.BackseatClubId == club.ClubId
                || f.BetalerClubId == club.ClubId
                || (f.PilotClubId == null && f.BackseatClubId == null && f.BetalerClubId == null);
        }
    }
}
