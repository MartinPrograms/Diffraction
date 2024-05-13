using Diffraction.Physics;
using Diffraction.Rendering;
using Diffraction.Rendering.Meshes;
using Diffraction.Scripting.Globals;
using ImGuiNET;
using Silk.NET.Assimp;
using Camera = Diffraction.Rendering.Camera;

namespace Diffraction.Editor.GUI;

public class MainMenuBar : EventObject
{
    public override void Render(Camera camera)
    {
        ImGui.BeginMainMenuBar();
        if (ImGui.BeginMenu("File"))
        {
            if (ImGui.MenuItem("New"))
            {
                // New file
            }
            if (ImGui.MenuItem("Open"))
            {
                // Open file
            }
            if (ImGui.MenuItem("Save"))
            {
                // Save file
            }
            if (ImGui.MenuItem("Save As"))
            {
                // Save file as
            }
            ImGui.EndMenu();
        }
        
        if (ImGui.BeginMenu("Edit"))
        {
            if (ImGui.MenuItem("Undo"))
            {
                // Undo
            }
            if (ImGui.MenuItem("Redo"))
            {
                // Redo
            }
            ImGui.EndMenu();
        }
        
        if (ImGui.BeginMenu("View"))
        {
            if(ImGui.MenuItem("Save Layout"))
            {
                ImGui.SaveIniSettingsToDisk("layout.ini");
            }
            if (ImGui.MenuItem("Viewport"))
            {
                // Viewport
            }
            if (ImGui.MenuItem("Inspector"))
            {
                // Inspector
            }
            ImGui.EndMenu();
        }
        
        if (ImGui.BeginMenu("Window"))
        {
            if (ImGui.MenuItem("Close"))
            {
                // Close window
            }
            ImGui.EndMenu();
        }
        
        ImGui.Spacing();
        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.Spacing();
        
        if (ImGui.MenuItem("Play"))
        {
            ObjectScene.Instance.Play();
        }
        
        if (ImGui.MenuItem("Stop"))
        {
            Simulation.Instance.Clear();
            ObjectScene.Instance.Stop();
        }
        
        if (ImGui.MenuItem("Reload"))
        {
            Simulation.Instance.Clear();
            ObjectScene.Instance.ReloadScene();
        }
    }
}