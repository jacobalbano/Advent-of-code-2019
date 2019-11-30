using AdventOfCode2019.Days;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
                yield return new Option { Name = $"{day.Name}.1", Run = day.Part1 };
                yield return new Option { Name = $"{day.Name}.2", Run = day.Part2 };
            }
        }

        public class Option
        {
            public string Name { get; set; }
            public Func<string, string> Run { get; set; }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            e.Result = Options[comboBox1.SelectedIndex].Run(txtInput.Text);
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            txtOutput.Text = (string) e.Result;
            comboBox1.Enabled = button1.Enabled = true;
        }
    }
}
