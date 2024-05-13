using System.Numerics;
using Diffraction.Rendering.Meshes;
using Diffraction.Rendering.Windowing;

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
    private Vector3 _color;
    
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
    
    public Vector3 Color
    {
        get => _color;
        set => _color = value;
    }
    
    public Text(string text, string font, float size, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, float x, float y, Vector3 color)
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
        _color = new Vector3(1, 1, 1);
    }
    
    public override void Render(Camera camera)
    {
        TextRenderer.RenderText(Window.Instance.GL, _font, _text, _x, _horizontalAlignment, _y, _verticalAlignment, _size, _color);
    }
}