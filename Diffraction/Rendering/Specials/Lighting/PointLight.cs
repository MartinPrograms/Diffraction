using System.Numerics;
using Diffraction.Scripting.Globals;
using Diffraction.Serializables;
using Silk.NET.OpenGL;
using Silk.NET.SDL;
using PixelFormat = Silk.NET.OpenGL.PixelFormat;
using PixelType = Silk.NET.OpenGL.PixelType;
using Window = Diffraction.Rendering.Windowing.Window;

namespace Diffraction.Rendering.Specials.Lighting;

public class PointLight : Light
{
    public PointLight(sObject parent, sShader sShader) : base(parent, sShader)
    {
        CreateAllAroundShadowMapAndFbo();
        Name = "Point Light";
        
        CastsShadows = true;
        
        ShadowFar = 10;
    }
    
    public unsafe void CreateAllAroundShadowMapAndFbo()
    {
        ShadowMapSize = new Vector2(1024, 1024);

        // Create the shadow FBO
        GL.GenFramebuffers(1, out ShadowFBO);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, ShadowFBO);
        
        // Create the shadow map
        GL.GenTextures(1, out ShadowMap);
        
        GL.BindTexture(TextureTarget.TextureCubeMap, ShadowMap);
        
        GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, InternalFormat.DepthComponent, (uint)ShadowMapSize.X, (uint)ShadowMapSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, null);
        GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, InternalFormat.DepthComponent, (uint)ShadowMapSize.X, (uint)ShadowMapSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, null);
        GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, InternalFormat.DepthComponent, (uint)ShadowMapSize.X, (uint)ShadowMapSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, null);
        GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, InternalFormat.DepthComponent, (uint)ShadowMapSize.X, (uint)ShadowMapSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, null);
        GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, InternalFormat.DepthComponent, (uint)ShadowMapSize.X, (uint)ShadowMapSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, null);
        GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, InternalFormat.DepthComponent, (uint)ShadowMapSize.X, (uint)ShadowMapSize.Y, 0, PixelFormat.DepthComponent, PixelType.Float, null);
        
        // God bless this code
        
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
        
        if (GL.GetError() != GLEnum.NoError)
        {
            Console.WriteLine("An OpenGL error occurred!");
        }
        
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.TextureCubeMap, ShadowMap, 0);
        
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
        
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != GLEnum.FramebufferComplete)
        {
            Console.WriteLine("Framebuffer not complete!");
        }
        
        if (GL.GetError() != GLEnum.NoError)
        {
            Console.WriteLine("An OpenGL error occurred!");
        }
        
    }
    
    public override void Render(Camera camera)
    {
        ShadowMapSize = new Vector2(1024, 1024);

        if (!Enabled) return;
        if (CastsShadows)
        {
            if (Shader == null)
            {
                Shader = ShadowShader.GetShader();
            }

            GL.BindFramebuffer(FramebufferTarget.Framebuffer, ShadowFBO);

            GL.Viewport(0, 0, (uint)ShadowMapSize.X, (uint)ShadowMapSize.Y);
            GL.Clear(ClearBufferMask.DepthBufferBit );
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            Shader.Use();
            GL.CullFace(TriangleFace.Front);
            
            var shadowProj = Matrix4x4.CreatePerspectiveFieldOfView(MathUtils.ToRadians(90), 1, 0.1f, ShadowFar);
            var position = Parent.GetObject().Transform.Position;
            
            List<Matrix4x4> shadowMatrices = new();

            shadowMatrices.Add(shadowProj * Matrix4x4.CreateLookAt(position, position + new Vector3(1.0f, 0.0f, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f)));
            
            shadowMatrices.Add(shadowProj * Matrix4x4.CreateLookAt(position, position + new Vector3(-1.0f, 0.0f, 0.0f),
                new Vector3(0.0f, -1.0f, 0.0f)));
            
            shadowMatrices.Add(shadowProj * Matrix4x4.CreateLookAt(position, position + new Vector3(0.0f, 1.0f, 0.0f),
                new Vector3(0.0f, 0.0f, 1.0f)));
            
            shadowMatrices.Add(shadowProj * Matrix4x4.CreateLookAt(position, position + new Vector3(0.0f, -1.0f, 0.0f),
                new Vector3(0.0f, 0.0f, -1.0f)));
            
            shadowMatrices.Add(shadowProj * Matrix4x4.CreateLookAt(position, position + new Vector3(0.0f, 0.0f, 1.0f),
                new Vector3(0.0f, -1.0f, 0.0f)));
            
            shadowMatrices.Add(shadowProj * Matrix4x4.CreateLookAt(position, position + new Vector3(0.0f, 0.0f, -1.0f),
                new Vector3(0.0f, -1.0f, 0.0f)));
            
            // Our shader has a geometry shader that will create the 6 faces of the cube map, so we only need to render the scene once
            // Set the shadowMatrices in a loop
            for (int i = 0; i < 6; i++)
            {
                Shader.SetMat4("shadowMatrices[" + i + "]", shadowMatrices[i]);
            }
            
            Shader.SetFloat("far_plane", ShadowFar);
            Shader.SetVec3("lightPos", Parent.GetObject().Transform.Position);
            
            foreach (var obj in ObjectScene.Instance.Objects)
            {
                if (obj is Skybox || obj.IsSkyBox)
                {
                    return; // We don't want to render the skybox to the shadow map
                }
                obj.RawRender(Shader);
            }
            
            GL.CullFace(TriangleFace.Back);
            
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.Viewport(0, 0, (uint)Window.Instance.IWindow.FramebufferSize.X, (uint)Window.Instance.IWindow.FramebufferSize.Y);
            
        }
    }
    
    public override int GetLightType()
    {
        return (int)LightType.Point;
    }

    public override void BindShadowMap(TextureUnit unit)
    {
        GL.ActiveTexture(unit);
        GL.BindTexture(TextureTarget.TextureCubeMap, ShadowMap);
    }
}