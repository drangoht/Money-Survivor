using UnityEngine;

/// <summary>
/// Abstraction over Unity's Input so gameplay/UI code depends on an interface
/// instead of static calls. Makes it easier to test and to swap backends later.
/// </summary>
public interface IInputService
{
    Vector2 GetMoveAxis();              // Player movement
    float GetHorizontalMenuAxis();      // Left/right navigation
    float GetVerticalMenuAxis();        // Up/down navigation

    bool GetPausePressed();             // Pause / unpause
    bool GetSubmitPressed();            // Confirm / select
}

public static class InputService
{
    private static IInputService _current;

    /// <summary>Current input backend. Defaults to legacy Input Manager implementation.</summary>
    public static IInputService Current
    {
        get
        {
            if (_current == null) _current = new LegacyInputService();
            return _current;
        }
        set => _current = value;
    }
}

/// <summary>
/// Default implementation that wraps Unity's legacy Input Manager.
/// </summary>
public class LegacyInputService : IInputService
{
    public Vector2 GetMoveAxis()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        var v = new Vector2(x, y);
        if (v.sqrMagnitude > 1f) v.Normalize();
        return v;
    }

    public float GetHorizontalMenuAxis() => Input.GetAxisRaw("Horizontal");

    public float GetVerticalMenuAxis() => Input.GetAxisRaw("Vertical");

    public bool GetPausePressed()
    {
        return Input.GetKeyDown(KeyCode.Escape)
               || Input.GetButtonDown("Cancel")
               || Input.GetKeyDown(KeyCode.JoystickButton7); // Start
    }

    public bool GetSubmitPressed()
    {
        return Input.GetButtonDown("Submit");
    }
}

