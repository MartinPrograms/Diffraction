using System.Numerics;
using Diffraction.Physics;
using Diffraction.Rendering.Meshes;
using Diffraction.Scripting.Globals;
using ImGuiNET;

namespace Diffraction.Rendering.Objects;

public class ObjectUI : EventObject
{
    private List<Object> _objects = new List<Object>();
    Vector3 force = Vector3.Zero;

    public override void Render(Camera camera)
    {
        ImGui.Begin("Object Editor");

        var obj = ObjectScene.Instance.SelectedObject;

        if (obj == null)
        {
            ImGui.Text("No object selected");
        }
        else
        {
            if (ImGui.TreeNodeEx(obj.Name + "##" + obj.GetHashCode(), ImGuiTreeNodeFlags.DefaultOpen))
            {
                bool shouldUpdate = false;
                ImGui.Checkbox("Visible##" + obj.GetHashCode(), ref obj.IsVisible);
                var skybox = obj.IsSkyBox;
                ImGui.Checkbox("Is Skybox##" + obj.GetHashCode(), ref skybox);
                obj.IsSkyBox = skybox;
                ImGui.Text("Position");
                if (ImGui.DragFloat3("##Position" + obj.GetHashCode(), ref obj.Transform.Position))
                {
                    shouldUpdate = true;
                }

                ImGui.Text("Rotation");
                var rot = new Vector4(obj.Transform.Rotation.X, obj.Transform.Rotation.Y, obj.Transform.Rotation.Z,
                    obj.Transform.Rotation.W);

                if (ImGui.DragFloat4("##Rotation" + obj.GetHashCode(), ref rot))
                {
                    shouldUpdate = true;
                }

                rot = Vector4.Normalize(rot);
                obj.Transform.Rotation = new Quaternion(rot.X, rot.Y, rot.Z, rot.W);

                ImGui.Text("Scale");
                if (ImGui.DragFloat3("##Scale" + obj.GetHashCode(), ref obj.Transform.Scale))
                {
                    shouldUpdate = true;
                }

                foreach (var updatable in obj.Components)
                {
                    if (updatable is PhysicsObject physicsObject)
                    {
                        ImGui.Text("Physics");
                        ImGui.Text("Add Force");

                        ImGui.DragFloat3("##Force" + obj.GetHashCode(), ref force);
                        if (ImGui.Button("Apply Force"))
                        {
                            physicsObject.AddForce(force);
                        }
                    }
                }

                if (shouldUpdate)
                {
                    foreach (var objUpdatable in obj.Components)
                    {
                        if (objUpdatable is PhysicsObject physicsObject)
                        {
                            physicsObject.SetPhysicsTransform(obj.Transform);
                        }

                        if (objUpdatable is StaticPhysicsObject staticPhysicsObject)
                        {
                            staticPhysicsObject.SetPhysicsTransform(obj.Transform);
                        }
                    }
                }

                ImGui.TreePop();
            }

        }

        ImGui.End();
    }

    public void SetObjects(List<Object> objects)
    {
        _objects = objects;
    }
}