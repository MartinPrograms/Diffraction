using System.Numerics;
using Diffraction.Rendering.Windowing;
using Diffraction.Serializables;
using Newtonsoft.Json;
using Silk.NET.OpenGL;
using Silk.NET.Vulkan;

namespace Diffraction.Rendering.Shaders.Materials;

public class Material
{
    public sShader sShader;
    public sTexture sTexture;
    public sTexture sNormalMap;
    
    public Vector3 Color = new Vector3(1, 1, 1);
    public float SpecularStrength = 0.5f;
    public float Shininess = 32;

    public TriangleFace CullMode = TriangleFace.Back;
    
    [JsonIgnore] public Shader Shader = null;

    [JsonIgnore] public Texture Texture = null;
    
    [JsonIgnore] public Texture NormalMap = null;
    
    public Material(sShader shader, sTexture texture, sTexture normalMap)
    {
        sShader = shader;
        sTexture = texture;
        sNormalMap = normalMap;
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
        if (NormalMap == null)
        {
            NormalMap = TextureUtils.GetTexture(sNormalMap.TextureName);
        }
        
        var gl = Window.Instance.GL;
        Shader.Use();
        
        Texture.Bind();
        
        Shader.SetInt("texture0", 0);
        
        NormalMap.Bind(TextureUnit.Texture1);  // everything 8+ is shadow maps, the most expensive part of the shader.
        
        Shader.SetInt("normalMap", 1);
        
        Shader.SetVec3("materialColor", Color);
        Shader.SetFloat("materialSpecularStrength", SpecularStrength);
        Shader.SetFloat("materialSpecularExponent", Shininess);
        
        gl.CullFace(CullMode);
    }
}