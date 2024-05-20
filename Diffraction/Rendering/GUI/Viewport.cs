using System.Numerics;
using Diffraction.Rendering.Buffers;
using Diffraction.Rendering.Meshes;
using ImGuiNET;
using Silk.NET.Maths;
using SilkyGizmos;

namespace Diffraction.Rendering.GUI;

public class Viewport : EventObject
{
    private RenderTexture _texture;
    private Camera _camera;
    public Viewport(RenderTexture texture, Camera camera)
    
    {
        _camera = camera;
        _texture = texture;
    }

    public override void Render(Camera camera)
    {
        ImGui.Begin("Viewport");
        var size = ImGui.GetContentRegionAvail();
        if (size.X > 0 && size.Y > 0)
        {
            _camera.Resolution = new Vector2D<int>((int)size.X, (int)size.Y);
            _camera.AspectRatio = (float)size.X / size.Y;
            SilkyGizmos.Gizmos.SetResolution((int)size.X, (int)size.Y);
           
            _texture.SetSize((int)size.X, (int)size.Y);
            _texture.ImBind();
            
            
            ImGui.Image((IntPtr)_texture.Texture, size, new Vector2(0, 1), new Vector2(1, 0));
            _texture.ImUnbind();
        }

        ImGui.End();
    }
}