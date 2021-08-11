
var scannerChartsDrawnCallback = function() {
    // can be overridden
}


var drawBarChart = function (chart) {
    var fetchedBarLables = chart.data("x");
    var fetchedBarData = chart.data("y");
    var showLegend = chart.data("showlegend");
    var stacked = chart.data("stacked");
    if (fetchedBarLables != null && fetchedBarData != null) {
        var dataseries = [];
        for (var i = 0; i < fetchedBarData.length; i++) {
            var ds = fetchedBarData[i];
            var d = {
                label: unescape(ds.Name),
                backgroundColor: ds.FillColorHex,
                borderColor: ds.StrokeColorHex,
                data: ds.Data,
                fill: ds.UseFill
            };

            dataseries.push(d);
        }

        var chartData = {
            labels: fetchedBarLables,
            datasets: dataseries
        };

        var barChartCanvas = chart.get(0).getContext("2d");

        var barChartData = chartData;
        var barChartOptions = {
            legend: {
                display: showLegend
            },
            scales: {
                xAxes: [{
                    gridLines: {
                        display: false
                    },
                    stacked: stacked
                }],
                yAxes: [{
                    ticks: {
                        beginAtZero: true
                    }
                }]
            }
        };

        new Chart(barChartCanvas, {
            type: "bar",
            data: barChartData,
            options: barChartOptions
        });
    }

};

var drawPieChart = function (chart, showlegend = false, hiddenElements) {
    var pieData = chart.data("y");
    var labels = [];
    var values = [];
    var colors = [];
    for (var index = 0; index < pieData.length; index++) {
        var ds = pieData[index];
        if (hiddenElements && hiddenElements.includes(ds.Name))
            continue;
        labels.push(unescape(ds.Name));
        values.push(ds.Data);
        colors.push(ds.FillColorHex);
    }
    var pieChartCanvas = chart.get(0).getContext("2d");
    var pieDataSeries = {
        labels: labels,
        datasets: [
            {
                data: values,
                backgroundColor: colors
            }
        ]
    };


    var pieOptions = {
        legend: {
            display: showlegend
        },
        animation: {
            easing: "easeOutQuad",
            duration: 300
        }
    };
    new Chart(pieChartCanvas,
        {
            type: "doughnut",
            data: pieDataSeries,
            options: pieOptions

        });
};



var drawLineChart = function (notVisibleNames) {
    $(".lineChart").each(function () {
        var fetchedLineLables = $(this).data("x");
        var fetchedLineData = $(this).data("y");
        if (fetchedLineLables != null && fetchedLineData != null) {
            var linedataseries = [];
            for (var i = 0; i < fetchedLineData.length; i++) {
                var ds = fetchedLineData[i];

                var show = (notVisibleNames == null || $.inArray(ds.Name, notVisibleNames) <= -1);
                var dash = [0, 0];
                if (ds.Dashed) {
                    dash = [5, 5];
                }
                var d = {
                    label: unescape(ds.Name),
                    backgroundColor: ds.FillColorHex,
                    borderColor: ds.StrokeColorHex,
                    data: ds.Data,
                    fill: ds.UseFill,
                    pointRadius: 0,
                    showLine: show,
                    pointHitRadius: 15,
                    borderWidth: 3,
                    borderDash: dash
                };
                linedataseries.push(d);

            }

            var linechartData = {
                labels: fetchedLineLables,
                datasets: linedataseries
            };

            var areaChartOptions = {
                maintainAspectRatio: false,
                legend: {
                    display: false
                },
                scales: {
                    xAxes: [{
                        gridLines: {
                            display: false
                        }
                    }],
                    yAxes: [{
                        ticks: {
                            beginAtZero: true
                        }
                    }]
                }
            };
            var lineChartCanvas = $(this).get(0).getContext("2d");
            var lineChartOptions = areaChartOptions;
            lineChartOptions.datasetFill = false;

            new Chart(lineChartCanvas, {
                type: "line",
                data: linechartData,
                options: lineChartOptions
            });
        }
    });
};

var drawScatterDateCharts = function (
    fromDate,
    toDate,
    onElementClick,
    onChartClick,
    onZoom,
    toolTipValueFormatter,
    legendPositionIfShown,
    optionsModificationCallback) {

    charts = []; // reduce leaks

    $(".scatterChartDate").each(function () {
        var chartDiv = $(this);
        drawScatterDateChart(chartDiv,
            fromDate,
            toDate,
            onElementClick,
            onChartClick,
            onZoom,
            toolTipValueFormatter,
            legendPositionIfShown,
            optionsModificationCallback);
    });

    if (typeof (scannerChartsDrawnCallback) == 'function')
        scannerChartsDrawnCallback();
}

var setDataSeriesVisibility = function(dataSeriesName, visible) {
    charts.forEach(function(item, index) {
        item.data.datasets.forEach(function(ds) {
            if (ds.stack == dataSeriesName)
                ds.hidden = !visible;
        });
        item.update();
    });
}

var charts = [];

var drawScatterDateChart = function (chart,
    fromDate,
    toDate,
    onElementClick,
    onChartClick,
    onZoom,
    toolTipValueFormatter,
    legendPositionIfShown,
    optionsModificationCallback) {
    var chartId = chart.data("chartid");
    var chartDescription = chart.data("description");
    var fetchedLineData = chart.data("dataseries");
    var minY = chart.data("miny");
    var maxY = chart.data("maxy");
    var stepY = chart.data("ystep");

    if (fetchedLineData == null || fetchedLineData.length == 0) {
        return;
    }

    var yTickCallback = null;
    var yLabels = chart.data("ylabelmap");
    if (yLabels) {
        yTickCallback = function (value, index, values) {
            var label = yLabels[value];
            return label ? label:  "?";
        }
        //toolTipValueFormatter = function(v) {
        //    var label = yLabels[v];
        //    return label ? label : "?";
        //}
    } 
    var yLabel = chart.data("ylabel");
    var xLabel = chart.data("xlabel");

    var showYLabel = yLabel != null && yLabel.length > 0;
    var showXLabel = xLabel != null && xLabel.length > 0;

    var lineChartCanvas = chart.get(0).getContext("2d");
    var dataSeries = [];
    var minDate;
    if (fromDate != null)
        minDate = moment(fromDate);
    var maxDate;
    if (toDate != null)
        maxDate = moment(toDate);
    fetchedLineData.forEach(function (ds) {
        var dataValues = [];
        ds.Data.forEach(function (d) {
            var date = moment(d.Timestamp);
            var point = {
                x: date,
                y: d.Value,
                note: d.Note,
                key: d.Key
            }
            dataValues.push(point);

            if (fromDate == null && (minDate == null || date < minDate))
                minDate = date;
            if (toDate == null && (maxDate == null || date > maxDate))
                maxDate = date;
        });
        var dash = [0, 0];
        if (ds.Dashed) {
            dash = [20, 20];
        }
        dataSeries.push({
            stack: ds.Name, // slight abuse
            label: ds.ChartTitle,
            data: dataValues,
            showLine: ds.ShowLine,
            borderDash: dash,
            pointRadius: ds.PointRadius,
            pointStyle: ds.PointStyle,
            tension: 0,
            backgroundColor: ds.FillColorHex,
            borderColor: ds.StrokeColorHex,
            fill: ds.UseFill,
            borderWidth: ds.PointRadius / 2
        });
    });

    var legendPosition = false;
    fetchedLineData.forEach(function (ds) {
        if (ds.ChartTitle != null && ds.ChartTitle.length > 0) {
            if (legendPositionIfShown)
                legendPosition = legendPositionIfShown;
            else
                legendPosition = 'top';
        }
    });


    var ISOfmt = "YYYY-MM-DDTHH:mm:ss.SSSZ";
    var options =
    {
        type: "line",
        
        data: {
            datasets: dataSeries
        },
        options: {
            title: {
                display: false
            },
            animation: false,
            maintainAspectRatio: false,
            tooltips: {
                enabled: true,
                mode: "single",
                callbacks: {
                    label: function(tooltipItems, d) {
                        var note = d.datasets[tooltipItems.datasetIndex].data[tooltipItems.index].note;
                        if (note)
                            return note.split('\n')[0];
                        if (toolTipValueFormatter)
                            return toolTipValueFormatter(tooltipItems.yLabel);
                        return parseFloat(tooltipItems.yLabel).toFixed(2);
                    },
                    afterLabel: function(tooltipItems, d) {
                        var note = d.datasets[tooltipItems.datasetIndex].data[tooltipItems.index].note;
                        if (note)
                            return note.split('\n')[1];
                        return null;
                    },
                    title: function (tooltipItems) {
                        var fmt = tooltipformat;
                        return moment(tooltipItems[0].label).format(fmt);
                    }
                }
            },
            scales: {
                xAxes: [
                    {
                        type: "time",
                        time: {
                            displayFormats: {
                                'day': ISOfmt,
                                'minute': ISOfmt,
                                'hour': ISOfmt,
                                'second': ISOfmt,
                                'millisecond': ISOfmt
                            },
                            tooltipFormat: ISOfmt, // use easily parseable format, we will use the right one in the callback
                            min: minDate,
                            max: maxDate
                        },
                        ticks: {
                            callback: function (value) {
                                var fmt = axisformat_day;
                                return moment(value).format(fmt);
                            }
                        },
                        gridLines: {
                            display: true
                        },
                        scaleLabel: {
                            display: showXLabel,
                            labelString: xLabel
                        }
                    }
                ],
                yAxes: [
                    {
                        ticks: {
                            min: minY,
                            max: maxY,
                            stepSize: stepY,
                            callback: yTickCallback,
                            fontSize: 8
                        },
                        scaleLabel: {
                            display: showYLabel,
                            labelString: yLabel
                        }
                    }
                ]
            },
            legend: {
                display: legendPosition != false,
                position: legendPosition
            },
            onClick: function (event) {
                if (onElementClick != null) {
                    var element = this.getElementAtEvent(event);
                    if (element != null && element.length > 0) {
                        var key = dataSeries[element[0]._datasetIndex].data[element[0]._index].key;
                        onElementClick(key);
                    }
                }
                if (onChartClick != null) {
                    var element = this.getElementAtEvent(event);
                    if (element == null || element.length == 0) {
                        onChartClick(chartId, chartDescription);
                    }
                }
            },
           
            plugins: {
                crosshair: {
                    line: {
                        color: "#00000000", // invisible
                        width: 1
                    },
                    sync: {
                        enabled: false, // enable trace line syncing with other charts (works quite slowly, so disable)
                        group: 1, // chart group
                        suppressTooltips: false // suppress tooltips when showing a synced tracer
                    },
                    zoom: {
                        enabled: true,
                        zoomButtonClass: "btn btn-sm btn-flat text-uppercase margin pull-right"
                    },
                    callbacks: {
                        beforeZoom: function (start, end) {
                            // Resharper disable once CssBrowserCompatibility
                            var syncZoom = $("#syncZoom:checked").length > 0;
                            var from = moment(start);
                            var to = moment(end);

                            if (syncZoom) {
                                if (onZoom != null) {
                                    onZoom(from.format(), to.format()); // we zoom by letting the callback change the time range (affecting all charts), and effectively refresh the search
                                }
                                return false; // block the plugin from zooming by itself
                            } else {
                                return true;
                            }
                        },
                        afterZoom: function () { }
                    }
                }
            }
        }
    };
    if (optionsModificationCallback)
        optionsModificationCallback(options);

    // ReSharper disable once ConstructorCallNotUsed
    charts.push(new Chart(lineChartCanvas, options));
};


var getDateTimeFormat = function (key, defaultFormat) {
    var fmt = $("#chartformats").data(key);
    return (fmt != null && fmt != "") ? convertDateTimeFormatToMomentJs(fmt) : defaultFormat;

};

var setChartFontColorSameAsBody = function () {
    var bodyColor = $("body").css("color");
    Chart.defaults.global.defaultFontColor = bodyColor;
}

var tooltipformat = getDateTimeFormat("tooltip", "YYYY-MM-DD HH:mm:ss");
var axisformat_day = getDateTimeFormat("axis_day", "D MMM ");
var axisformat_hour = getDateTimeFormat("axis_hour", "D MMM HH:mm");
var axisformat_minute = getDateTimeFormat("axis_minute", "D MMM HH:mm:ss");

