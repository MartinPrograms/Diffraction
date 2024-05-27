using System.Drawing;
using Diffraction.Rendering.Windowing;
using Diffraction.Scripting.Globals;
using ImGuiNET;
using Silk.NET.OpenGL;

namespace Diffraction.Rendering.Specials.Lighting;

public class LightUI : EventObject
{
    private int selectedShadowFrameBuffer = -1;

    public override unsafe void Render(Camera camera)
    {
        ImGui.Begin("Light & Shadow Settings");

        ImGui.Combo("Light", ref selectedShadowFrameBuffer, ObjectScene.Instance.Lights.Select(x => x.Name).ToArray(),
            ObjectScene.Instance.Lights.Count);

        if (selectedShadowFrameBuffer != -1)
        {
            var light = ObjectScene.Instance.Lights[selectedShadowFrameBuffer];
            
            ImGui.Text("Lighting");

            if (light is DirectionalLight)
            {
                DirectionalLight directionalLight = (DirectionalLight) light;
                ImGui.DragFloat("Shadow Size", ref directionalLight.ShadowSize, 0.01f);
                ImGui.DragFloat("Shadow Near", ref directionalLight.ShadowNear, 0.01f);
                ImGui.DragFloat("Shadow Far", ref directionalLight.ShadowFar, 0.01f);
            }
            
            ImGui.Separator();
            ImGui.Text("Shadow Settings");
            ImGui.Text("TBA");

            if (light.CastsShadows)
            {
                if (light is DirectionalLight)
                {
                    ImGui.Image((IntPtr) ((DirectionalLight) light).ShadowMap, new System.Numerics.Vector2(256, 256));
                }
                else if (light is PointLight)
                {
                    Window.Instance.GL.Enable(EnableCap.TextureCubeMap);
                    Window.Instance.GL.ActiveTexture(TextureUnit.Texture0);
                    Window.Instance.GL.BindTexture(TextureTarget.TextureCubeMap, light.ShadowMap);
                    
                    // WE have to extract the data using GL.GetTexImage
                    for (int i = 0; i < 6; i++)
                    {
                        byte[] data = new byte[(int)light.ShadowMapSize.X * (int)light.ShadowMapSize.Y * (int)3];
                        
                        fixed (byte* ptr = data)
                        {
                            Window.Instance.GL.GetTexImage(TextureTarget.TextureCubeMapPositiveX + i, 0, PixelFormat.Rgb, PixelType.UnsignedByte,(void*) ptr);
                        }
                        
                        var img = Window.Instance.GL.GenTexture();
                        Window.Instance.GL.BindTexture(TextureTarget.Texture2D, img);
                        fixed (byte* ptr = data)
                        {
                            Window.Instance.GL.TexImage2D(TextureTarget.Texture2D, 0, (int) InternalFormat.Rgb, (uint) light.ShadowMapSize.X, (uint) light.ShadowMapSize.Y, 0, PixelFormat.Rgb, PixelType.UnsignedByte, ptr);
                        }
                        
                        Window.Instance.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
                        Window.Instance.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                        Window.Instance.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                        Window.Instance.GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                        
                        ImGui.Image((IntPtr) img, new System.Numerics.Vector2(256, 256));
                        
                        Window.Instance.GL.DeleteTexture(img);
                    }
                }
            }
            
        }
        
        ImGui.End();
    }
}