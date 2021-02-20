using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FlightJournal.Web.Controllers;
using FlightJournal.Web.Extensions;
using FlightJournal.Web.Translations;

namespace FlightJournal.Web.Models
{
    public class Flight
    {
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
        [LocalizedDisplayName("Date")]
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
        [LocalizedDisplayName("Departure")]
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
        [LocalizedDisplayName("Landing")]
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
        [LocalizedDisplayName("Landing Count")]
        [Required]
        public int LandingCount { get; set; }

        [LocalizedDisplayName("Plane")]
        [Required]
        public int PlaneId { get; set; }
        [LocalizedDisplayName("Plane")]
        public virtual Plane Plane { get; set; }

        [LocalizedDisplayName("Pilot")]
        [Required]
        public int PilotId { get; set; }
        public virtual Pilot Pilot { get; set; }

        //Pilot function time (copilot/instructor or passenger)

        // Pilot position  PilotInCommand (Luftfartøjschef) / Student (Elev) / FlightInstructor (Instruktør) / FlightExaminer (Kontrollant) / Passenger

        // FlightTrainingLessonId (TMG ? S ? A normer B normer )
        // FlightTrainingLessonApprovedByPilotId (int Pilot approving the lesson (on solo flights this might not be an FI onboard))
        // FlightTrainingLesson also has aspects not controlled by a flight 

        // Flight Type FlightSchool (Elev/skoling) / ProficiencyCheck (Færdighedstræning) / Passenger

        [LocalizedDisplayName("Co-Pilot")]
        public int? PilotBackseatId { get; set; }
        public virtual Pilot PilotBackseat { get; set; }
        
        ///// <summary>
        ///// When flying two seats the passenger name can be registered instead of using the Description field
        ///// </summary>
        //public string PassengerName { get; set; }

        //PilotBackseat function Co-Pilot / Instructor / Passenger 
        //PilotBackseat Name

        [LocalizedDisplayName("Take-off method")]
        [Required]
        public int StartTypeId { get; set; }
        public virtual StartType StartType { get; set; }

        [LocalizedDisplayName("Take-off")]
        public int StartedFromId { get; set; }
        [LocalizedDisplayName("Take-off")]
        [ForeignKey("StartedFromId")]
        public virtual Location StartedFrom { get; set; }

        [LocalizedDisplayName("Landing")]
        public int? LandedOnId { get; set; }
        [LocalizedDisplayName("Landing")]
        [ForeignKey("LandedOnId")]
        public virtual Location LandedOn { get; set; }

        [LocalizedDisplayName("Tacho Departure")]
        public double? TachoDeparture { get; set; }

        [LocalizedDisplayName("Tacho Landing")]
        public double? TachoLanding { get; set; }

        [LocalizedDisplayName("Task Distance")]
        public double? TaskDistance { get; set; }

        [LocalizedDisplayName("Description")]
        public string Description { get; set; }
        
        //[XmlIgnore]
        //public virtual ICollection<Note> Notes { get; set; }
        //[XmlIgnore]
        //public virtual ICollection<PilotLogEntry> PilotLogEntries { get; set; }

        [LocalizedDisplayName("Billing")]
        public int BetalerId { get; set; }
        public virtual Pilot Betaler { get; set; }

        public double StartCost { get; set; }
        public double FlightCost { get; set; }
        public double TachoCost { get; set; }

        [LocalizedDisplayName("Deleted")]
        public DateTime? Deleted { get; set; }

        [Required]
        [LocalizedDisplayName("Last updated")]
        public DateTime LastUpdated { get; set; }
        [LocalizedDisplayName("Last updated by")]
        public string LastUpdatedBy { get; set; }

        public int RecordKey { get; set; }
        public bool IsTrainingFlight => true; //TODO: figure out how to derive this from db

        public static string CsvHeaders
        {
            get
            {
                return
                    @"Dato;Fly;Forsæde medlemsnr;Forsæde navn;Forsæde unionsnr;Bagsæde medlemsnr;Bagsæde navn;Bagsæde unionsnr;Betaler medlemsnr;Betaler navn;Betaler unionsnr;Startet;Landed;Flyvetid;Tacho start;Tacho slut;Tacho;Note;Km;Starttype;Start fra;Landed på;Klub;Sidst opdateret;Sidst opdateret af\n";
            }
        }

        public string ToCsvString()
        {
                return
                string.Format(
                    "{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};{18};{19};{20};{21};{22};{23};{24}\n",
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
                    this.Duration.TotalHoursWithMinutesAsDecimal(),
                    this.TachoDeparture,
                    this.TachoLanding,
                    this.Tacho,
                    this.Description,
                    this.TaskDistance,
                    this.StartType,
                    this.StartedFrom,
                    this.LandedOn,
                    this.Pilot != null ? this.Pilot.Club.ShortName : string.Empty,
                    this.LastUpdated,
                    this.LastUpdatedBy);
            
        }

        [LocalizedDisplayName("Cost")]
        public double TotalCost
        {
            get
            {
                return this.FlightCost + this.StartCost + this.TachoCost;    
            }
        }

        [LocalizedDisplayName("Tacho")]
        public double Tacho
        {
            get 
            {
                if (this.TachoLanding.HasValue && this.TachoDeparture.HasValue && this.TachoLanding.Value > this.TachoDeparture.Value)
                {
                    return this.TachoLanding.Value - this.TachoDeparture.Value;
                }
                return 0;
            }
        }

        /// <summary>
        /// Calculates flight time based on Landing and departure time
        /// </summary>
        /// <returns>
        /// Timespan if not zero
        /// </returns>
        [LocalizedDisplayName("Duration")]
        public TimeSpan Duration
        {
            get
            {
                if (this.Departure.HasValue && this.Landing.HasValue && this.Landing > this.Departure)
                {
                    return this.Landing.Value - this.Departure.Value;
                }

                return TimeSpan.Zero;    
            }
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

        /// <summary>
        /// Return true if the current Flight is relevant for the Currently Selected Club
        /// </summary>
        /// <returns></returns>
        public bool IsCurrentClubPilots()
        {
            return IsCurrentClubPilots(this);
        }

        /// <summary>
        /// Return true if the current Flight is relevant for the Currently Selected Club
        /// </summary>
        /// <remarks>usable direcly in linq where statements</remarks>
        public static bool IsCurrentClubPilots(Flight arg)
        {
            return ClubController.CurrentClub.ShortName == null 
                || (arg.Pilot != null && arg.Pilot.ClubId == ClubController.CurrentClub.ClubId)
                || (arg.PilotBackseat != null && arg.PilotBackseat.ClubId == ClubController.CurrentClub.ClubId)
                || (arg.Betaler != null && arg.Betaler.ClubId == ClubController.CurrentClub.ClubId)
                || (arg.Pilot == null && arg.PilotBackseat == null && arg.Betaler == null);
        }
    }
}
