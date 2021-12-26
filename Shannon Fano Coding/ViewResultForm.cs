using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ShannonFanoCoding
{
    public partial class ViewResultForm : Form
    {
        public ViewResultForm()
        {
            InitializeComponent();
        }

        Hashtable _msgCodewords;
        List<string> _msgChars;
        int _m;

        public ViewResultForm(IEnumerable<string> msgChars, Hashtable msgProbabilities, Hashtable msgCodewords, int m,
            double hx, double l̅, double η, double totalProbability, int minimumCodewordLength,
            int maximumCodewordLength)
        {
            InitializeComponent();

            M_TextBox.Text = m.ToString();
            PxTextBox.Text = totalProbability.ToString();
            LBarTextBox.Text = l̅.ToString();
            HxTextBox.Text = hx.ToString();
            efficiencyTextBox.Text = η.ToString();
            _msgChars = new List<string>();
            _msgChars.AddRange(msgChars);
            _msgCodewords = msgCodewords;
            _m = m;

            #region "Add required Columns."

            for (var i = 0; i < maximumCodewordLength; i++)
            {
                finalResultDataGridView.Columns.Add("col" + i, " ");
            }

            var lengthCol = new DataGridViewTextBoxColumn
            {
                HeaderText = @"Length",
                DefaultCellStyle = new DataGridViewCellStyle()
            };
            lengthCol.DefaultCellStyle.ForeColor = Color.Red;
            lengthCol.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            finalResultDataGridView.Columns.Add(lengthCol);

            #endregion

            #region "Add required Rows"

            List<string> data;

            var en = msgProbabilities.GetEnumerator();

            while (en.MoveNext())
            {
                data = new List<string>
                {
                    en.Key.ToString(),
                    en.Value.ToString()
                };
                data.AddRange(msgCodewords[en.Key].ToString().Split(','));
                finalResultDataGridView.Rows.Add(data.ToArray());
                finalResultDataGridView.Rows[finalResultDataGridView.Rows.Count - 1].Cells[maximumCodewordLength + 2]
                    .Value = (data.Count - 2).ToString();
            }

            #endregion
        }

        private void decodeButton_Click(object sender, EventArgs e)
        {
            var dcForm = new DecodeForm(_msgCodewords, _msgChars, _m);
            dcForm.Show();
        }
    }
}