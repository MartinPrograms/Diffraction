using System.Diagnostics;

namespace Diffraction.Scripting;

public static class Utilities
{
    public static void LaunchVSCode(string path)
    {
        Process.Start("code", path);
    }
}