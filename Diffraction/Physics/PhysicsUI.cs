using System.Numerics;
using Diffraction.Rendering;
using Diffraction.Rendering.Meshes;
using Diffraction.Scripting.Globals;
using ImGuiNET;

namespace Diffraction.Physics;

public class PhysicsUI : EventObject
{
    private string _stopStart = "Play";
    public override void Render(Camera camera)
    {
        ImGui.Begin("Physics Editor");
        // 2 decimal places
        ImGui.Text("Time Since Start: " + Time.TimeSinceStart.ToString("F2") + "s");
        ImGui.DragFloat("Time Scale", ref Time.TimeScale, 0.01f, 0.001f, 10);
        
        ImGui.End();
    }
}