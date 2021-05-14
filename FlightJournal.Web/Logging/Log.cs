using System;

namespace FlightJournal.Web.Logging
{
    public class Log
    {
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        public static void Exception(string msg, Exception e)
        {
            _log.Error(msg, e);
        }
        public static void Error(string msg)
        {
            _log.Error(msg);
        }
        public static void Warning(string msg)
        {
            _log.Warn(msg);
        }
        public static void Debug(string msg)
        {
            _log.Debug(msg);
        }
        public static void Information(string msg)
        {
            _log.Info(msg);
        }
    }
}