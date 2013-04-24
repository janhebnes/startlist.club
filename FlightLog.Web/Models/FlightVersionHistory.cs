using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace FlightLog.Models
{
    using FlightLog.Controllers;

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
        public FlightVersionHistory(Flight f, System.Data.EntityState entityState)
        {
            this.State = entityState.ToString();

            this.FlightId = f.FlightId;
            this.Created = DateTime.Now;
            this.LastUpdated = f.LastUpdated;
            this.LastUpdatedBy = f.LastUpdatedBy;
            this.Description = f.Description;

            this.Date = f.Date;
            this.Departure = f.Departure;
            this.Landing = f.Landing;
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

        [Required]
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }

        /// <summary>
        /// Return true if the current Flight is relevant for the Currently Selected Club
        /// </summary>
        /// <returns></returns>
        public bool IsCurrent()
        {
            // Has the Club Default Location been touched
            var clubLocationId = ClubController.CurrentClub.DefaultStartLocationId;

            if (this.StartedFromId == clubLocationId) return true;
            if (this.LandedOnId == clubLocationId) return true;

            // Has a Club Pilot been involved in the flight
            var clubId = ClubController.CurrentClub.ClubId;
            if (this.Pilot != null && this.Pilot.ClubId == clubId) return true;
            if (this.PilotBackseat != null && this.PilotBackseat.Club.IsCurrent()) return true;
            if (this.Betaler != null && this.Betaler.Club.IsCurrent()) return true;

            return false;
        }
    }
}