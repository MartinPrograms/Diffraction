using System.Numerics;
using Diffraction.Rendering.Windowing;
using Diffraction.Scripting.Globals;
using Diffraction.Serializables;
using Silk.NET.OpenGL;

namespace Diffraction.Rendering.Specials.Lighting;

public class DirectionalLight : Light
{
    
    // Shadows (extra):
    // In this case its an orthographic camera
    public float ShadowSize = 10; // The size of the shadow camera

    public DirectionalLight(sObject parent, sShader sShader, bool castShadows) : base(parent, sShader)
    {
        CastsShadows = castShadows;
        if (CastsShadows)
        {
            CreateShadowFBO();
            CreateShadowMap();
        }

        Name = "Directional Light";
        
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
            GL.Viewport(0, 0, (uint)ShadowMapSize.X, (uint)ShadowMapSize.Y);
            GL.Clear(ClearBufferMask.DepthBufferBit );
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            
            Shader.Use();

            GL.CullFace(TriangleFace.Front);
            
            var lightSpaceMatrix = GetLightSpaceMatrix();
            Shader.SetMat4("lightSpaceMatrix", lightSpaceMatrix);
            
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
        return (int)LightType.Directional;
    }

    public Matrix4x4 GetLightSpaceMatrix()
    {
        var lightProjection = Matrix4x4.CreateOrthographic(ShadowSize, ShadowSize, ShadowNear, ShadowFar);
        if (Parent == null)
        {
            return lightProjection;
        }
        var lightView = Matrix4x4.CreateLookAt(-Parent.GetObject().Transform.Forward, Vector3.Zero, Parent.GetObject().Transform.Up);
        
        return lightView * lightProjection;
    }
}