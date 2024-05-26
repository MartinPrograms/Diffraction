using Diffraction.Scripting.Globals;
using ImGuiNET;

namespace Diffraction.Rendering.Specials.Lighting;

public class LightUI : EventObject
{
    private int selectedShadowFrameBuffer = -1;

    public override void Render(Camera camera)
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
                ImGui.Image(light.GetShadowMap(), new System.Numerics.Vector2(256, 256));
            }
            
        }
        
        ImGui.End();
    }
}