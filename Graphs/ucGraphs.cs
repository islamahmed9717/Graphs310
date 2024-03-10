

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using CheckBox = System.Windows.Forms.CheckBox;
using GroupBox = System.Windows.Forms.GroupBox;
using Timer = System.Windows.Forms.Timer;

namespace ThrusterTest.UserControls
{
    public partial class ucGraphs : UserControl
    {

        private Random random = new Random();
        private Timer timer = new Timer();
        private ConcurrentDictionary<string, ConcurrentDictionary<string, Series>> chartSeriesDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<string, Series>>();
        private int globalMouseX;

        public ucGraphs()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            InitializeCharts();
            //PopulateInitialData();
            //InitializeAllCharts();
            InitializeVerticalLine();
            InizializeTimer();
            CreateChartColors();
            accessDic();



        }


        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            int middleY = panel1.Height / 2;
            int lineLength = panel1.Width; // Set the length of the dashed lines to match the width of the panel
            int gap = 20; // Set the gap between the two lines

            // Calculate the Y positions for the two dashed lines
            int topLineY = middleY - gap / 2;
            int bottomLineY = middleY + gap / 2;

            // Draw dashed lines at the middle of the Panel
            using (Pen pen = new Pen(Color.Black))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                // Draw the top line
                e.Graphics.DrawLine(pen, new Point(0, topLineY), new Point(lineLength, topLineY));

                // Draw the bottom line
                e.Graphics.DrawLine(pen, new Point(0, bottomLineY), new Point(lineLength, bottomLineY));
            }
        }


        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            int middleY = panel2.Height / 2;
            int lineLength = panel2.Width; // Set the length of the dashed lines to match the width of the panel
            int gap = 20; // Set the gap between the two lines

            // Calculate the Y positions for the two dashed lines
            int topLineY = middleY - gap / 2;
            int bottomLineY = middleY + gap / 2;

            // Draw dashed lines at the middle of the Panel
            using (Pen pen = new Pen(Color.Black))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

                // Draw the top line
                e.Graphics.DrawLine(pen, new Point(0, topLineY), new Point(lineLength, topLineY));

                // Draw the bottom line
                e.Graphics.DrawLine(pen, new Point(0, bottomLineY), new Point(lineLength, bottomLineY));
            }
        }


        private void InitializeCharts()
        {
            chartSeriesDictionary.TryAdd("chart1", new ConcurrentDictionary<string, Series>());
            chartSeriesDictionary.TryAdd("chart2", new ConcurrentDictionary<string, Series>());
            chartSeriesDictionary.TryAdd("chart3", new ConcurrentDictionary<string, Series>());
            chartSeriesDictionary.TryAdd("chart4", new ConcurrentDictionary<string, Series>());


            AddSeries("chart1", new string[] { "T1", "T2", "T3", "T4" });
            AddSeries("chart2", new string[] { "A1", "A2", "A3", "A4" });
            AddSeries("chart3", new string[] { "Actual", "Desired" });
            AddSeries("chart4", new string[] { "Actual1", "Desired1" });


            SetAxisRanges("chart1", -100, 100); // Y1 range for chart1
            SetAxisRanges("chart2", 0, 360);    // Y2 range for chart2
            SetAutoScale("chart3");
            SetAutoScale("chart4");


            chart1.MouseDown += Chart_MouseDown;
            chart2.MouseDown += Chart_MouseDown;
            chart3.MouseDown += Chart_MouseDown;
            chart4.MouseDown += Chart_MouseDown;

        }

        private void Chart_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) // Check if the left mouse button was clicked
            {
                Chart clickedChart = sender as Chart;
                if (clickedChart != null)
                {
                    // Convert the mouse position to a point in the chart's coordinates
                    HitTestResult result = clickedChart.HitTest(e.X, e.Y);

                    if (result.ChartElementType == ChartElementType.DataPoint)
                    {
                        // Get the series and data point index
                        Series series = result.Series;
                        int pointIndex = result.PointIndex;

                        // Get the X value of the clicked point
                        double xValue = series.Points[pointIndex].XValue;

                        // Build the tooltip text by finding corresponding points on all series in all charts
                        StringBuilder tooltipText = new StringBuilder();

                        foreach (var chartEntry in chartSeriesDictionary)
                        {
                            Chart chart = Controls.Find(chartEntry.Key, true).FirstOrDefault() as Chart;
                            if (chart != null)
                            {
                                tooltipText.AppendLine($"{chart.Name}:");

                                foreach (var seriesEntry in chartEntry.Value)
                                {
                                    Series s = seriesEntry.Value;

                                    // Find the point in 's' that has the same X value as the clicked point
                                    DataPoint closestPoint = s.Points.OrderBy(p => Math.Abs(p.XValue - xValue)).First();

                                    tooltipText.AppendLine($"  {s.Name}: X={closestPoint.XValue}, Y={closestPoint.YValues[0]}");
                                }
                            }
                        }

                        // Show tooltip
                        ToolTip tooltip = new ToolTip();
                        tooltip.Show(tooltipText.ToString(), clickedChart, e.Location.X, e.Location.Y - 15, 2000); // Adjust as needed
                    }
                }
            }
        }

        private Panel Verticalline;
        private Panel Verticalline1;
        private Panel Verticalline2;
        private Panel Verticalline3;
        private void InitializeVerticalLine()
        {
            InitializeLineForChart(chart1, ref Verticalline);
            InitializeLineForChart(chart2, ref Verticalline1);
            InitializeLineForChart(chart3, ref Verticalline2);
            InitializeLineForChart(chart4, ref Verticalline3);

            chart1.MouseMove += Chart_MouseMove;
            chart2.MouseMove += Chart_MouseMove;
            chart3.MouseMove += Chart_MouseMove;
            chart4.MouseMove += Chart_MouseMove;
        }

        private void InitializeLineForChart(Chart chart, ref Panel line)
        {
            line = new Panel
            {
                Width = 2,
                Height = (int)GetPlotAreaRectangle(chart).Height, // Set initial height based on plot area
                BackColor = Color.Red,
                Location = new Point(0, 0) // Initial position
            };

            chart.Controls.Add(line); // Add line to chart
        }
        private void Chart_MouseMove(object sender, MouseEventArgs e)
        {
            Chart chart = sender as Chart;

            if (chart != null)
            {
                // Convert mouse position to chart area position
                Point chartAreaPosition = e.Location;

                // Calculate the plot area bounds
                RectangleF plotAreaRect = GetPlotAreaRectangle(chart);

                if (plotAreaRect.Contains(chartAreaPosition))
                {
                    // Adjust the mouse X position to be within the plot area bounds
                    globalMouseX = (int)(chartAreaPosition.X - plotAreaRect.X);
                    UpdateVerticalLinesInAllCharts();
                }
            }
        }

        private void UpdateVerticalLinesInAllCharts()
        {
            // Update the vertical lines for all charts based on the global mouse X position
            UpdateVerticalLinePosition(chart1, globalMouseX);
            UpdateVerticalLinePosition(chart2, globalMouseX);
            UpdateVerticalLinePosition(chart3, globalMouseX);
            UpdateVerticalLinePosition(chart4, globalMouseX);
        }
        private void UpdateVerticalLinePosition(Chart chart, int mouseX)
        {
            Panel verticalLine = FindVerticalLineForChart(chart);
            if (verticalLine == null) return;

            // Calculate the plot area's bounds
            RectangleF plotAreaRect = GetPlotAreaRectangle(chart);

            // Ensure the vertical line's X position is within the plot area bounds
            int lineX = Math.Max(0, Math.Min((int)plotAreaRect.Width, mouseX));

            // Update the vertical line's position and make it visible within the plot area
            verticalLine.Location = new Point((int)plotAreaRect.X + lineX - (verticalLine.Width / 2), (int)plotAreaRect.Y);
            verticalLine.Height = (int)plotAreaRect.Height;
            verticalLine.Visible = true;
        }

        private RectangleF GetPlotAreaRectangle(Chart chart)
        {
            ChartArea chartArea = chart.ChartAreas[0];
            RectangleF chartAreaRect = chart.ClientRectangle;

            float plotX = chartAreaRect.Left + (chartAreaRect.Width * chartArea.InnerPlotPosition.X / 100f);
            float plotY = chartAreaRect.Top + (chartAreaRect.Height * chartArea.InnerPlotPosition.Y / 100f);
            float plotWidth = chartAreaRect.Width * chartArea.InnerPlotPosition.Width / 100f;
            float plotHeight = chartAreaRect.Height * chartArea.InnerPlotPosition.Height / 100f;

            return new RectangleF(plotX, plotY, plotWidth, plotHeight);
        }

        private Panel FindVerticalLineForChart(Chart chart)
        {
            // Assuming you have a method to retrieve the correct vertical line Panel for each chart
            // This could be done using a switch statement, dictionary, or any suitable method based on your setup
            switch (chart.Name)
            {
                case "chart1":
                    return Verticalline;
                case "chart2":
                    return Verticalline1;
                case "chart3":
                    return Verticalline2;
                case "chart4":
                    return Verticalline3;
                default:
                    return null;
            }
        }



        private void AddSeries(string chartName, string[] seriesNames)
        {
            var chart = Controls.Find(chartName, true).FirstOrDefault() as Chart;

            if (chart != null)
            {
                foreach (var seriesName in seriesNames)
                {
                    Series series = new Series(seriesName); // Set series name here
                    series.ChartType = SeriesChartType.Line;
                    series.BorderWidth = 4;
                    series.XValueType = ChartValueType.Int32;
                    series.YValueType = ChartValueType.Int32;
                    // Add circular markers at each data point
                    series.MarkerStyle = MarkerStyle.Circle;
                    series.MarkerSize = 2;
                    series.MarkerColor = Color.Black;


                    chart.Series.Add(series);
                    chartSeriesDictionary[chartName].TryAdd(seriesName, series);
                }
            }
            chart.Legends.Clear();
        }


        private void SetAxisRanges(string chartName, double minY, double maxY)
        {
            var chart = Controls.Find(chartName, true).FirstOrDefault() as Chart;

            if (chart != null)
            {
                // Check which chart is being configured and set the Y-axis limits accordingly
                if (chartName == "chart1")
                {
                    // Y1 axis
                    chart.ChartAreas[0].AxisY.Minimum = minY;
                    chart.ChartAreas[0].AxisY.Maximum = maxY;
                }
                else if (chartName == "chart2")
                {
                    // Y2 axis
                    chart.ChartAreas[0].AxisY.Minimum = minY;
                    chart.ChartAreas[0].AxisY.Maximum = maxY;
                }
            }
            else
            {
                Console.WriteLine($"Chart '{chartName}' not found.");
            }
        }


        private void SetAutoScale(string chartName)
        {
            var chart = Controls.Find(chartName, true).FirstOrDefault() as Chart;

            if (chart != null)
            {
                // Enable autoscaling for X axis
                foreach (var axis in chart.ChartAreas[0].Axes)
                {
                    axis.Minimum = double.NaN;
                    axis.Maximum = double.NaN;
                }

                // Check if the chart is either chart1 or chart2, then enable autoscaling for Y axis
                if (chartName == "chart1" || chartName == "chart2")
                {
                    chart.ChartAreas[0].AxisY.Minimum = double.NaN;
                    chart.ChartAreas[0].AxisY.Maximum = double.NaN;
                }
            }
            else
            {
                Console.WriteLine($"Chart '{chartName}' not found.");
            }
        }



        private void InizializeTimer()
        {
            timer.Interval = 1000;
            timer.Tick += Timer_Tick;
            timer.Start();
        }


        private void Timer_Tick(object? sender, EventArgs e)
        {
            double ranvalue = 10;

            foreach (var chartName in chartSeriesDictionary.Keys)
            {
                var seriesDictionary = chartSeriesDictionary[chartName];
                foreach (var seriesName in seriesDictionary.Keys)
                {
                    double newValue;
                    if (chartName == "chart3" || chartName == "chart4")
                    {
                        // For chart3 and chart4, generate random numbers within a reasonable range for Decimal type
                        newValue = random.NextDouble() * ranvalue;
                    }
                    else if (chartName == "chart1")
                    {
                        newValue = random.Next(-100, 100);
                    }
                    else
                    {
                        // For other charts, generate random numbers within a range
                        newValue = random.Next(0, 360);
                    }

                    double xInterval = 10;
                    UpdateChart(chartName, seriesName, newValue, xInterval);
                }
                ranvalue++;
            }

        }


        private void UpdateChart(string chartName, string seriesName, double newValue, double xInterval)
        {
            var series = chartSeriesDictionary[chartName][seriesName];

            double maxX = series.Points.Count > 0 ? series.Points.Max(p => p.XValue) : 0;

            double newX = maxX + xInterval;

            series.Points.AddXY(newX + 1, newValue);
        }

        private void CreateChartColors()
        {
            Dictionary<string, Dictionary<string, Color[]>> chartColors = new Dictionary<string, Dictionary<string, Color[]>>()
    {
        {"chart1", new Dictionary<string, Color[]>
            {
                {"T1", new Color[] { Color.Green }},
                {"T2", new Color[] { Color.Red }},
                {"T3", new Color[] { Color.Blue }},
                {"T4", new Color[] { Color.LightGreen }}
            }
        },
        {"chart2", new Dictionary<string, Color[]>
            {
                {"A1", new Color[] { Color.Blue }},
                {"A2", new Color[] { Color.Red }},
                {"A3", new Color[] { Color.Green }},
                {"A4", new Color[] { Color.LightGreen }}
            }
        },
        {"chart3", new Dictionary<string, Color[]>
            {
                {"Actual", new Color[] { Color.Blue }},
                {"Desired", new Color[] { Color.Red }}
            }
        },
        {"chart4", new Dictionary<string, Color[]>
            {
                {"Actual1", new Color[] { Color.Blue }},
                {"Desired1", new Color[] { Color.Red }}
            }
        }
    };

            foreach (var chartName in chartSeriesDictionary.Keys)
            {
                Console.WriteLine($"Processing chart: {chartName}");

                var chart = Controls.Find(chartName, true).FirstOrDefault() as Chart;
                if (chart != null && chartColors.ContainsKey(chartName))
                {
                    var seriesDictionary = chartSeriesDictionary[chartName];
                    var colors = chartColors[chartName];
                    foreach (var seriesName in seriesDictionary.Keys)
                    {
                        Console.WriteLine($"  Processing series: {seriesName}");
                        if (colors.ContainsKey(seriesName))
                        {
                            var colorArray = colors[seriesName];
                            var series = seriesDictionary[seriesName];

                            // Ensure colorArray has sufficient length
                            if (series.Points.Count <= colorArray.Length)
                            {
                                // Assign series color
                                series.Color = colorArray[0];
                                Console.WriteLine($"      Assigned color: {colorArray[0]}");
                            }
                            else
                            {
                                Console.WriteLine($"  Warning: Insufficient colors for series '{seriesName}' in chart '{chartName}'.");
                                // Assign default color to the series
                                series.Color = Color.Black;
                                Console.WriteLine($"      Assigned default color: Black");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"  Warning: No colors defined for series '{seriesName}' in chart '{chartName}'.");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"Chart '{chartName}' not found or no colors defined.");
                }
            }
        }



        private void chkT4_CheckedChanged(object sender, EventArgs e)
        {
            var series = chart1.Series.FirstOrDefault(s => s.Name == "T4");
            if (series != null)
            {

                series.Enabled = chkT4.Checked;
            }
            else
            {

                Console.WriteLine("Series 'T4' not found in the SeriesCollection of chart1.");
            }

        }

        private void chkT2_CheckedChanged(object sender, EventArgs e)
        {
            var series = chart1.Series.FirstOrDefault(s => s.Name == "T2");
            if (series != null)
            {

                series.Enabled = chkT2.Checked;
            }
            else
            {

                Console.WriteLine("Series 'T2' not found in the SeriesCollection of chart1.");
            }
        }

        private void chkT3_CheckedChanged(object sender, EventArgs e)
        {
            var series = chart1.Series.FirstOrDefault(s => s.Name == "T3");
            if (series != null)
            {
                series.Enabled = chkT3.Checked;
            }
            else
            {

                Console.WriteLine("Series 'T3' not found in the SeriesCollection of chart1.");
            }
        }

        private void chkT1_CheckedChanged(object sender, EventArgs e)
        {
            var series = chart1.Series.FirstOrDefault(s => s.Name == "T1");
            if (series != null)
            {

                series.Enabled = chkT1.Checked;
            }

            else
            {

                Console.WriteLine("Series 'T1' not found in the SeriesCollection of chart1.");
            }

        }

        private void chkA4_CheckedChanged(object sender, EventArgs e)
        {
            var series = chart2.Series.FirstOrDefault(s => s.Name == "A4");
            if (series != null)
            {
                // Enable or disable the series based on the checked state of chkT4
                series.Enabled = chkA4.Checked;
            }

            else
            {
                // The series with the name "T4" does not exist in the SeriesCollection
                Console.WriteLine("Series 'A4' not found in the SeriesCollection of chart2.");
            }
        }

        private void chkA2_CheckedChanged(object sender, EventArgs e)
        {
            var series = chart2.Series.FirstOrDefault(s => s.Name == "A2");
            if (series != null)
            {
                series.Enabled = chkA2.Checked;
            }

            else
            {
                Console.WriteLine("Series 'A2' not found in the SeriesCollection of chart2.");
            }
        }

        private void chkA3_CheckedChanged(object sender, EventArgs e)
        {
            var series = chart2.Series.FirstOrDefault(s => s.Name == "A3");
            if (series != null)
            {
                series.Enabled = chkA3.Checked;
            }

            else
            {
                Console.WriteLine("Series 'A3' not found in the SeriesCollection of chart2.");
            }
        }

        private void chkA1_CheckedChanged(object sender, EventArgs e)
        {
            var series = chart2.Series.FirstOrDefault(s => s.Name == "A1");
            if (series != null)
            {
                series.Enabled = chkA1.Checked;
            }

            else
            {
                Console.WriteLine("Series 'A1' not found in the SeriesCollection of chart2.");
            }
        }


        private void chkActual_CheckedChanged(object sender, EventArgs e)
        {
            var series = chart3.Series.FirstOrDefault(s => s.Name == "Actual");
            if (series != null)
            {
                series.Enabled = chkActual.Checked;
            }

            else
            {
                Console.WriteLine("Series 'Actual' not found in the SeriesCollection of chart3.");
            }
        }

        private void chkDesired_CheckedChanged(object sender, EventArgs e)
        {
            var series = chart3.Series.FirstOrDefault(s => s.Name == "Desired");
            if (series != null)
            {
                series.Enabled = chkDesired.Checked;
            }
            else { Console.WriteLine("Series 'Desired' not found in the SeriesCollection of chart3."); }

        }

        private void chkActual1_CheckedChanged(object sender, EventArgs e)
        {
            var series = chart4.Series.FirstOrDefault(s => s.Name == "Actual1");
            if (series != null)
            {
                series.Enabled = chkActual1.Checked;
            }

            else
            {
                Console.WriteLine("Series 'Actual1' not found in the SeriesCollection of chart4.");
            }
        }

        private void chkDesired1_CheckedChanged(object sender, EventArgs e)
        {
            var series = chart4.Series.FirstOrDefault(s => s.Name == "Desired1");
            if (series != null)
            {
                series.Enabled = chkDesired1.Checked;
            }

            else
            {
                Console.WriteLine("Series 'Desired1' not found in the SeriesCollection of chart4.");
            }
        }


        private void accessDic()
        {
            string cal = "chart1";

            if (chartSeriesDictionary.ContainsKey(cal))
            {
                var innerDic = chartSeriesDictionary[cal];

                foreach (var seriesname in innerDic.Keys)
                {

                    Console.WriteLine($"the series name is = {seriesname}");

                    var series = innerDic[seriesname];
                }
            }

            else
            {
                Console.WriteLine($"chart '{cal}' not found in chart series dictionary");
            }
        }

    }
}


//private bool IsCheckboxChecked(string seriesName)
//{
//    System.Windows.Forms.CheckBox checkBox = FindCheckBox(seriesName, Controls);

//    if (checkBox != null && chartSeriesDictionary.ContainsKey(checkBox.Parent.Name) && chartSeriesDictionary[checkBox.Parent.Name] != null && chartSeriesDictionary[checkBox.Parent.Name].ContainsKey(seriesName))
//    {
//        var series = chartSeriesDictionary[checkBox.Parent.Name][seriesName];

//        if (checkBox.Checked)
//        {
//            series.Enabled = true;
//        }
//        else
//        {
//            series.Enabled = false;
//        }

//        return checkBox.Checked && series.Enabled;
//    }
//    else
//    {
//        // Handle the case where the object is not set
//        return false;
//    }
//}

//private System.Windows.Forms.CheckBox FindCheckBox(string seriesName, ControlCollection controls)
//{
//    foreach (Control control in controls)
//    {
//        if (control is GroupBox groupBox)
//        {
//            CheckBox checkBox = FindCheckBox(seriesName, groupBox.Controls);
//            if (checkBox != null)
//                return checkBox;
//        }
//        else if (control is CheckBox checkBox && checkBox.Name == seriesName)
//        {
//            return checkBox;
//        }
//    }
//    return null;
//}