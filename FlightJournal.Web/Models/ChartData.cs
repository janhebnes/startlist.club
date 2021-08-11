using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using FlightJournal.Web.Models.Training.Catalogue;
using Microsoft.Ajax.Utilities;

namespace FlightJournal.Web.Models
{
    public class ChartData
    {
        public ChartData(object[] xLabels, DataSerie[] dataSeries)
        {
            DataSeries = dataSeries;
            XLabels = xLabels;
        }

        public ChartData(DataSerie[] dataSerie) : this(new object[] { }, dataSerie)
        {
        }

        public ChartData(SingleSerieChart singleSerieChart) : this(singleSerieChart.Groups.ToArray(), new[] {singleSerieChart.DataSerie})
        {
            
        }

        public ChartData(MultiSerieChart multiSerieChart) : this(multiSerieChart.Groups.ToArray(), multiSerieChart.DataSeries)
        {
            
        }

        public string YLabel { get; set; }
        public string XLabel { get; set; }

        public DataSerie[] DataSeries { get; }
        public object[] XLabels { get; }

        public string Title { get; protected set; }

        public static ChartData Empty => new ChartData(Enumerable.Empty<object>().ToArray(), Enumerable.Empty<DataSerie>().ToArray());
    }

    /// <summary>
    /// Range between to limits where the lower limit is included
    /// </summary>
    public class RangeData // TODO make generic where T is comparable
    {
        public RangeData(int lower, int upper, string color)
        {
            Lower = lower;
            Upper = upper;
            Color = color;
        }

        public string Color { get; set; }

        public int Lower { get; set; }
        public int Upper { get; set; }

        /// <summary>
        /// Above or equal to lower limit and below upper limit.
        /// Note upper limit is not included
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool Contains(int value)
        {
            return value >= Lower && value < Upper;
        }

        public override string ToString()
        {
            return $"[{Lower}..{Upper}[";
        }
    }

    /// <summary>
    /// A data point in a time series intended for display in scatterchart (Charting.js)
    /// </summary>
    public class TimestampedValue
    {
        public DateTime Timestamp { get; set; }
        /// <summary>
        ///  Numerical value
        /// </summary>
        public double Value { get; set; }
        /// <summary>
        /// Shown as tooltip
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// Used for click handler on point click
        /// </summary>
        public string Key { get; set; }
    }
    

    public class TimeDataSerie : DataSerieBase
    {
        public TimeDataSerie(IEnumerable<TimestampedValue> data, string name, string strokeColorHex, string fillColorHex, bool useFill = true, bool showLine = true, string title = "") : base(name, strokeColorHex, fillColorHex, useFill, showLine)
        {
            Data = data;
            ChartTitle = title;
        }
        public TimeDataSerie(IEnumerable<TimestampedValue> data, string name, Color strokeColor, Color fillColor, bool useFill = true, bool showLine = true) : this(data, name, ColorTranslator.ToHtml(Color.FromArgb(strokeColor.ToArgb())), ColorTranslator.ToHtml(Color.FromArgb(fillColor.ToArgb())), useFill, showLine)
        { }

        public TimeDataSerie(IEnumerable<TimestampedValue> data, string name, string hexStrokeColor, bool showLine = true, string title = "") : this(data, name, hexStrokeColor, hexStrokeColor, false, showLine, title)
        {
        }

        public IEnumerable<TimestampedValue> Data { get; }
        public string ChartTitle { get; }
    }

    
    public class DataSerie : DataSerieBase
    {
        public DataSerie(IEnumerable<double> data, string name, string strokeColorHex, string fillColorHex, bool useFill = true) : base(name, strokeColorHex, fillColorHex, useFill)
        {
            Data = data;
        }
        public DataSerie(IEnumerable<double> data, string name, Color strokeColor, Color fillColor, bool useFill = true) : this(data, name, ColorTranslator.ToHtml(Color.FromArgb(strokeColor.ToArgb())), ColorTranslator.ToHtml(Color.FromArgb(fillColor.ToArgb())), useFill)
        {}

        public DataSerie(IEnumerable<double> data, string name, string hexStrokeColor) : this(data, name, hexStrokeColor, hexStrokeColor, false)
        { }
        public IEnumerable<double> Data { get; }
       
    }

    public class DataSerieBase
    {
        public DataSerieBase(string name, string strokeColorHex, string fillColorHex, bool useFill = true, bool showLine = true)
        {
            Name = string.IsNullOrEmpty(name) ? "-" : name.Replace(" ", "%20") ; 
            FillColorHex = fillColorHex;
            UseFill = useFill;
            ShowLine = showLine;
            StrokeColorHex = strokeColorHex;
            Dashed = false;
        }

        public DataSerieBase(string name, Color strokeColor, Color fillColor, bool useFill = true) : this(name, ColorTranslator.ToHtml(Color.FromArgb(strokeColor.ToArgb())), ColorTranslator.ToHtml(Color.FromArgb(fillColor.ToArgb())), useFill)
        { }

        public DataSerieBase(string name, Color strokeColor) : this(name, strokeColor, strokeColor, false)
        { }

        public DataSerieBase(string name, string hexStrokeColor) : this(name, hexStrokeColor, hexStrokeColor, false)
        { }

        public string Name { get; }

        public string FillColorHex { get; }
        public string StrokeColorHex { get; }
        public bool UseFill { get; }
        
        public bool ShowLine { get; }
        public bool Dashed { get; set;  }
    }

    public class SingleSerieChart
    {
        public SingleSerieChart(IEnumerable<object> groups, DataSerie dataSerie)
        {
            Groups = groups;
            DataSerie = dataSerie;
        }

        public IEnumerable<object> Groups { get; }
        public DataSerie DataSerie { get; }
    }

    public class MultiSerieChart
    {
        public MultiSerieChart(IEnumerable<object> groups, DataSerie[] dataSeries)
        {
            Groups = groups;
            DataSeries = dataSeries;
        }

        public IEnumerable<object> Groups { get; }
        public DataSerie[] DataSeries { get; }
    }

    

    public class TimestampedDataSeriesViewModel
    {
        public TimestampedDataSeriesViewModel(TimeDataSerie source)
        {
            ChartTitle = source.ChartTitle;
            Name = source.Name;
            FillColorHex = source.FillColorHex;
            StrokeColorHex = source.StrokeColorHex;
            UseFill = source.UseFill;
            ShowLine = source.ShowLine;
            Dashed = source.Dashed;
            Data = source.Data;
        }
        
        public IEnumerable<TimestampedValue> Data { get; }
        public string ChartTitle { get; }
        
        public string Name { get; }

        public string FillColorHex { get; }
        public string StrokeColorHex { get; }
        public bool UseFill { get; }
        
        public bool ShowLine { get; }
        public bool Dashed { get; set;  }
        public int PointRadius { get; set; } = 2;
        public string PointStyle { get; set; }

        public Dictionary<string, string> Metadata;
    }


    public interface IPartialExerciseToNumberMapper
    {
        int PartialExerciseToNumber(Training2Exercise ex);
        int PartialExerciseToNumber(int exerciseId);
    }

    /// <summary>
    /// Maps between Exercises (code: Lesson) and numbers
    /// </summary>
    public class CoarseExerciseToNumberMapper : IPartialExerciseToNumberMapper
    {
        private readonly Training2Lesson[] exercises;
        private readonly IEnumerable<Training2Exercise> partialExercises;

        public CoarseExerciseToNumberMapper(IEnumerable<Training2Exercise> catalogue)
        {
            exercises = catalogue
                .Select(x => x.Lessons.FirstOrDefault())
                .DistinctBy(x => x.Training2LessonId)
                .OrderBy(x => x.DisplayOrder)
                .ToArray();
            partialExercises = catalogue;

            var d = new Dictionary<string, string>();
            foreach (var x in exercises.SelectMany(ex=>ex.Exercises))
            {
                var k = PartialExerciseToNumber(x).ToString();
                if (!d.ContainsKey(k))
                    d.Add(k, x.Lessons.First().Name);
            }

            Labels = d;
        }

        public Dictionary<string, string> Labels { get; }

        public int PartialExerciseToNumber(Training2Exercise ex)
        {
            var lessonId = ex.Lessons.FirstOrDefault()?.Training2LessonId;
            for (var i = 0; i < exercises.Length; i++)
            {
                if (exercises[i].Training2LessonId == lessonId)
                    return i;
            }

            return -1;

        }

        public int PartialExerciseToNumber(int partialExerciseId)
        {
            var ex= partialExercises.FirstOrDefault(y => y.Training2ExerciseId == partialExerciseId);
            return ex == null ? -1 : PartialExerciseToNumber(ex);
        }
    }

}