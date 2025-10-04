namespace WinTube.Model;

public class ListModeChangeMessage(bool isCompactMode)
{
    public bool IsCompactMode { get; } = isCompactMode;
}