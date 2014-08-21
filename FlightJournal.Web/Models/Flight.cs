using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Serialization;
using FlightJournal.Web.Controllers;

namespace FlightJournal.Web.Models
{
    public class Flight
    {
        private Pilot m_Pilot;
        private DateTime m_Date;
        private DateTime? m_Departure;
        private DateTime? m_Landing;

        public Flight()
        {
            this.FlightId = Guid.NewGuid();
            this.Date = DateTime.Today;
            this.LastUpdated = DateTime.Now;
            this.LandingCount = 1;
        }
        [Key]
        public Guid FlightId { get; set; }
        [DisplayName("Dato")]
        [DataType(DataType.Date)]
        public DateTime Date
        {
            get
            {
                return m_Date;
            }
            set
            {
                if (value != value.ToUniversalTime())
                {
                    m_Date = new DateTime(value.Year, value.Month, value.Day, 0, 0, 0);
                }
                else
                    m_Date = value;

                if (m_Date.TimeOfDay != new TimeSpan(0, 0, 0))
                    throw new ArgumentOutOfRangeException("DateTime is not a UTC Date set at 00:00:00 must be submitted, date: " + m_Date.ToString() + " value:" + value.ToString() + " universaltime: " + value.ToUniversalTime().ToString());
            }
        }
        [DisplayName("Startet")]
        [DataType(DataType.Time)]
        public DateTime? Departure
        {
            get { return m_Departure; }
            set
            {
                m_Departure = value;

                // Departure is slave to the flight date. 
                if ((m_Departure.HasValue) && (m_Departure.Value.ToShortDateString() != this.Date.ToShortDateString()))
                    this.m_Departure = new DateTime(this.Date.Year, this.Date.Month, this.Date.Day, m_Departure.Value.Hour, m_Departure.Value.Minute, m_Departure.Value.Second);
            }
        }
        [DisplayName("Landet")]
        [DataType(DataType.Time)]
        public DateTime? Landing
        {
            get { return m_Landing; }
            set
            {
                m_Landing = value;

                // Departure is slave to the flight date. 
                if ((m_Landing.HasValue) && (m_Landing.Value.ToShortDateString() != this.Date.ToShortDateString()))
                {
                    this.m_Landing = new DateTime(
                        this.Date.Year,
                        this.Date.Month,
                        this.Date.Day,
                        m_Landing.Value.Hour,
                        m_Landing.Value.Minute,
                        m_Landing.Value.Second);
                }

                if (this.m_Landing < this.m_Departure)
                {
                    var diff = this.m_Departure.Value.Subtract(this.m_Landing.Value);

                    // Handles minute difference related to data import
                    if (diff.Minutes < 2)
                    {
                        var dep = this.m_Departure;
                        this.m_Departure = this.m_Landing;
                        this.m_Landing = dep;
                    }
                    else
                    {
                        ////throw new Exception(
                        ////    string.Format(
                        ////        "Landing kan ikke registreres på grund af landingstid finder sted før starttid {0} < {1}.",
                        ////        this.m_Landing,
                        ////        this.m_Departure));    
                    }
                }
            }
        }
        [DisplayName("Antal")]
        [Required]
        public int LandingCount { get; set; }

        [DisplayName("Fly")]
        [Required]
        public int PlaneId { get; set; }
        public virtual Plane Plane { get; set; }
        [DisplayName("Pilot")]
        [Required]
        public int PilotId { get; set; }
        public virtual Pilot Pilot { get; set; }

        [DisplayName("Bagsæde")]
        public int? PilotBackseatId { get; set; }
        public virtual Pilot PilotBackseat { get; set; }

        [DisplayName("Start metode")]
        [Required]
        public int StartTypeId { get; set; }
        public virtual StartType StartType { get; set; }

        [DisplayName("Startsted")]
        public int StartedFromId { get; set; }
        [ForeignKey("StartedFromId")]
        public virtual Location StartedFrom { get; set; }

        [DisplayName("Landingssted")]
        public int? LandedOnId { get; set; }

        [ForeignKey("LandedOnId")]
        public virtual Location LandedOn { get; set; }

        [DisplayName("Tacho start")]
        public double? TachoDeparture { get; set; }

        [DisplayName("Tacho slut")]
        public double? TachoLanding { get; set; }
        
        [DisplayName("Opgave km")]
        public double? TaskDistance { get; set; }

        [DisplayName("Note")]
        public string Description { get; set; }
        [XmlIgnore]
        public virtual ICollection<Note> Notes { get; set; }
        //[XmlIgnore]
        //public virtual ICollection<PilotLogEntry> PilotLogEntries { get; set; }

        [DisplayName("Betaler")]
        public int BetalerId { get; set; }
        public virtual Pilot Betaler { get; set; }

        public double StartCost { get; set; }
        public double FlightCost { get; set; }
        public double TachoCost { get; set; }

        [Required]
        public DateTime LastUpdated { get; set; }
        public string LastUpdatedBy { get; set; }

        public int RecordKey { get; set; }

        public static string CsvHeaders
        {
            get
            {
                return
                    "Dato;Fly;Forsæde medlemsnr;Forsæde navn;Forsæde unionsnr;Bagsæde medlemsnr;Bagsæde navn;Bagsæde unionsnr;Betaler medlemsnr;Betaler navn;Betaler unionsnr;Startet;Landed;Flyvetid;Tacho start;Tacho slut;Tacho;Note;Km;Starttype;Start fra;Landed på;Sidst opdateret;Sidst opdateret af\n";
            }
        }

        public string ToCsvString()
        {
                return
                string.Format(
                    "{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};{18};{19};{20};{21};{22};{23}\n",
                    this.Date.ToShortDateString(),
                    this.Plane,
                    this.Pilot != null ? this.Pilot.MemberId : string.Empty,
                    this.Pilot != null ? this.Pilot.Name : string.Empty,
                    this.Pilot != null ? this.Pilot.UnionId : string.Empty,
                    this.PilotBackseat != null ? this.PilotBackseat.MemberId : string.Empty,
                    this.PilotBackseat != null ? this.PilotBackseat.Name : string.Empty,
                    this.PilotBackseat != null ? this.PilotBackseat.UnionId : string.Empty,
                    this.Betaler != null ? this.Betaler.MemberId : string.Empty,
                    this.Betaler != null ? this.Betaler.Name : string.Empty,
                    this.Betaler != null ? this.Betaler.UnionId : string.Empty,
                    this.Departure.HasValue ? this.Departure.Value.ToString("HH:mm") : string.Empty,
                    this.Landing.HasValue ? this.Landing.Value.ToString("HH:mm") : string.Empty,
                    new DateTime(this.FlightTime().Ticks).ToString("HH:mm"),
                    this.TachoDeparture,
                    this.TachoLanding,
                    this.TachoCount(),
                    this.Description,
                    this.TaskDistance,
                    this.StartType,
                    this.StartedFrom,
                    this.LandedOn,
                    this.LastUpdated,
                    this.LastUpdatedBy);
            
        }

        public double TotalCost()
        {
            return this.FlightCost + this.StartCost + this.TachoCost;
        }

        public double TachoCount()
        {
            if (this.TachoLanding.HasValue && this.TachoDeparture.HasValue && this.TachoLanding.Value > this.TachoDeparture.Value)
            {
                return this.TachoLanding.Value - this.TachoDeparture.Value;
            }
            return 0;
        }

        /// <summary>
        /// Calculates flight time based on Landing and departure time
        /// </summary>
        /// <returns>
        /// Timespan if not zero
        /// </returns>
        public TimeSpan FlightTime()
        {
            if (this.Departure.HasValue && this.Landing.HasValue && this.Landing > this.Departure)
            {
                return this.Landing.Value - this.Departure.Value;
            }

            return TimeSpan.Zero;
        }

        /// <summary>
        /// Return true if the current Flight is relevant for the Currently Selected Club
        /// </summary>
        /// <returns></returns>
        public bool IsCurrent()
        {
            return IsCurrent(this);
        }

        /// <summary>
        /// Return true if the current Flight is relevant for the Currently Selected Club
        /// </summary>
        /// <remarks>usable direcly in linq where statements</remarks>
        public static bool IsCurrent(Flight arg)
        {
            return ClubController.CurrentClub.ShortName == null
                || arg.StartedFromId == ClubController.CurrentClub.LocationId
                || arg.LandedOnId == ClubController.CurrentClub.LocationId
                || (arg.Pilot != null && arg.Pilot.ClubId == ClubController.CurrentClub.ClubId)
                || (arg.PilotBackseat != null && arg.PilotBackseat.ClubId == ClubController.CurrentClub.ClubId)
                || (arg.Betaler != null && arg.Betaler.ClubId == ClubController.CurrentClub.ClubId);
        }

    }
}
