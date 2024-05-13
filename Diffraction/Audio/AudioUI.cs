using Diffraction.Rendering;
using Diffraction.Rendering.Meshes;
using ImGuiNET;

namespace Diffraction.Audio;

public class AudioUI : EventObject
{
    public override void Render(Camera camera)
    {
        ImGui.Begin("Audio Editor");
        
        ImGui.Text("Globals:");
        float volume = Audio.Instance.Volume;
        ImGui.DragFloat("Master Volume", ref volume, 0.01f, 0.0f, 2.0f);
        Audio.Instance.Volume = volume;
        
        
        
        ImGui.End();
    }
}