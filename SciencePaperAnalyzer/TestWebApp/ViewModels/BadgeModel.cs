using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebPaperAnalyzer.ViewModels
{
    public class Color
    {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }
    }
    public class BadgeModel
    {
        public string Criterion { get; set; }
        public int MaxScore { get; set; }
        public int Score { get; set; }
        public bool IsProcessing { get; set; }
        public bool IsMLSProcessing { get; set; }
        public string ScoreString
        {
            get
            {
                if (!IsProcessing)
                    return Score + "/" + MaxScore;
                else return "Processing";
            }
        }
        public int ScoreStringWidth
        {
            get
            {
                return ScoreString.Length * 9;
            }
        }
        public int CriterionStringWidth
        {
            get
            {
                return Criterion.Length * 9;
            }
        }

        public Color Color
        {
            get
            {
                return new Color
                {
                    R = !IsProcessing ? 255 - (Score<0?0:Score) * 255 / (MaxScore == 0 ? 1 : MaxScore) : 0,
                    G = !IsProcessing ? (Score < 0 ? 0 : Score) * 255 / (MaxScore == 0 ? 1 : MaxScore) : 0,
                    B = !IsProcessing ? 0 : 170
                };
            }
        }
    }
}
