namespace AnalyzeResults.Presentation
{
    public abstract class Criterion
    {
        public Criterion(string name, CriterionType type, string description = "")
        {
            Name = name;
            Type = type;
            Description = description;
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public CriterionType Type { get; set; }

        public abstract bool IsMet();

        public abstract string GetStringValue();

        public abstract string GetStringRequirements();

        public abstract double GetGradePart();

        public abstract string GetAdvice();
    }
}
