namespace FlightJournal.Web.Models.Training
{
    /// <summary>Single source of truth for who may read or write training-log data.</summary>
    public static class TrainingDataPolicy
    {
        public static bool CanAccess(bool isInstructor, bool isAdministrator)
        {
            return isInstructor || isAdministrator;
        }

        /// <summary>Instructor credited on a training flight; never taken from the client.</summary>
        public static int ResolveInstructorId(Pilot backseatPilot, Pilot currentUserPilot)
        {
            if (backseatPilot != null && backseatPilot.IsInstructor) return backseatPilot.PilotId;
            if (currentUserPilot != null && currentUserPilot.IsInstructor) return currentUserPilot.PilotId;
            return -1;
        }
    }
}
