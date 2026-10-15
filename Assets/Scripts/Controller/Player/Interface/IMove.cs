public interface IMove
{
    float Horizontal { get; }
    float Vertical { get; }
    bool IsRunning { get; }
    bool IsMoveDisabled { get; set; }
}
