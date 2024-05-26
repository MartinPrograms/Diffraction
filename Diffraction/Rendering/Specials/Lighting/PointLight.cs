using System.Numerics;
using Diffraction.Scripting.Globals;
using Diffraction.Serializables;
using Silk.NET.OpenGL;

namespace Diffraction.Rendering.Specials.Lighting;

public class PointLight : Light
{
    public PointLight(sObject parent, sShader sShader) : base(parent, sShader)
    {
        CreateShadowFBO();
        CreateAllAroundShadowMap();
        
        Name = "Point Light";
    }
    
    public unsafe void CreateAllAroundShadowMap()
    {
        // Create the shadow map
        GL.GenTextures(1, out ShadowMap);
        GL.BindTexture(TextureTarget.TextureCubeMap, ShadowMap);
        
        for (int i = 0; i < 6; i++)
        {
            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, InternalFormat.DepthComponent, (uint)ShadowMapSize.X, (uint)ShadowMapSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, null);
        }
        
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        
        GL.BindTexture(TextureTarget.TextureCubeMap, 0);
        
        GL.GenFramebuffers(1, out ShadowFBO);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, ShadowFBO);
        GL.FramebufferTexture(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, ShadowMap, 0);
        
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        
    }
    
    public override void Render(Camera camera)
    {
        if (!Enabled) return;
        if (CastsShadows)
        {
            if (Shader == null)
            {
                Shader = ShadowShader.GetShader();
            }
            
            BindShadowFBO();
            
            List<Matrix4x4> shadowMatrices = new();
            shadowMatrices.Add(Matrix4x4.CreateLookAt(Parent.GetObject().Transform.Position, Parent.GetObject().Transform.Position + new Vector3(1, 0, 0), new Vector3(0, -1, 0)));
            shadowMatrices.Add(Matrix4x4.CreateLookAt(Parent.GetObject().Transform.Position, Parent.GetObject().Transform.Position + new Vector3(-1, 0, 0), new Vector3(0, -1, 0)));
            shadowMatrices.Add(Matrix4x4.CreateLookAt(Parent.GetObject().Transform.Position, Parent.GetObject().Transform.Position + new Vector3(0, 1, 0), new Vector3(0, 0, 1)));
            shadowMatrices.Add(Matrix4x4.CreateLookAt(Parent.GetObject().Transform.Position, Parent.GetObject().Transform.Position + new Vector3(0, -1, 0), new Vector3(0, 0, -1)));
            shadowMatrices.Add(Matrix4x4.CreateLookAt(Parent.GetObject().Transform.Position, Parent.GetObject().Transform.Position + new Vector3(0, 0, 1), new Vector3(0, -1, 0)));
            shadowMatrices.Add(Matrix4x4.CreateLookAt(Parent.GetObject().Transform.Position, Parent.GetObject().Transform.Position + new Vector3(0, 0, -1), new Vector3(0, -1, 0)));
            
            // Our shader has a geometry shader that will create the 6 faces of the cube map, so we only need to render the scene once
            Shader.Use();
            // Set the shadowMatrices in a loop
            for (int i = 0; i < 6; i++)
            {
                Shader.SetMat4("shadowMatrices[" + i + "]", shadowMatrices[i]);
            }
            
            Shader.SetFloat("far_plane", ShadowFar);
            Shader.SetVec3("lightPos", Parent.GetObject().Transform.Position);
            Shader.SetMat4("model", Parent.GetObject().Transform.GetModelMatrix());
            
            foreach (var obj in ObjectScene.Instance.Objects)
            {
                if (obj is Skybox || obj.IsSkyBox)
                {
                    return; // We don't want to render the skybox to the shadow map
                }
                obj.RawRender(Shader);
            }
            
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        }
    }
    
    public override int GetLightType()
    {
        return (int)LightType.Point;
    }
}