using Silk.NET.OpenGL;

namespace Diffraction.Rendering.Shaders;

public struct ShaderAttribute
{
    public string Name;
    public GLEnum Type;
    public int Location;
    public int Size;
    
    public ShaderAttribute(string name, GLEnum type, int location, int size)
    {
        Name = name;
        Type = type;
        Location = location;
        Size = size;
    }
}