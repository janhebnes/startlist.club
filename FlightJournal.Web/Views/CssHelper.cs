using FlightJournal.Web.Models;

namespace FlightJournal.Web.Views
{
    public class CssHelper
    {
        public static string CssClassFor(TrainingStatus status)
        {
            string clz;
            switch (status)
            {
                case TrainingStatus.NotStarted:
                    clz = "status-not-started";
                    break;
                case TrainingStatus.Briefed:
                    clz = "status-in-progress";
                    break;
                case TrainingStatus.Trained:
                    clz = "status-in-progress";
                    break;
                case TrainingStatus.Completed:
                    clz = "status-completed";
                    break;
                default:
                    clz = "status-not-started";
                    break;
            }

            return clz;
        }
    }
}