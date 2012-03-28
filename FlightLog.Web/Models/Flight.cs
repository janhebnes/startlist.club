using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace FlightLog.Models
{
    using System.ComponentModel;
    using System.Runtime.Remoting.Contexts;
    using System.Xml.Serialization;

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
        [DisplayName("Fly")]
        [Required]
        public int PlaneId { get; set; }
        public virtual Plane Plane { get; set; }
        [DisplayName("Pilot")]
        [Required]
        public int PilotId { get; set; }
        public virtual Pilot Pilot
        {
            get { return m_Pilot; }
            set
            {
                m_Pilot = value;
                if (Betaler == null) Betaler = m_Pilot;
            }
        }
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
        [XmlIgnore]
        public virtual ICollection<PilotLog> PilotLogs { get; set; }

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
                    "Dato;Fly;Forsæde medlemsnr;Forsæde navn;Forsæde DSvU;Bagsæde medlemsnr;Bagsæde navn;Bagsæde DSvU;Betaler medlemsnr;Betaler navn;Betaler DSvU;Startet;Landed;Flyvetid;Tacho;Start pris;Flyvetid pris ;Tacho pris;Total pris;Note;Km;Starttype;Started fra;Landed på;Sidst opdateret;Sidst opdateret af;Nøgle\n";
            }
        }

        public string ToCsvString()
        {
            return
                string.Format(
                    "{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10};{11};{12};{13};{14};{15};{16};{17};{18};{19};{20};{21};{22};{23};{24};{25};{26}\n",
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
                    this.TachoCount(),
                    this.FlightCost,
                    this.StartCost,
                    this.TachoCost,
                    this.TotalCost(),
                    this.Description,
                    this.TaskDistance,
                    this.StartType,
                    this.StartedFrom,
                    this.LandedOn,
                    this.LastUpdated,
                    this.LastUpdatedBy,
                    this.RecordKey);
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
    }
}
