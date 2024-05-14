using System.Diagnostics;

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