using System.Diagnostics;
using System.Numerics;

namespace Diffraction.Scripting;

public static class Utilities
{
    public static void LaunchVSCode(string path)
    {
        if (OperatingSystem.IsLinux() || OperatingSystem.IsWindows())
        {
            Process.Start("code", path);
        }
        
        if (OperatingSystem.IsMacOS())
        {
            Process.Start("open", "\"/Applications/Visual Studio Code.app\" --args " + Path.GetFullPath(path));
        }
    }
}

public static class Mathf
{

    public static bool Approximately(Quaternion p0, float p1)
    {
        var angle = Math.Acos(p0.W) * 2;
        return Math.Abs(angle - p1) < 0.01f;
    }

    public static bool Approximately(Quaternion p0, Quaternion p1)
    {
        return Math.Abs(p0.X - p1.X) < 0.01f && Math.Abs(p0.Y - p1.Y) < 0.01f && Math.Abs(p0.Z - p1.Z) < 0.01f && Math.Abs(p0.W - p1.W) < 0.01f;
    }

    public static bool Approximately(Vector3 p0, Vector3 p1)
    {
        return Math.Abs(p0.X - p1.X) < 0.01f && Math.Abs(p0.Y - p1.Y) < 0.01f && Math.Abs(p0.Z - p1.Z) < 0.01f;
    }
    
    public static bool Approximately(float p0, float p1)
    {
        return Math.Abs(p0 - p1) < 0.01f;
    }
    
    public static float[] ToArray(this Matrix4x4 matrix)
    {
        return new[]
        {
            matrix.M11, matrix.M12, matrix.M13, matrix.M14,
            matrix.M21, matrix.M22, matrix.M23, matrix.M24,
            matrix.M31, matrix.M32, matrix.M33, matrix.M34,
            matrix.M41, matrix.M42, matrix.M43, matrix.M44
        };
    }
    
    public static Matrix4x4 ToMatrix4x4(this float[] array)
    {
        return new Matrix4x4(
            array[0], array[1], array[2], array[3],
            array[4], array[5], array[6], array[7],
            array[8], array[9], array[10], array[11],
            array[12], array[13], array[14], array[15]
        );
    }
    
    public static Vector3 ExtractTranslation(this Matrix4x4 matrix)
    {
        return new Vector3(matrix.M41, matrix.M42, matrix.M43);
    }
    
    public static Quaternion ExtractRotation(this Matrix4x4 matrix)
    {
        var forward = new Vector3(matrix.M31, matrix.M32, matrix.M33);
        var up = new Vector3(matrix.M21, matrix.M22, matrix.M23);
        return Quaternion.CreateFromRotationMatrix(Matrix4x4.CreateLookAt(Vector3.Zero, forward, up));
    }
    
    public static Vector3 ExtractScale(this Matrix4x4 matrix)
    {
        return new Vector3(matrix.M11, matrix.M22, matrix.M33);
    }
}