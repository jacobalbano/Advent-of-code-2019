using AdventOfCode2019.Days;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdventOfCode2019
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            comboBox1.DataSource = Options;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Enabled = button1.Enabled = false;
            backgroundWorker1.RunWorkerAsync();
        }

        public List<Option> Options { get; } = GenerateOptions();
        private static List<Option> GenerateOptions()
        {
            return typeof(DayBase).Assembly
                .GetTypes()
                .Where(typeof(DayBase).IsAssignableFrom)
                .Where(x => !x.IsAbstract)
                .Select(x => (DayBase) Activator.CreateInstance(x))
                .OrderBy(x => Regex.Match(x.Name, @"Day(\d+)").Groups[1].Value)
                .SelectMany(YieldOptions)
                .ToList();

            IEnumerable<Option> YieldOptions(DayBase day)
            {
                yield return new Option { Name = $"{day.Name}.1", Run = day.Part1, Test = day.Part1Test };
                yield return new Option { Name = $"{day.Name}.2", Run = day.Part2, Test = day.Part2Test };
            }
        }

        public class Option
        {
            public string Name { get; set; }
            public Func<string, string> Run { get; set; }
            public Action Test { get; set; }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            string result = null;
            var time = new List<TimeSpan>(10);
            var totalTime = Stopwatch.StartNew();
            for (int i = 0; i < 10 || totalTime.ElapsedMilliseconds < 150; i++)
            {
                var timer = Stopwatch.StartNew();
                var newResult = selectedOption.Run(txtInput.Text);
                time.Add(timer.Elapsed);
                if (result != null && result != newResult)
                    throw new Exception($"Result differed between test runs! Old: {result}, New: {newResult}");

                result = newResult;
            }

            e.Result = Tuple.Create(result, time);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var tpus = TimeSpan.TicksPerMillisecond / 1000;
            var result = (Tuple<string, List<TimeSpan>>)e.Result;
            var output = result.Item1;
            var times = result.Item2.Select(x => x.Ticks / tpus).ToList();
            times.Sort();

            var mean = (int) Math.Floor(times.Sum() / (float)times.Count);
            var median = times[times.Count / 2];
            string meanUnit = "μs", medianUnit = "μs";
            if (mean > 1000) { mean /= 1000; meanUnit = "ms"; }
            if (median > 1000) { median /= 1000; medianUnit = "ms"; }
            
            txtOutput.Text = result.Item1;
            Text = $"AdventRunner - Last run: Reps({times.Count}) ・ Mean({mean}{meanUnit}) ・ Median({median}{medianUnit})";
            comboBox1.Enabled = button1.Enabled = true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedOption = Options[comboBox1.SelectedIndex];
            selectedOption.Test();
        }

        private Option selectedOption;
    }
}
