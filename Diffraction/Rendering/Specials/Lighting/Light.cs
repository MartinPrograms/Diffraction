using System.Numerics;
using Diffraction.Rendering.Meshes;
using Diffraction.Rendering.Windowing;
using Diffraction.Scripting.Globals;
using Diffraction.Serializables;
using Newtonsoft.Json;
using Silk.NET.OpenGL;

namespace Diffraction.Rendering.Specials.Lighting;

public class Light : EventObject
{
    // The base class for all lights
    public Vector3 Color = new Vector3(1, 1, 1); 
    public bool CastsShadows = false;
    public float Intensity = 1;
    public bool Enabled = true;
    public sObject Parent;
    public float Range = 10;
    // Shadows:
    public sShader ShadowShader;
    [JsonIgnore]
    internal Shaders.Shader Shader;
    public float ShadowBias = 0.005f;
    internal uint ShadowMap; // Texture ID
    internal uint ShadowFBO; // Framebuffer ID
    public Vector2 ShadowMapSize = new Vector2(8096, 8096); // The resolution of the shadow map
    public float ShadowNear = 0.1f; // The near plane of the shadow camera
    public float ShadowFar = 100; // The far plane of the shadow camera
    // All remaining shadow related stuff is per light type, so it's in the derived classes.

    public Transform Transform => Parent.GetObject().Transform;
    internal GL GL => Window.Instance.GL;
    public Light(sObject parent, sShader sShader)
    {
        Parent = parent;
        ShadowShader = sShader;
        ObjectScene.Instance.RegisterLight(this);
        
        Name = "Light";
    }

    public void CreateShadowFBO()
    {
        // Create the shadow FBO
        GL.GenFramebuffers(1, out ShadowFBO);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, ShadowFBO);
    }
    
    public void BindShadowFBO()
    {
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, ShadowFBO);
    }
    
    public unsafe void CreateShadowMap()
    {
        // Create the shadow map
        GL.GenTextures(1, out ShadowMap);
        GL.BindTexture(TextureTarget.Texture2D, ShadowMap);
        GL.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.DepthComponent, (uint)ShadowMapSize.X, (uint)ShadowMapSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, null);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
        
        float[] borderColor = {1.0f, 1.0f, 1.0f, 1.0f};
        fixed (float* ptr = borderColor)
        {
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBorderColor, ptr);
        }
        // Attach the shadow map to the FBO
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, ShadowMap, 0);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        
        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            throw new Exception("Framebuffer is not complete!");
        }
        
        if (GL.GetError() != GLEnum.NoError)
        {
            throw new Exception("An OpenGL error occurred!");
        }
    }
    
    public virtual void BindShadowMap(TextureUnit unit)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.Texture2D, ShadowMap);
        GL.ActiveTexture(TextureUnit.Texture0);
    }

    public IntPtr GetShadowMap()
    {
        return (IntPtr)ShadowMap;
    }

    public virtual int GetLightType()
    {
        Console.WriteLine("Warning: GetLightType() called on base Light class");
        return -1; // this is a placeholder, should not be used
    }
}

public enum LightType
{
    Directional = 0,
    Point = 1,
    Spot = 2
}