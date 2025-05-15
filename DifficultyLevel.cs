/// <summary>
/// Уровень сложности
/// </summary>
public class DifficultyLevel
{
    public string Name { get; set; }
    public int Value { get; set; }

    public override string ToString() => Name;
}