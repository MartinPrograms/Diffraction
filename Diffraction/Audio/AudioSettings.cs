using System.Numerics;

namespace Diffraction.Audio;

public class AudioSettings
{
    public float Volume { get; set; } = 1.0f;
    public float Pitch { get; set; } = 1.0f;
    public bool Loop { get; set; } = false;
    public Vector3 Position { get; set; } = Vector3.Zero;
}