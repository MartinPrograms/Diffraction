using System.Numerics;
using Diffraction.Audio;
using Diffraction.Rendering.GUI;
using Diffraction.Rendering.GUI.Text;
using Diffraction.Scripting.Globals;
using Silk.NET.GLFW;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace Diffraction.Rendering.Windowing;

public class Window
{
    public static Window Instance;
    
    private IWindow _window;
    private GL _gl;
    private IInputContext _input;
    private ImGuiInstance _imgui;
    
    public GL GL => _gl;
    public IWindow IWindow => _window;
    public IInputContext Input => _input;
    public int MainThreadID;

    public Action Open;
    public Action <double>Update;
    public Action <double>Render;
    public Action <double> LateRender;
    public Action Close;
    public Action<Vector2D<int>> Resize;
    
    internal Audio.Audio Audio;
    
    public unsafe Window(WindowOptions options)
    {
        MainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
        _window = Silk.NET.Windowing.Window.Create(options);
   
        _window.Load += () =>
        {
            _gl = GL.GetApi(_window);
            _input = _window.CreateInput();
            _imgui = new ImGuiInstance(this);
            
            _gl.Enable(EnableCap.DepthTest);
            
            TextRenderer.Init(_gl);
            TextRenderer.LoadFont(_gl, "Ruda", "Editor/Fonts/ruda-variable.ttf", 14);
         
            Audio = new Audio.Audio();
            Audio.Init(false);

            var file = new AudioFile("Audio/open.wav", false);
            Audio.AudioFiles.Add(file);
            
            Open?.Invoke();
        };
        
        _window.Update += (time) =>
        {
            Update?.Invoke(time);
            
            Diffraction.Input.Input.SetInput(Input);
            
            Time.Update(time);
        };
        
        _window.Render += (time) =>
        {
            _gl.Clear((uint)ClearBufferMask.ColorBufferBit | (uint)ClearBufferMask.DepthBufferBit);
            _gl.ClearColor(0.2f, 0.2f,0.4f, 1.0f);

            Render?.Invoke(time);
            
            LateRender?.Invoke(time);
        };
        
        _window.Resize += (size) =>
        {
            Resize?.Invoke(size);
            
            _gl.Viewport(0, 0, (uint)size.X, (uint)size.Y);
        };
        
        _window.Closing += () =>
        {
            Audio.Exit();
            Close?.Invoke();
        };
        
        Instance = this;
    }

    public static Window Create(WindowOptions options)
    {
        return new Window(options);
    }
    
    public void Run()
    {
        _window.Run();
    }

    public void SetWindowState(WindowState maximized)
    {
        _window.WindowState = maximized;
    }
    private bool _fullscreen = false;
    public void ToggleFullscreen()
    {
        if (_fullscreen)
        {
            _window.WindowState = WindowState.Normal;
            _fullscreen = false;
        }
        else
        {
            _window.WindowState = WindowState.Fullscreen;
            _fullscreen = true;
        }
    }
}