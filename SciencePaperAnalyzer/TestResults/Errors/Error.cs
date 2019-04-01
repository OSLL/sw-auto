namespace TestResults.Errors
{
    public abstract class Error
    {
        public string Explanation { get; }

        public string Tip { get; }

        public ErrorType ErrorType { get; }
    }
}
