using System.Numerics;
using Diffraction.Rendering.GUI.Text;
using Diffraction.Rendering.Windowing;

namespace Diffraction.Rendering.GUI.Interactables;

public class Button
{
    private Text.Text _text;
    private Rectangle _rectangle;
    
    public Button(Text.Text text, Rectangle rectangle)
    {
        _text = text;
        _rectangle = rectangle;
    }
    
    public void Render()
    {
        RectangleRenderer.Render(_rectangle);
        _text.Render(Camera.MainCamera);
    }
    
    public bool Update()
    {
        var mouse = Input.Input.MousePosition;
        
        // Position 0,0 = center for the rectangle, so we have to adjust the mouse position, where x=1 is the right side of the screen and y=1 is the top of the screen
        var adjustedMouse = new Vector2((mouse.X / Window.Instance.IWindow.Size.X) * 2 - 1, (mouse.Y / Window.Instance.IWindow.Size.X) * 2 - 1);
        
        if (adjustedMouse.X > _rectangle.Position.X - _rectangle.Size.X / 2 && adjustedMouse.X < _rectangle.Position.X + _rectangle.Size.X / 2 &&
            adjustedMouse.Y > _rectangle.Position.Y - _rectangle.Size.Y / 2 && adjustedMouse.Y < _rectangle.Position.Y + _rectangle.Size.Y / 2)
        {
            Console.WriteLine("Zoowie mama! You hovering the button!");
            return true;
        }
        
        return false;
    }
}