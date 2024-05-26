using Diffraction.Rendering.Shaders;

namespace Diffraction.Serializables;

public class sShader
{
    public string ShaderName;
    
    public sShader(string shaderName)
    {
        ShaderName = shaderName;
    }
    
    public Rendering.Shaders.Shader GetShader()
    {
        return ShaderUtils.GetShader(ShaderName);
    }
}