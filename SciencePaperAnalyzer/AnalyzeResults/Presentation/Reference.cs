namespace AnalyzeResults.Presentation
{
    public class Reference
    {
        public Reference(Sentence original, int number)
        {
            Original = original;
            Number = number;
        }

        public Sentence Original { get; set; }

        public int Number { get; set; }

        public int Year { get; set; }

        public bool ReferedTo { get; set; }
    }
}
