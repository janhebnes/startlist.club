using System;
using System.Collections.Generic;
using System.Drawing;
using FlightJournal.Web.Models.Training.Catalogue;

namespace FlightJournal.Web.Models.Training
{
    /// <summary>
    /// Represents training status (and history) on a time line.
    ///
    /// Intended for display as a scatter chart, with time on X, and (partial) exercises on y (mapped to some numerical value).
    ///
    /// Each Grading is represented as a time series to encode status as a color.
    ///
    /// Net effect will be a plot of 'dots' for each flight with a (partial) exercise over time, with the color of each dot representing the grading.
    ///
    /// Data structures are optimized for use with Chart.js
    /// </summary>
    public class TrainingTimelineViewModel
    {
        public ScatterChartDataViewModel Data { get; set; }
    }
}