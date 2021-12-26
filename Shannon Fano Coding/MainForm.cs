using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace ShannonFanoCoding
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        Hashtable _msgProbabilities;
        Hashtable _msgCodewords;
        int _m;

        double _hx;
        double _l̅;
        double _η;
        double _totalProbability;
        int _minimumCodewordLength;
        int _maximumCodewordLength;

        private void MainForm_Load(object sender, EventArgs e)
        {
            inputDataGridView.Rows.Clear();
        }

        private void encodeButton_Click(object sender, EventArgs e)
        {
            if (inputDataGridView.Rows.Count <= 2)
            {
                MessageBox.Show(@"Enter at least two messages.", @"Error!", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return;
            }

            encodeButton.Enabled = false;
            encodeButton.Text = @"Wait...";

            var msgChars = new List<string>();
            var msgProbs = new List<string>();

            foreach (DataGridViewRow row in inputDataGridView.Rows)
            {
                if (!row.IsNewRow)
                {
                    msgChars.Add(row.Cells[0].Value.ToString());
                    msgProbs.Add(row.Cells[1].Value.ToString());
                }
            }

            _m = (int) M_NumericUpDown.Value;

            ShannonFano.Encode(msgChars, msgProbs, _m, out _msgProbabilities, out _msgCodewords, out _totalProbability,
                out _hx, out _l̅, out _η, out _minimumCodewordLength, out _maximumCodewordLength);

            encodeButton.Enabled = true;
            encodeButton.Text = @">> Encode >>";

            WindowState = FormWindowState.Minimized;
            var viewResult = new ViewResultForm(msgChars, _msgProbabilities, _msgCodewords, _m, _hx, _l̅, _η,
                _totalProbability, _minimumCodewordLength, _maximumCodewordLength);
            viewResult.Show();
            viewResult.WindowState = FormWindowState.Normal;
        }

        private void inputDataGridView_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FileDrop"))
            {
                var fileName = ((string[]) e.Data.GetData("FileName"))[0].ToLower();
                if (fileName.EndsWith(".txt"))
                {
                    e.Effect = DragDropEffects.Copy;
                }
            }
            else if (e.Data.GetDataPresent("System.String[]"))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private void inputDataGridView_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("FileDrop"))
            {
                try
                {
                    var data = File.ReadAllLines(((string[]) e.Data.GetData("FileName"))[0]);
                    inputDataGridView.Rows.Clear();

                    foreach (var dataLine in data)
                    {
                        if (dataLine.StartsWith("NL"))
                        {
                            inputDataGridView.Rows.Add(new object[] {"NL", dataLine.Substring(2)});
                        }
                        else
                        {
                            inputDataGridView.Rows.Add(new object[] {dataLine[0], dataLine.Substring(1)});
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
            else if (e.Data.GetDataPresent("System.String[]"))
            {
                try
                {
                    var data = ((string[]) e.Data.GetData("System.String[]"));
                    inputDataGridView.Rows.Clear();

                    foreach (var dataLine in data)
                    {
                        if (dataLine.StartsWith("NL"))
                        {
                            inputDataGridView.Rows.Add(new object[] {"NL", dataLine.Substring(2)});
                        }
                        else
                        {
                            inputDataGridView.Rows.Add(new object[] {dataLine[0], dataLine.Substring(1)});
                        }
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        private void loadFromButton_Click(object sender, EventArgs e)
        {
            if (openProbabilityFileOpenFileDialog.ShowDialog() == DialogResult.OK)
            {
                var data = File.ReadAllLines(openProbabilityFileOpenFileDialog.FileName);
                inputDataGridView.Rows.Clear();

                foreach (var dataLine in data)
                {
                    if (dataLine.StartsWith("NL"))
                    {
                        inputDataGridView.Rows.Add(new object[] {"NL", dataLine.Substring(2)});
                    }
                    else
                    {
                        inputDataGridView.Rows.Add(new object[] {dataLine[0], dataLine.Substring(1)});
                    }
                }
            }
        }
    }
}