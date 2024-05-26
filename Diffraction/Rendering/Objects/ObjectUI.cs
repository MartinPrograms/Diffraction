using System.Numerics;
using System.Reflection;
using Diffraction.Physics;
using Diffraction.Rendering.GUI;
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
                
                ListAllProperties(obj);

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

    private void ListAllProperties(Object o)
    {
        ImGui.Text("Surface Properties");
        var objProperties = o.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var property in objProperties)
        {
            // If the property has the attribute ShowInEditor, then show it
            if (property.GetCustomAttribute<ShowInEditor>() == null)
            {
                continue;
            }

            if (property.Name == "Name" || property.Name == "Transform" || property.Name == "IsVisible")
            {
                continue;
            }

            if (property.FieldType == typeof(float))
            {
                float value = (float)property.GetValue(o);
                if (ImGui.DragFloat(property.Name + "##" + o.GetHashCode(), ref value))
                {
                    property.SetValue(o, value);
                }
            }
            else if (property.FieldType == typeof(Vector3))
            {
                Vector3 value = (Vector3)property.GetValue(o);
                if (ImGui.DragFloat3(property.Name + "##" + o.GetHashCode(), ref value))
                {
                    property.SetValue(o, value);
                }
            }
            else if (property.FieldType == typeof(Vector4))
            {
                Vector4 value = (Vector4)property.GetValue(o);
                if (ImGui.DragFloat4(property.Name + "##" + o.GetHashCode(), ref value))
                {
                    property.SetValue(o, value);
                }
            }
            else if (property.FieldType == typeof(bool))
            {
                bool value = (bool)property.GetValue(o);
                if (ImGui.Checkbox(property.Name + "##" + o.GetHashCode(), ref value))
                {
                    property.SetValue(o, value);
                }
            }
        }

        ImGui.Text("Component Properties");
        
        foreach (var component in o.Components)
        {
            ImGui.Text(component.GetType().Name);
            foreach (var property in component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                // If the property has the attribute ShowInEditor, then show it
                if (property.GetCustomAttribute<ShowInEditor>() == null)
                {
                    continue;
                }

                if (property.Name == "Name" || property.Name == "Transform" || property.Name == "IsVisible")
                {
                    continue;
                }

                if (property.FieldType == typeof(float))
                {
                    float value = (float)property.GetValue(component);
                    if (ImGui.DragFloat(property.Name + "##" + component.GetHashCode(), ref value))
                    {
                        property.SetValue(component, value);
                    }
                }
                else if (property.FieldType == typeof(Vector3))
                {
                    Vector3 value = (Vector3)property.GetValue(component);
                    if (ImGui.DragFloat3(property.Name + "##" + component.GetHashCode(), ref value))
                    {
                        property.SetValue(component, value);
                    }
                }
                else if (property.FieldType == typeof(Vector4))
                {
                    Vector4 value = (Vector4)property.GetValue(component);
                    if (ImGui.DragFloat4(property.Name + "##" + component.GetHashCode(), ref value))
                    {
                        property.SetValue(component, value);
                    }
                }
                else if (property.FieldType == typeof(bool))
                {
                    bool value = (bool)property.GetValue(component);
                    if (ImGui.Checkbox(property.Name + "##" + component.GetHashCode(), ref value))
                    {
                        property.SetValue(component, value);
                    }
                }
            }
        }
    }

    public void SetObjects(List<Object> objects)
    {
        _objects = objects;
    }
}