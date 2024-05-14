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
}