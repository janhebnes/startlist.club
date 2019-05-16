using FlightJournal.Web.Translations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace FlightJournal.Web.Models
{
    public class FlightVersionHistory
    {
        public FlightVersionHistory()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FlightVersionHistory"/> class.
        /// </summary>
        /// <param name="flight">
        /// The flight.
        /// </param>
        /// <param name="entityState">
        /// The entity state.
        /// </param>
        public FlightVersionHistory(Flight f, EntityState entityState)
        {
            this.State = entityState.ToString();

            this.FlightId = f.FlightId;
            this.Created = DateTime.Now;
            this.Deleted = f.Deleted;
            this.LastUpdated = f.LastUpdated;
            this.LastUpdatedBy = f.LastUpdatedBy;
            this.Description = f.Description;

            this.Date = f.Date;
            this.Departure = f.Departure;
            this.Landing = f.Landing;
            this.LandingCount = f.LandingCount;
            this.PlaneId = f.PlaneId;
            this.PilotId = f.PilotId;
            this.PilotBackseatId = f.PilotBackseatId;
            this.BetalerId = f.BetalerId;
            this.StartTypeId = f.StartTypeId;
            this.StartedFromId = f.StartedFromId;
            this.LandedOnId = f.LandedOnId;
            this.TachoDeparture = f.TachoDeparture;
            this.TachoLanding = f.TachoLanding;
        }

        [Key]
        [Column(Order = 0)]
        public Guid FlightId { get; set; }
        [Key]
        [Column(Order = 1)]
        public DateTime Created { get; set; }
        public string State { get; set; }
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }
        [DataType(DataType.Time)]
        public DateTime? Departure { get; set; }
        [DataType(DataType.Time)]
        public DateTime? Landing { get; set; }
        [Required]
        public int LandingCount { get; set; }
        [Required]
        public int PlaneId { get; set; }
        public virtual Plane Plane { get; set; }
        public int PilotId { get; set; }
        public virtual Pilot Pilot { get; set; }
        public int? PilotBackseatId { get; set; }
        public virtual Pilot PilotBackseat { get; set; }
        public int StartTypeId { get; set; }
        public StartType StartType { get; set; }
        public int BetalerId { get; set; }
        public virtual Pilot Betaler { get; set; }
        public int StartedFromId { get; set; }
        [ForeignKey("StartedFromId")]
        public virtual Location StartedFrom { get; set; }
        public int? LandedOnId { get; set; }
        [ForeignKey("LandedOnId")]
        public virtual Location LandedOn { get; set; }
        public double? TachoDeparture { get; set; }
        public double? TachoLanding { get; set; }

        public string Description { get; set; }

        public DateTime? Deleted { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }

        /// <summary>
        /// Get the change log from previous to this 
        /// </summary>
        /// <param name="previous"></param>
        /// <returns></returns>
        public string GetChangeHistoryDescription(FlightVersionHistory previous)
        {
            if (previous == null)
                return _("Created");

            var log = new List<string>();

            if (this.Description != previous.Description)
                log.Add(_("Note") + $": {this.Description} <strike>{previous.Description}</strike>");

            if (this.Date != previous.Date)
                log.Add(_("Date") + $": {this.Date.ToString("dd-MMM-yyyy")} <strike>{previous.Date.ToString("dd-MMM-yyyy")}</strike>");

            if (this.Departure != previous.Departure)
            {
                if (this.Departure.HasValue && previous.Departure.HasValue)
                {
                    log.Add(_("Departure") + $": {this.Departure.Value.ToString("HH:mm")} <strike>{previous.Departure.Value.ToString("HH:mm")}</strike>");
                }
                else if (this.Departure.HasValue && !previous.Departure.HasValue)
                {
                    log.Add(_("Departure") + $": {this.Departure.Value.ToString("HH:mm")}");
                }
                else //if (!this.Departure.HasValue && previous.Departure.HasValue)
                {
                    log.Add(_("Departure") + $": <strike>{previous.Departure.Value.ToString("HH:mm")}</strike>");
                }
            }

            if (this.Landing != previous.Landing)
            {
                if (this.Landing.HasValue && previous.Landing.HasValue)
                {
                    log.Add(_("Landing") + $": {this.Landing.Value.ToString("HH:mm")} <strike>{previous.Landing.Value.ToString("HH:mm")}</strike>");
                }
                else if (this.Landing.HasValue && !previous.Landing.HasValue)
                {
                    log.Add(_("Landing") + $": {this.Landing.Value.ToString("HH:mm")}");
                }
                else //if (!this.Landing.HasValue && previous.Landing.HasValue)
                {
                    log.Add(_("Landing") + $": <strike>{previous.Landing.Value.ToString("HH:mm")}</strike>");
                }
            }

            if (this.LandingCount != previous.LandingCount)
                log.Add(_("Landing Count") + $": {this.LandingCount} <strike>{previous.LandingCount}</strike>");

            throw new NotImplementedException("This way of providing the change history has not been completed, i am unsure if it is the right direction.");

            //this.PlaneId = previous.PlaneId;
            //this.PilotId = previous.PilotId;
            //this.PilotBackseatId = previous.PilotBackseatId;
            //this.BetalerId = previous.BetalerId;
            //this.StartTypeId = previous.StartTypeId;
            //this.StartedFromId = previous.StartedFromId;
            //this.LandedOnId = previous.LandedOnId;
            //this.TachoDeparture = previous.TachoDeparture;
            //this.TachoLanding = previous.TachoLanding;

            return string.Join(", ",log.ToArray());
        }

        private string _(string resourceId)
        {
            return Internationalization.GetText(resourceId, Internationalization.LanguageCode);
        }
    }
}