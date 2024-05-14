using System.Numerics;
using Diffraction.Rendering.Meshes;
using Diffraction.Rendering.Shaders;
using Diffraction.Scripting.Globals;
using ImGuiNET;
using Silk.NET.OpenGL;

namespace Diffraction.Rendering.Objects;

[Serializable]
public class Object : EventObject
{
    public Guid Id = Guid.NewGuid();
    
    public List<EventObject> Components = new();
    
    public List<Object> Children = new();
    
    public string Name = "Object";
    
    public Object()
    {
    }
    
    public Object(string name)
    {
        Name = name;
    }
    
    public bool IsVisible = true;
    
    [ExposeToLua("Transform")]
    public Transform Transform = new Transform(new Vector3(0, 5, 0), new Quaternion(0,0,0,1), new Vector3(1, 1, 1));
    
    [ExposeToLua("Selected")]
    public bool Selected;

    public bool IsSkyBox
    {
        get
        {
            return Components.OfType<Mesh>().Any(mesh => mesh.IsSkybox);
        }
        set
        {
            foreach (Mesh mesh in Components.OfType<Mesh>())
            {
                mesh.IsSkybox = value;
            }
        }
    }


    public override void Render(Camera camera)
    {
        if (IsVisible)
        {
            foreach (var child in Children)
            {
                child.Render(camera);
            }
        }
        
        if (IsVisible && !Selected)
        {
            foreach (Mesh mesh in Components.OfType<Mesh>())
            {
                mesh.SetParentTransform(Transform);
                mesh.Render(camera);
            }
        }

        if (Selected)
        {
            foreach (Mesh mesh in Components.OfType<Mesh>())
            {
                mesh.SetParentTransform(Transform);
                if (IsVisible)
                {
                    mesh.Render(camera, ShaderUtils.GetShader("SelectionShader"), GLEnum.Fill);
                }
                else
                {
                    mesh.Render(camera, ShaderUtils.GetShader("TransparentShader"), GLEnum.Line);
                }
            }
        }
    }
    
    public override void Update(double time)
    {
        foreach (Object child in Children)
        {
            child.Update(time);
        }
        
        foreach (EventObject component in Components)
        {
            component.Update(time);
        }
    }
    
    public virtual void Dispose()
    {
        foreach (Object child in Children)
        {
            child.Dispose();
        }
    }

    public object Clone()
    {
        Object obj = new Object(Name)
        
        {
            Children = Children,
            Name = Name,
            IsVisible = IsVisible,
            Transform = Transform,
            Selected = Selected
        };
        
        return obj;
    }
}