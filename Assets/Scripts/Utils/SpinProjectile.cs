using UnityEngine;

/// <summary>
/// Spins a projectile/weapon sprite.
/// Attach to the sprite child of any projectile or orbital weapon.
/// </summary>
public class SpinProjectile : MonoBehaviour
{
    [Tooltip("Degrees per second. Negative = counter-clockwise.")]
    public float speed = 360f;

    void Update() => transform.Rotate(0f, 0f, speed * Time.deltaTime);
}
