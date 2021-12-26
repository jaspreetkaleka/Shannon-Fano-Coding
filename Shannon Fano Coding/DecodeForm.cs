using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ShannonFanoCoding
{
    public partial class DecodeForm : Form
    {
        public DecodeForm()
        {
            InitializeComponent();
        }

        Hashtable _msgCodewords;
        List<string> _msgChars;
        int _m;

        public DecodeForm(Hashtable msgCodewords, IEnumerable<string> msgChars, int m)
        {
            InitializeComponent();

            _msgCodewords = msgCodewords;

            _msgChars = new List<string>();
            _msgChars.AddRange(msgChars);

            _m = m;
        }

        private void decodeButton_Click(object sender, EventArgs e)
        {
            var inputSeq = inputSequenceRichTextBox.Text;

            var encodedMsg = new List<string>();
            foreach (var ch in _msgChars)
            {
                encodedMsg.Add(_msgCodewords[ch].ToString().Replace(",", ""));
            }

            decodedSequenceRichTextBox.Clear();
            var startIndex = 0;
            var toogleColor = true;
            while (inputSeq != "")
            {
                var decoded = false;
                for (var i = 0; i < encodedMsg.Count; i++)
                {
                    if (inputSeq.StartsWith(encodedMsg[i]))
                    {
                        var index = decodedSequenceRichTextBox.Text.Length;
                        var dataToAppend = _msgChars[i];
                        if (dataToAppend == "NL")
                        {
                            decodedSequenceRichTextBox.AppendText(Environment.NewLine);
                        }
                        else
                        {
                            decodedSequenceRichTextBox.AppendText(dataToAppend);
                            decodedSequenceRichTextBox.Select(index, 1);
                            decodedSequenceRichTextBox.SelectionColor = toogleColor ? Color.Red : Color.Blue;
                        }

                        inputSeq = inputSeq.Substring(encodedMsg[i].Length);

                        inputSequenceRichTextBox.Select(startIndex, encodedMsg[i].Length);
                        inputSequenceRichTextBox.SelectionColor = toogleColor ? Color.Red : Color.Blue;
                        startIndex = startIndex + encodedMsg[i].Length;

                        toogleColor = !toogleColor;
                        decoded = true;

                        break;
                    }
                }

                if (!decoded)
                {
                    var index = decodedSequenceRichTextBox.Text.Length;
                    decodedSequenceRichTextBox.AppendText(inputSeq);

                    decodedSequenceRichTextBox.Select(index, inputSeq.Length);
                    decodedSequenceRichTextBox.SelectionColor = Color.Black;

                    break;
                }
            }
        }

        private void generateRandomSequencePictureBox_Click(object sender, EventArgs e)
        {
            IEnumerable<string> encodingAlphabets = ShannonFano.GetEncodingAlphabets(_m);
            var randomSeq = "";
            var r = new Random();
            for (var i = 0; i < 30; i++)
            {
                randomSeq = randomSeq + encodingAlphabets.ElementAt(r.Next(0, _m));
            }
            inputSequenceRichTextBox.Text = randomSeq;
        }

        private void inputSequenceRichTextBox_TextChanged(object sender, EventArgs e)
        {
            if (inputSequenceRichTextBox.Text.Contains("\t"))
            {
                inputSequenceRichTextBox.Text = inputSequenceRichTextBox.Text.Replace("\t", "").Replace(" ", "");
            }
        }

    }
}
