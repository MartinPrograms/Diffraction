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
    
    [ExposeToLua("Components")]
    public List<EventObject> Components = new();
    
    [ExposeToLua("Children")]
    public List<Object> Children = new();
    
    [ExposeToLua("Name")]
    public string Name = "Object";
    
    public Object()
    {
    }
    
    public Object(string name)
    {
        Name = name;
    }
    
    [ExposeToLua("IsVisible")]
    public bool IsVisible = true;
    
    [ExposeToLua("Transform")]
    public Transform Transform = new Transform(new Vector3(0, 5, 0), new Quaternion(0,0,0,1), new Vector3(1, 1, 1));
    
    public EventObject GetComponent(string name)
    {
        return Components.FirstOrDefault(component => component.Name == name);
    }

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

    public bool Selected = false;

    public override void Render(Camera camera)
    {
        if (IsVisible)
        {
            foreach (var child in Children)
            {
                child.Render(camera);
            }
        }
        
        if (IsVisible)
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

    public void RawRender(Shaders.Shader shader)
    {
        // A render that does not use any shaders, because they are previously assigned out of this scope
        
        foreach (var child in Children)
        {
            child.RawRender(shader);
        }
        
        foreach (Mesh mesh in Components.OfType<Mesh>())
        {
            mesh.SetParentTransform(Transform);
            mesh.RawRender(shader);
        }
    }
}