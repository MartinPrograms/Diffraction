using System.Numerics;
using Diffraction.Rendering;
using Diffraction.Rendering.Meshes;
using ImGuiNET;
using Object = Diffraction.Rendering.Objects.Object;

namespace Diffraction.Scripting.Globals;

public class SceneUI : EventObject
{
    public override void Render(Camera camera)
    {
        ImGui.Begin("Scene View");
        ImGui.Text("Scene Bounds");
        ImGui.DragFloat4("##Bounds", ref ObjectScene.Instance.Bounds);
        
        ImGui.Text("Objects");
        RecursiveRender(ObjectScene.Instance.Objects);
        
        ImGui.End();
    }

    private void RecursiveRender(List<Object> instanceObjects)
    {
        foreach (Object obj in instanceObjects)
        {
            bool selected = ObjectScene.Instance.SelectedObject == obj;

            var node = ImGui.TreeNodeEx(obj.Name + "##" + obj.GetHashCode(),
                (selected ? ImGuiTreeNodeFlags.Selected : ImGuiTreeNodeFlags.None) | ImGuiTreeNodeFlags.OpenOnArrow);

            if (ImGui.IsItemClicked())
            {
                ObjectScene.Instance.SelectedObject = obj;
            }

            if (node)
            {
                RecursiveRender(obj.Children);
                ImGui.TreePop();
            }
        }
    }
}