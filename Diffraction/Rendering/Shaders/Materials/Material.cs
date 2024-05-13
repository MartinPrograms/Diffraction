using Diffraction.Rendering.Windowing;
using Diffraction.Serializables;
using Newtonsoft.Json;
using Silk.NET.OpenGL;

namespace Diffraction.Rendering.Shaders.Materials;

public class Material
{
    public sShader sShader;
    public sTexture sTexture;

    [JsonIgnore] public Shader Shader = null;

    [JsonIgnore] public Texture Texture = null;
    
    public Material(sShader shader, sTexture texture)
    {
        sShader = shader;
        sTexture = texture;
    }
    
    public void Use()
    {
        if (Shader == null)
        {
            Shader = ShaderUtils.GetShader(sShader.ShaderName);
        }
        if (Texture == null)
        {
            Texture = TextureUtils.GetTexture(sTexture.TextureName);
        }
        var gl = Window.Instance.GL;
        Shader.Use();
        
        gl.ActiveTexture(TextureUnit.Texture0);
        Texture.Bind();
        
        Shader.SetInt("texture0", 0);
    }
}