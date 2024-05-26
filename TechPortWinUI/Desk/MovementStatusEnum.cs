namespace TechPortWinUI.Desk;

/// <summary>
/// Enum representing the current movement of the desk
/// </summary>
public enum MovementStatus
{
    /// <summary>
    /// The desk is at a fix position
    /// </summary>
    Idle = 0,
    /// <summary>
    /// The desk is moving up
    /// </summary>
    MovingUp = 1,
    /// <summary>
    /// The desk is moving down
    /// </summary>
    MovingDown = -1
}