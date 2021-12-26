using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ShannonFanoCoding
{
    static class ShannonFano
    {
        public static void Encode(IEnumerable<string> msgChars, IEnumerable<string> msgProbs,
            int numberOfEncodingAlphabets, out Hashtable msgProbabilities, out Hashtable msgCodewords,
            out double totalProbability, out double entropyHx, out double l̅, out double efficiencyΗ,
            out int minimumCodewordLength, out int maximumCodewordLength)
        {
            msgProbabilities = new Hashtable();
            msgCodewords = new Hashtable();

            #region "Set Message and their Probabilities"

            for (var i = 0; i < msgChars.Count(); i++)
            {
                try
                {
                    var prob = msgProbs.ElementAt(i);
                    if (prob.Contains("/"))
                    {
                        msgProbabilities.Add(msgChars.ElementAt(i),
                            Convert.ToDouble(prob.Split(new char[] {'/'})[0]) /
                            Convert.ToDouble(prob.Split(new char[] {'/'})[1]));
                    }
                    else
                    {
                        msgProbabilities.Add(msgChars.ElementAt(i), double.Parse(prob));
                    }
                }
                catch
                {
                    // ignored
                }
            }

            var onlyProbabilitiesSorted =
                new List<double>(new ArrayList(msgProbabilities.Values).ToArray(typeof(double)) as IEnumerable<double>);
            onlyProbabilitiesSorted.Sort();
            onlyProbabilitiesSorted.Reverse();

            totalProbability = onlyProbabilitiesSorted.Sum();

            #endregion

            #region "Encode the message"

            var encodedMsg = Encode(onlyProbabilitiesSorted, GetEncodingAlphabets(numberOfEncodingAlphabets).ToArray());

            #endregion

            #region "Map the code words to the corresponding messages."

            var onlyMsgCharacters =
                new List<string>(new ArrayList(msgProbabilities.Keys).ToArray(typeof(string)) as IEnumerable<string>);
            var onlyProbabilities =
                new List<double>(new ArrayList(msgProbabilities.Values).ToArray(typeof(double)) as IEnumerable<double>);

            for (var i = 0; i < onlyProbabilitiesSorted.Count; i++)
            {
                var index = onlyProbabilities.IndexOf(onlyProbabilitiesSorted[i]);
                msgCodewords.Add(onlyMsgCharacters[index], encodedMsg[i]);
                onlyMsgCharacters.RemoveAt(index);
                onlyProbabilities.RemoveAt(index);
            }

            #endregion

            #region "Calculate Entropy"

            entropyHx = 0;
            for (var i = 0; i < onlyProbabilitiesSorted.Count; i++)
            {
                if (onlyProbabilitiesSorted[i] != 0)
                {
                    entropyHx = entropyHx + onlyProbabilitiesSorted[i] * Math.Log(onlyProbabilitiesSorted[i], 2);
                }
            }

            entropyHx = -entropyHx;

            #endregion

            #region "Calculate average length per codeword, L̅"

            maximumCodewordLength = 0;
            minimumCodewordLength = 0;
            l̅ = 0;
            for (var i = 0; i < onlyProbabilitiesSorted.Count; i++)
            {
                var codewordLength = encodedMsg[i].Length - encodedMsg[i].Replace(",", "").Length + 1;
                l̅ = l̅ + onlyProbabilitiesSorted[i] * codewordLength;
                if (maximumCodewordLength < codewordLength)
                {
                    maximumCodewordLength = codewordLength;
                }

                if (minimumCodewordLength > codewordLength)
                {
                    minimumCodewordLength = codewordLength;
                }
            }

            #endregion

            #region "Calculate Efficiency"

            efficiencyΗ = entropyHx / (l̅ * Math.Log(numberOfEncodingAlphabets, 2)) * 100;

            #endregion
        }

        public static List<string> GetEncodingAlphabets(int alphabetCount)
        {
            var eA = new List<string>();
            if (alphabetCount == 2)
            {
                eA.Add("0");
                eA.Add("1");
            }
            else if (alphabetCount % 2 == 0)
            {
                var x = alphabetCount / 2;

                for (var i = -x; i <= x; i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }

                    eA.Add(i.ToString());
                }
            }
            else
            {
                var x = (alphabetCount - 1) / 2;

                for (var i = -x; i <= x; i++)
                {
                    eA.Add(i.ToString());
                }
            }

            return eA;
        }

        private static List<string> Encode(List<double> msgProbs, string[] encodingAlphabets)
        {
            var m = encodingAlphabets.Count();

            var sortedMsgProbs = new List<double>();
            sortedMsgProbs.AddRange(msgProbs);

            #region "Sort the messages in order of decreasing probabilities."

            sortedMsgProbs.Sort();
            sortedMsgProbs.Reverse();

            #endregion

            if (sortedMsgProbs.Count == 1)
            {
                return new List<string>();
            }
            else if (sortedMsgProbs.Count <= m)
            {
                var codeSequence = new List<string>();
                for (var i = 0; i < sortedMsgProbs.Count; i++)
                {
                    codeSequence.Add(encodingAlphabets[i]);
                }

                return codeSequence;
            }
            else
            {
                #region "Find all the possible ways of partitioning the sorted messages."

                var patterns = new List<string>();

                if (m <= 4)
                {
                    patterns =
                        new List<string>(GenerateAllPossiblePatterns(sortedMsgProbs.Count, m - 1)).FindAll(pttrn =>
                            pttrn.Last() == '0');
                }
                else
                {
                    string bin;
                    for (var i = 2; i < Math.Pow(2, sortedMsgProbs.Count); i += 2)
                    {
                        bin = Convert.ToString(i, 2).PadLeft(sortedMsgProbs.Count, '0');
                        if (bin.Replace("0", "").Count() == (m - 1))
                        {
                            patterns.Add(bin);
                        }
                    }
                }

                patterns.Reverse();

                #endregion

                #region "Choose the most appropriate pattern."

                var optimalPattern = "";
                var tempDiffs = new List<double>();

                foreach (var pattern in patterns)
                {
                    double sum = 0;
                    double diff = 0;

                    for (var i = 0; i < pattern.Length; i++)
                    {
                        sum = sum + sortedMsgProbs[i];
                        if (pattern[i] == '1')
                        {
                            diff = sum - diff;
                            sum = 0;
                        }
                    }

                    var netDiff = Math.Abs((sum - diff) - ((m % 2 == 0) ? 0 : (1F / m)));
                    tempDiffs.Add(netDiff);

                    if (netDiff == 0)
                    {
                        optimalPattern = pattern;
                        break;
                    }
                }

                optimalPattern = patterns[tempDiffs.IndexOf(tempDiffs.Min())];

                #endregion

                #region "Generate the sequence."

                var alphabetIndex = 0;
                var codeSequence = new List<string>();
                var temp = new List<double>();
                for (var i = 0; i < optimalPattern.Length; i++)
                {
                    codeSequence.Add(encodingAlphabets[alphabetIndex]);
                    temp.Add(sortedMsgProbs[i]);
                    if (optimalPattern[i] == '1' || i == optimalPattern.Length - 1)
                    {
                        var subsequence = Encode(temp, encodingAlphabets);
                        for (var j = 0; j < subsequence.Count; j++)
                        {
                            codeSequence[i - temp.Count + 1 + j] =
                                codeSequence[i - temp.Count + 1 + j] + "," + subsequence[j];
                        }

                        temp.Clear();
                        alphabetIndex++;
                    }
                }

                return codeSequence;

                #endregion
            }
        }

        private static string[] GenerateAllPossiblePatterns(int totalBits, int totalHighBits)
        {
            var e = new List<string>();

            var block = "".PadLeft(totalHighBits, '1');

            var equivalentOne = new List<string>();

            for (var i = 1; i <= totalBits - totalHighBits; i++)
            {
                equivalentOne.AddRange(PadZeros(i + 1, "1"));
            }

            var blocks = new List<string>();

            foreach (var oneEq in equivalentOne)
            {
                for (var i = 1; i <= totalHighBits; i++)
                {
                    var blk = "";
                    for (var j = 0; j < i; j++)
                    {
                        blk = blk + oneEq;
                    }

                    blocks.AddRange(PadOnes(totalHighBits + blk.Length - i, blk));
                }

                for (var i = 1; i <= totalHighBits; i++)
                {
                    var blk = "";
                    for (var j = 0; j < i; j++)
                    {
                        blk = blk + "1" + oneEq;
                    }

                    var width = totalHighBits + blk.Length - 2 * i;
                    if (width <= totalBits)
                    {
                        if (blk.Replace("0", "").Length <= totalHighBits)
                        {
                            blocks.AddRange(PadOnes(width, blk));
                        }
                    }
                }
            }

            var trimmedBlocks = new List<string>();

            foreach (var blk in blocks)
            {
                var b = blk.Trim(new char[] {'0'});
                if (b.Length <= totalBits)
                {
                    if (!trimmedBlocks.Contains(b))
                    {
                        trimmedBlocks.Add(b);
                    }
                }
            }

            foreach (var blk in trimmedBlocks)
            {
                e.AddRange(PadZeros(totalBits, blk));
            }

            return e.ToArray();
        }

        private static List<string> PadZeros(int totalWidth, string block)
        {
            var e = new List<string>();
            var i = 0;
            while (true)
            {
                var err = block.PadLeft(totalWidth - i, '0').PadRight(totalWidth, '0');
                e.Add(err);
                i++;
                if (err.StartsWith(block))
                {
                    break;
                }
            }

            return e;
        }

        private static List<string> PadOnes(int totalWidth, string block)
        {
            var e = new List<string>();
            var i = 0;
            while (true)
            {
                var err = block.PadLeft(totalWidth - i, '1').PadRight(totalWidth, '1');
                e.Add(err);
                i++;
                if (err.StartsWith(block))
                {
                    break;
                }
            }

            return e;
        }
    }
}