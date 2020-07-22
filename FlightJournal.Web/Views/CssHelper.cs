using FlightJournal.Web.Models;

namespace FlightJournal.Web.Views
{
    public class CssHelper
    {
        public static string CssClassFor(TrainingStatus status, bool isBriefingOnlyRequired)
        {
            string clz;
            switch (status)
            {
                case TrainingStatus.Briefed:
                    clz = isBriefingOnlyRequired ? "btn-success" : "btn-info";
                    break;
                case TrainingStatus.Trained:
                    clz = "btn-info";
                    break;
                case TrainingStatus.Completed:
                    clz = "btn-success";
                    break;
                default:
                    clz = "btn-default";
                    break;
            }

            return clz;
        }
    }
}