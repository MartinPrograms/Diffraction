using System.Numerics;
using Diffraction.Scripting.Globals;
using Diffraction.Serializables;
using Silk.NET.OpenGL;

namespace Diffraction.Rendering.Specials.Lighting;

public class DirectionalLight : Light
{
    
    // Shadows (extra):
    // In this case its an orthographic camera
    public float ShadowSize = 10; // The size of the shadow camera
    
    public DirectionalLight(sObject parent, sShader sShader) : base(parent, sShader)
    {
        
            CreateShadowFBO();
            CreateShadowMap();
        
        
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
            GL.Clear(ClearBufferMask.DepthBufferBit);
            Shader.Use();
            
            var lightProjection = Matrix4x4.CreateOrthographic(ShadowSize, ShadowSize, ShadowNear, ShadowFar);
            var lightView = Matrix4x4.CreateLookAt(Parent.GetObject().Transform.Position, Parent.GetObject().Transform.Position + Parent.GetObject().Transform.Forward, Parent.GetObject().Transform.Up);
            
            var lightSpaceMatrix = lightView * lightProjection;
            Shader.SetMat4("lightSpaceMatrix", lightSpaceMatrix);
            
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
}