using System.Numerics;
using Silk.NET.Input;
using Silk.NET.Input.Extensions;
using Silk.NET.Maths;

namespace Diffraction.Input;

public static class Input
{
    private static List<Key> _previousFrameKeys = new();
    private static List<Key> _currentFrameKeys = new();
    private static List<MouseButton> _previousFrameMouseButtons = new();
    private static List<MouseButton> _currentFrameMouseButtons = new();
    private static Vector2 _previousFrameMousePosition;
    private static Vector2 _currentFrameMousePosition;
    private static Vector2 _mouseDelta = Vector2.Zero;
    
    private static Vector2 _previousMouseScroll = Vector2.Zero;
    private static Vector2 _mouseScroll = Vector2.Zero;
    
    public static Vector2 MouseDelta => _mouseDelta;
    public static Vector2 MousePosition => _currentFrameMousePosition;
    
    private static IInputContext _input;

    public static void Start(IInputContext input)
    {
        input.Mice[0].Scroll += (sender, args) =>
        {
            _previousMouseScroll = _mouseScroll;
            _mouseScroll = new Vector2(args.X, args.Y);
        };
    }
    
    public static bool GetKey(Key key)
    {
        return _currentFrameKeys.Contains(key);
    }
    
    
    public static bool GetKeyDown(Key key)
    {
        return _currentFrameKeys.Contains(key) && !_previousFrameKeys.Contains(key);
    }
    
    public static bool GetKeyUp(Key key)
    {
        return !_currentFrameKeys.Contains(key) && _previousFrameKeys.Contains(key);
    }
    
    public static bool GetMouseButton(MouseButton mouseButton)
    {
        return _currentFrameMouseButtons.Contains(mouseButton);
    }
    
    
    public static bool GetMouseButtonDown(MouseButton mouseButton)
    {
        return _currentFrameMouseButtons.Contains(mouseButton) && !_previousFrameMouseButtons.Contains(mouseButton);
    }
    
    public static bool GetMouseButtonUp(MouseButton mouseButton)
    {
        return !_currentFrameMouseButtons.Contains(mouseButton) && _previousFrameMouseButtons.Contains(mouseButton);
    }

    public static Vector2 GetMousePosition()
    {
        return new Vector2(_currentFrameMousePosition.X, _currentFrameMousePosition.Y);
    }
    
    public static Vector2 GetMouseDelta()
    {
        return new Vector2(_mouseDelta.X, _mouseDelta.Y);
    }
    
    public static Vector2 GetMouseScrollDelta()
    {
        return new Vector2(_mouseScroll.X, _mouseScroll.Y);
    }
    
    static bool isMouseLocked = false;

    public static bool IsMouseLocked()
    {
        return isMouseLocked;
    }

    public static void ToggleMouseLock()
    {
        isMouseLocked = !isMouseLocked;
        if (isMouseLocked)
        {
            _input.Mice[0].Cursor.CursorMode = CursorMode.Raw;
        }
        else
        {
            _input.Mice[0].Cursor.CursorMode = CursorMode.Normal;
        }
    }

    public static void SetInput(IInputContext input)
    {
        _previousFrameKeys = _currentFrameKeys;
        _currentFrameKeys = input.Keyboards[0].CaptureState().GetPressedKeys().ToArray().ToList();
        
        _previousFrameMouseButtons = _currentFrameMouseButtons;
        _currentFrameMouseButtons = input.Mice[0].CaptureState().GetPressedButtons().ToArray().ToList();
        
        _previousFrameMousePosition = _currentFrameMousePosition;
        _currentFrameMousePosition = input.Mice[0].CaptureState().Position;
        
        _mouseDelta = _currentFrameMousePosition - _previousFrameMousePosition;
        
        _input = input;
    }

    public static void SetCursorMode(CursorMode mode)
    {
        if (_input == null) return;
        _input.Mice[0].Cursor.CursorMode = mode;
    }
}