using System.Numerics;
using Diffraction.Rendering.Meshes;
using Diffraction.Rendering.Windowing;
using Diffraction.Scripting.Globals;

namespace Diffraction.Rendering.GUI.Text;

public class Text : EventObject
{
    //             TextRenderer.RenderText(_gl, "Ruda", "Hello, world!", 0, HorizontalAlignment.Left, 0, VerticalAlignment.Top, 1, new Vector3(1, 1, 1));

    private string _text;
    private string _font;
    private float _size;
    private HorizontalAlignment _horizontalAlignment;
    private VerticalAlignment _verticalAlignment;
    private float _x;
    private float _y;
    private Vector4 _color;
    
    public string TextValue
    {
        get => _text;
        set => _text = value;
    }
    
    public string Font
    {
        get => _font;
        set => _font = value;
    }
    
    public float Size
    {
        get => _size;
        set => _size = value;
    }
    
    public HorizontalAlignment HorizontalAlignment
    {
        get => _horizontalAlignment;
        set => _horizontalAlignment = value;
    }
    
    public VerticalAlignment VerticalAlignment
    {
        get => _verticalAlignment;
        set => _verticalAlignment = value;
    }
    
    public float X
    {
        get => _x;
        set => _x = value;
    }
    
    public float Y
    {
        get => _y;
        set => _y = value;
    }
    
    public Vector4 Color
    {
        get => _color;
        set => _color = value;
    }
    
    public Text(string text, string font, float size, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, float x, float y, Vector4 color)
    {
        _text = text;
        _font = font;
        _size = size;
        _horizontalAlignment = horizontalAlignment;
        _verticalAlignment = verticalAlignment;
        _x = x;
        _y = y;
        _color = color;
    }

    public Text(string text)
    {
        _text = text;
        _font = TextRenderer.DefaultFont;
        _size = 1;
        _horizontalAlignment = HorizontalAlignment.Left;
        _verticalAlignment = VerticalAlignment.Top;
        _x = 25;
        _y = 25;
        _color = new Vector4(1, 1, 1,1);
    }
    
    public override void Render(Camera camera)
    {
        TextRenderer.RenderText(Window.Instance.GL, _font, _text, _x, _horizontalAlignment, _y, _verticalAlignment, _size, _color);
    }
}

public static class StaticText
{
    public static void RenderText(string text)
    {
        Window.Instance.LateRenderQueue += d =>
        {
            TextRenderer.RenderText(Window.Instance.GL, TextRenderer.DefaultFont, text, 0, HorizontalAlignment.Center, 0, VerticalAlignment.Center, 1, new Vector4(1, 1, 1,1));
        };
    }
    
    public static void RenderText(string text, Vector2 position)
    {
        Window.Instance.LateRenderQueue += d =>
        {
            TextRenderer.RenderText(Window.Instance.GL, TextRenderer.DefaultFont, text, position.X, HorizontalAlignment.Center, position.Y, VerticalAlignment.Center, 1, new Vector4(1, 1, 1,1));
        };
    }
    
    public static void RenderText(string text, Vector2 position, float scale)
    {
        Window.Instance.LateRenderQueue += d =>
        {
            TextRenderer.RenderText(Window.Instance.GL, TextRenderer.DefaultFont, text, position.X, HorizontalAlignment.Center, position.Y, VerticalAlignment.Center, scale, new Vector4(1, 1, 1,1));
        };
    }
    
    public static void RenderText(string text, Vector2 position, float scale, Vector4 color)
    {
        Window.Instance.LateRenderQueue += d =>
        {
            TextRenderer.RenderText(Window.Instance.GL, TextRenderer.DefaultFont, text, position.X, HorizontalAlignment.Center, position.Y, VerticalAlignment.Center, scale, color);
        };
    }
    
    public static void RenderText(string text, Vector2 position, float scale, Vector4 color, HorizontalAlignment horizontalAlignment)
    {
        Window.Instance.LateRenderQueue += d =>
        {
            TextRenderer.RenderText(Window.Instance.GL, TextRenderer.DefaultFont, text, position.X, horizontalAlignment, position.Y, VerticalAlignment.Center, scale, color);
        };
    }
    
    public static void RenderText(string text, Vector2 position, float scale, Vector4 color, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
    {
        Window.Instance.LateRenderQueue += d =>
        {
            TextRenderer.RenderText(Window.Instance.GL, TextRenderer.DefaultFont, text, position.X, horizontalAlignment, position.Y, verticalAlignment, scale, color);
        };
    }
}