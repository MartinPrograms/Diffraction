using Diffraction.Rendering.GUI.Text;
using Diffraction.Rendering.Meshes;
using Diffraction.Rendering.Windowing;
using Diffraction.Scripting.Globals;

namespace Diffraction.Rendering.GUI;

public class Stats : EventObject
{
    private Text.Text _text;
    public Stats()
    {
        _text = new Text.Text("Stats", "Ruda", 1, HorizontalAlignment.Left, VerticalAlignment.Top, 10,-10, new System.Numerics.Vector3(1,1,1));
        _text.TextValue = "Stats: 0 FPS";
    }
    public override void Render(Camera camera)
    {
        var fps = 1.0 / Time.DeltaTime;
        string fpsString = fps.ToString("0.00");
        _text.TextValue = $"Stats: {fpsString} FPS";
        _text.Render(camera);
    }
}