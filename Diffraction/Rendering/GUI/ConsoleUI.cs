using Diffraction.Rendering.Meshes;
using ImGuiNET;

namespace Diffraction.Rendering.GUI;

public class ConsoleUI : EventObject
{
    private StringWriter stringWriter = new StringWriter();
    public ConsoleUI()
    {
        Console.SetOut(stringWriter);
    }
    public override void Render(Camera camera)
    {
        ImGui.Begin("Console");
        ImGui.SetWindowSize(new System.Numerics.Vector2(400, 200));
        ImGui.SetWindowPos(new System.Numerics.Vector2(camera.Resolution.X/2 - ImGui.GetWindowWidth()/2, camera.Resolution.Y - ImGui.GetWindowHeight() - 10));
        // Show the last console message
        ImGui.Text(stringWriter.ToString());
        ImGui.End();
    }
}