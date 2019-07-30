namespace AnalyzeResults.Errors
{
    public abstract class Error
    {
        public Error(ErrorType type, string name, string explanation, string tip)
        {
            ErrorType = type;
            Name = name;
            Explanation = explanation;
            Tip = tip;
        }

        public string Name { get; }

        public string Explanation { get; }

        public string Tip { get; }

        public ErrorType ErrorType { get; }
    }
}
