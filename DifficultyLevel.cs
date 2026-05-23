namespace WordTrainer
{
    public class DifficultyLevel
    {
        public string Name { get; set; } = string.Empty;
        public int Value { get; set; }

        public override string ToString() => Name;
    }
}
