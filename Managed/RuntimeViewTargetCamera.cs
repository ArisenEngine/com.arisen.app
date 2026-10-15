using System.Numerics;
using ArisenEngine.Core.Math;

namespace ArisenEngine.App;

/// <summary>
/// The third-person follow pose an interactive viewer uses when a package publishes a view target:
/// mouse look orbits, and the camera stands behind the target along the view direction. The math
/// is pure and static so the shell's follow behaviour is unit-testable without a window, and it
/// shares the renderer's yaw/pitch conventions: dragging right turns right by decreasing yaw, and
/// dragging down looks down by increasing pitch.
/// </summary>
public static class RuntimeViewTargetCamera
{
    public const float LookDegreesPerPixel = 0.14f;
    public const float MaxPitchDegrees = 70.0f;
    public const float DefaultDistance = 6.0f;
    public const float DefaultHeight = 1.6f;

    private const float RadiansPerDegree = MathF.PI / 180.0f;

    /// <summary>Advances the orbit angles from one mouse-look delta.</summary>
    public static (float Yaw, float Pitch) ApplyLook(
        float yawDegrees,
        float pitchDegrees,
        float deltaX,
        float deltaY)
    {
        float yaw = MathExtensions.WrapDegrees(yawDegrees - deltaX * LookDegreesPerPixel);
        float pitch = Math.Clamp(
            pitchDegrees + deltaY * LookDegreesPerPixel,
            -MaxPitchDegrees,
            MaxPitchDegrees);
        return (yaw, pitch);
    }

    /// <summary>
    /// The camera pose for a target: it watches along the orbit's view direction and stands
    /// <paramref name="distance"/> behind it, raised by <paramref name="height"/> over the target.
    /// </summary>
    public static void ComputeFollowPose(
        Vector3 target,
        float yawDegrees,
        float pitchDegrees,
        float distance,
        float height,
        out Vector3 position,
        out Quaternion rotation)
    {
        rotation = Quaternion.CreateFromYawPitchRoll(
            yawDegrees * RadiansPerDegree,
            pitchDegrees * RadiansPerDegree,
            0.0f);
        Vector3 forward = Vector3.Transform(MathExtensions.Forward, rotation);
        position = target + new Vector3(0.0f, height, 0.0f) - forward * distance;
    }
}
