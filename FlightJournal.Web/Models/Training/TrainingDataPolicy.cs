namespace FlightJournal.Web.Models.Training
{
    /// <summary>Single source of truth for who may read or write training-log data.</summary>
    public static class TrainingDataPolicy
    {
        public static bool CanAccess(bool isInstructor, bool isAdministrator)
        {
            return isInstructor || isAdministrator;
        }

        /// <summary>
        /// Instructor credited on a training flight; resolved server-side only, never taken from the client.
        /// The signed-in pilot if they are an instructor; otherwise the back-seat pilot if they are an
        /// instructor (e.g. an administrator correcting the record); otherwise -1 (no instructor).
        /// </summary>
        public static int ResolveInstructorId(Pilot backseatPilot, Pilot currentUserPilot)
        {
            if (currentUserPilot != null && currentUserPilot.IsInstructor) return currentUserPilot.PilotId;
            if (backseatPilot != null && backseatPilot.IsInstructor) return backseatPilot.PilotId;
            return -1;
        }
    }
}
