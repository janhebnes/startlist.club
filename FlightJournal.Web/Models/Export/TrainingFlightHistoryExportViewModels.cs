using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;
using FlightJournal.Web.Models.Training.Flight;
using FlightJournal.Web.Models.Training.Predefined;
using Newtonsoft.Json;

namespace FlightJournal.Web.Models.Export
{
    /// <summary>
    /// Export of all flights (used for a specific year)
    /// </summary>
    internal class TrainingFlightHistoryExportViewModel
    {
        public DateTimeOffset Timestamp { get; set; }
        public string ExportingUser { get; set; }
        public List<TrainingFlightExportViewModel> Flights { get; set; } = new List<TrainingFlightExportViewModel>();
    }


    /// <summary>
    /// Export of a specific flight
    /// </summary>
    internal class TrainingFlightExportViewModel
    {
        public string FlightId { get; set; }
        public string Timestamp { get; set; }
        public string Registration { get; set; }
        public int Seats { get; set; }

        public string CompetitionId { get; set; }
        public string FrontSeatOccupantName { get; set; }
        public string FrontSeatOccupantClubId { get; set; }
        public string FrontSeatOccupantUnionId { get; set; }
        public string BackSeatOccupantName { get; set; }
        public string BackSeatOccupantClubId { get; set; }
        public string BackSeatOccupantUnionId { get; set; }
        public string InstructorName { get; set; }
        public string InstructorClubId { get; set; }
        public string InstructorUnionId { get; set; }
        public string Airfield { get; set; }
        public string Duration { get; set; }
        public double DurationInMinutes { get; set; }
        public string TrainingProgramName { get; set; }
        public string TrainingProgramId { get; set; }
        
        public string Note { get; set; }

        [Ignore] // not in CSV
        public IEnumerable<TrainingFlightPartialExerciseExportViewModel> PartialExercises { get; set; }
        [Ignore] // not in CSV
        public IEnumerable<CommentInFlightPhaseExportViewModel> FlightPhaseComments { get; set; }
        [Ignore] // not in CSV
        public IEnumerable<ManeuverExportViewModel> Maneuvers { get; set; }

        [JsonIgnore] // Not in JSON
        public string FlattenedPartialExerciseGradings => string.Join("|", PartialExercises?.Select(x => $"{x.ExerciseName} {x.PartialExerciseName} :{x.GradingName}"));
        [JsonIgnore] // Not in JSON
        public string FlattenedFlightPhaseComments => string.Join("|", FlightPhaseComments?.Select(x => $"{x.FlightPhase.Phase}:{string.Join(",",x.Comments)}"));
    }
    public class TrainingFlightPartialExerciseExportViewModel
    {
        public TrainingFlightPartialExerciseExportViewModel(AppliedExercise ae)
        {
            ExerciseName = ae.Lesson.Name;
            ExerciseId = ae.Lesson.LessonIdForExport.ToString();
            PartialExerciseName = ae.Exercise.Name;
            PartialExerciseId = ae.Exercise.ExerciseIdForExport.ToString();
            GradingName = ae.Grading?.Name;
            GradingId = ae.Grading?.GradingIdForExport.ToString();
        }
        public string ExerciseName { get; }
        public string ExerciseId { get; }
        public string PartialExerciseName { get; }
        public string PartialExerciseId { get; }
        public string GradingName { get; }
        public string GradingId { get; }
    }


    public class CommentInFlightPhaseExportViewModel
    {
        public CommentInFlightPhaseExportViewModel(CommentaryType phase, IEnumerable<Commentary> comments)
        {
            FlightPhase = new FlightPhaseExportViewModel(phase);
            Comments = comments.Select(c => new CommentExportViewmodel(c));
        }

        public FlightPhaseExportViewModel FlightPhase { get; set; }
        public IEnumerable<CommentExportViewmodel> Comments { get; set; }
    }
    public class FlightPhaseExportViewModel
    {
        public FlightPhaseExportViewModel(CommentaryType phase)
        {
            Phase = phase.CType;
            PhaseId = phase.CommentaryTypeIdForExport.ToString();
        }

        public string PhaseId { get; }

        public string Phase { get; }
    }

    public class CommentExportViewmodel
    {
        public CommentExportViewmodel(Commentary comment)
        {
            Comment = comment.Comment;
            CommentId = comment.CommentaryIdForExport.ToString();
        }
        public string CommentId { get; }
        public string Comment { get; }
    }

    public class ManeuverExportViewModel
    {
        public ManeuverExportViewModel(Manouvre m)
        {
            Maneuver = m.ManouvreItem + " " + m.Description;
            ManueverId = m.ManouvreIdForExport.ToString();
        }
        public string Maneuver { get; }
        public string ManueverId { get; }
    }

}