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
            }
            ImGui.DragFloat("Shadow Near", ref light.ShadowNear, 0.01f);
            ImGui.DragFloat("Shadow Far", ref light.ShadowFar, 0.01f, 1f, 1000f);

            if (light is PointLight)
            {
                PointLight pointLight = (PointLight) light;
                ImGui.DragFloat("Shadow Fall Off", ref pointLight.ShadowFallOff, 0.01f);
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
                    // idfk how to render a cube map, seriously
                    // they're black magic
                }
            }
            
        }
        
        ImGui.End();
    }
}