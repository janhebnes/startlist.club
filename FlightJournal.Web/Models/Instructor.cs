using System;
using System.ComponentModel.DataAnnotations;

namespace FlightJournal.Web.Models
{
    public class Instructor
    {
        [Key]
        public int InstructorId { get; set; }

        // Flight Instructor is part of the DTO Flight School organisation and therefore several parameters are tracked
    }
}