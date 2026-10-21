using System;
using Skyhop.FlightAnalysis;
using Skyhop.FlightAnalysis.Models;

namespace FlightJournal.Web.Aprs
{
    /// <summary>
    /// The flight-analysis library as seen by <see cref="FlightAnalysisWorker"/>, its only caller.
    /// Implementations are not thread-safe; the worker calls them from a single thread.
    /// </summary>
    public interface IFlightAnalysisEngine
    {
        event EventHandler<OnTakeoffEventArgs> OnTakeoff;
        event EventHandler<OnLandingEventArgs> OnLanding;
        event EventHandler<OnRadarContactEventArgs> OnRadarContact;
        event EventHandler<OnContextDisposedEventArgs> OnContextDispose;

        void Enqueue(PositionUpdate update);
        void Detach(string aircraft);
    }

    /// <summary>Adapter over the Skyhop <see cref="FlightContextFactory"/>.</summary>
    public sealed class SkyhopFlightAnalysisEngine : IFlightAnalysisEngine
    {
        // The library's own expiry timer runs on the thread pool and races with Enqueue on its spatial index.
        // An expiration far beyond any real flight means the timer never selects (or touches) anything;
        // the worker expires idle aircraft itself on its single thread.
        private static readonly TimeSpan LibraryContextExpiration = TimeSpan.FromDays(3650);

        private readonly FlightContextFactory _factory =
            new FlightContextFactory(new Options(LibraryContextExpiration, minifyMemoryPressure: true));

        public event EventHandler<OnTakeoffEventArgs> OnTakeoff
        {
            add => _factory.OnTakeoff += value;
            remove => _factory.OnTakeoff -= value;
        }

        public event EventHandler<OnLandingEventArgs> OnLanding
        {
            add => _factory.OnLanding += value;
            remove => _factory.OnLanding -= value;
        }

        public event EventHandler<OnRadarContactEventArgs> OnRadarContact
        {
            add => _factory.OnRadarContact += value;
            remove => _factory.OnRadarContact -= value;
        }

        public event EventHandler<OnContextDisposedEventArgs> OnContextDispose
        {
            add => _factory.OnContextDispose += value;
            remove => _factory.OnContextDispose -= value;
        }

        public void Enqueue(PositionUpdate update)
        {
            _factory.Enqueue(update);
        }

        public void Detach(string aircraft)
        {
            _factory.Detach(aircraft);
        }
    }
}
