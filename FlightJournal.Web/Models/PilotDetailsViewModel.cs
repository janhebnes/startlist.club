using System;
using System.ComponentModel;
using System.Linq;
using FlightJournal.Web.Models.Training;

namespace FlightJournal.Web.Models
{
    public class PilotDetailsViewModel
    {
        public PilotDetailsViewModel()
        {

        }
        public PilotDetailsViewModel(Pilot pilot, IQueryable<PilotInTrainingProgram> trainingProgramsForPilot)
        {
            this.PilotId = pilot.PilotId;
            this.ClubId = pilot.ClubId;
            this.ClubName = pilot.Club.ShortName;
            this.Name = pilot.Name;
            this.Email = pilot.Email;
            this.MemberId = pilot.MemberId;
            this.MobilNumber = pilot.MobilNumber;
            this.UnionId = pilot.UnionId;
            this.InstructorId = pilot.InstructorId;
            this.ExitDate = pilot.ExitDate;

            this.TrainingPrograms = trainingProgramsForPilot.ToList().Select(x => new PilotInTrainingProgramViewModel(x)).ToArray();
        }

        public int PilotId { get; set; }
        public int ClubId { get; set; }
        public string ClubName { get; set; }

        public string Name { get; set; }
        public string Email { get; set; }
        public string MemberId { get; set; }
        public string MobilNumber { get; set; }
        public string UnionId { get; set; }
        public string InstructorId { get; set; }

        public DateTime? ExitDate { get; set; }

        public void Update(Pilot p)
        {
            p.PilotId = PilotId;
            p.ClubId = ClubId;
            p.Name = Name;
            p.Email = Email;
            p.MemberId = MemberId;
            p.MobilNumber = MobilNumber;
            p.UnionId = UnionId;
            p.InstructorId = InstructorId;
            p.ExitDate = ExitDate;
        }

        public PilotInTrainingProgramViewModel[] TrainingPrograms { get; set; }
    }

    public class PilotInTrainingProgramViewModel
    {
        public PilotInTrainingProgramViewModel()
        {

        }
        public PilotInTrainingProgramViewModel(PilotInTrainingProgram pitp)
        {
            PilotId = pitp.Pilot.PilotId;
            ProgramId = pitp.Program.Training2ProgramId;
            ProgramName = pitp.Program.Name;
            StartDate = pitp.StartDate;
            EndDate = pitp.EndDate;
            Completed = pitp.IsCompleted;

        }
        public int PilotId { get; set; }
        public int ProgramId { get; set; }
        public string ProgramName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool Completed { get; set; }

        public void Update(PilotInTrainingProgram pitp)
        {
            pitp.EndDate = EndDate;
            pitp.IsCompleted = Completed;
        }

    }
}