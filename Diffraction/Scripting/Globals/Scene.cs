using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Diffraction.Rendering;
using Diffraction.Rendering.Meshes;
using Diffraction.Rendering.Specials.Lighting;
using Newtonsoft.Json;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using SilkyGizmos;
using JsonConverter = Newtonsoft.Json.JsonConverter;
using Object = Diffraction.Rendering.Objects.Object;
using Window = Diffraction.Rendering.Windowing.Window;
namespace Diffraction.Scripting.Globals;

public class ObjectScene : EventObject
{
    public static ObjectScene Instance;
    public Vector4 Bounds = new Vector4(-100, 100, -100, 100); // xMin, xMax, yMin, yMax

    public Object SelectedObject;

    private bool _isPlaying;
    
    private static string _backupPath = "backup.scene";
    public bool Paused => !_isPlaying;
    public Action Reload;
    
    public List<Rendering.Objects.Object> Objects = new();
    public List<Light> Lights = new();    
    
    private JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All,
        Formatting = Formatting.Indented,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        PreserveReferencesHandling = PreserveReferencesHandling.None,
        Converters = new List<JsonConverter>()
        {
        }
    };

    public void Play()
    {
        // Convert Objects to JSON
        var objects = Objects;
        string json = JsonConvert.SerializeObject(objects, _jsonSettings);
        
        // Write JSON to file
        System.IO.File.WriteAllText(_backupPath, json);
        
        _isPlaying = true;
        LuaManager.ScanGlobals();

    }
     
    public void Stop()
    {
        _isPlaying = false;

        // Convert JSON to Objects
        
        var clonedList = new List<Rendering.Objects.Object>();
        clonedList.AddRange(Objects);
        
        var clonedLights = new List<Light>();
        clonedLights.AddRange(Lights);
        
        foreach (Rendering.Objects.Object obj in clonedList)
        {
            obj.Dispose();
        }
        
        foreach (Light light in clonedLights)
        {
            ObjectScene.Instance.UnregisterLight(light);
        }
        
        string json = System.IO.File.ReadAllText(_backupPath);
        Objects = JsonConvert.DeserializeObject<List<Object>>(json, _jsonSettings);
        
        Reload?.Invoke();
        _started = false;
        LuaManager.ScanGlobals();

        SelectedObject = null;
    }
    
    public ObjectScene(string workingDirectory)
    {
        Environment.CurrentDirectory = workingDirectory;
        Instance = this;
        LuaManager.ScanGlobals();
    }
    
    public void AddObject(Rendering.Objects.Object obj)
    {
        Objects.Add(obj);
    }
    
    public void RemoveObject(Rendering.Objects.Object obj)
    {
        Objects.Remove(obj);
    }

    private OPERATION _operation = OPERATION.SHOW;
    private MODE _mode = MODE.WORLD;
    public override void Render(Camera camera)
    {
        foreach (Rendering.Objects.Object obj in Objects)
        {
            Window.Instance.GL.CullFace(GLEnum.Back);
            obj.Render(camera);
        }
        
        if (Paused)
        {
            
            if (SelectedObject != null)
            {
                unsafe
                {
                    sgTransform model = SelectedObject.Transform.AsSGTransform();               
                    Matrix4x4 projection = camera.GetProjectionMatrix();
                    Matrix4x4 view = camera.GetViewMatrix();
                    
                    Gizmos.SetLineThickness(2);
                    if(Gizmos.Manipulate(view,projection, _operation, _mode, ref model))
                        SelectedObject.Transform = Transform.FromSGTransform(model);
                }
            }
            
            if (Input.Input.GetKeyDown(Key.Number1))
            {
                _operation = OPERATION.TRANSLATE;
            }
            if (Input.Input.GetKeyDown(Key.Number2))
            {
                _operation = OPERATION.ROTATE;
            }
            if (Input.Input.GetKeyDown(Key.Number3))
            {
                _operation = OPERATION.SCALE;
            }
            if (Input.Input.GetKeyDown(Key.Number4))
            {
                _operation = OPERATION.SHOW;
            }
            
            if (Input.Input.GetKeyDown(Key.Number5))
            {
                _mode = _mode == MODE.WORLD ? MODE.LOCAL : MODE.WORLD;
            }
        }
    }
    
    bool _started = false;
    public override void Update(double time)
    {
        if (!_isPlaying)
        {
            // Nothing needs to be updated
            if (!_started)
            {
                Reload?.Invoke();
                _started = true;
            }
            
            return;
        }
        
        foreach (Rendering.Objects.Object obj in Objects)
        {
            obj.Update(time);
        }
        
        List<Object> toRemove = new();
        foreach (Rendering.Objects.Object obj in Objects)
        {
            if (obj.Transform.Position.X < Bounds.X || obj.Transform.Position.X > Bounds.Y || obj.Transform.Position.Y < Bounds.Z || obj.Transform.Position.Y > Bounds.W)
            {
                toRemove.Add(obj);
            }
        }
        
        foreach (Object obj in toRemove)
        {
            Objects.Remove(obj);
        }
    }

    public void Dispose()
    {
        foreach (Object obj in Objects)
        {
            obj.Dispose();
        }
    }

    public Object? GetObject(Guid id)
    {
        foreach (Object obj in Objects)
        {
            if (obj.Id == id)
            {
                return obj;
            }
        }

        return null;
    }

    public void ReloadScene()
    {
        this.Play();
        this.Stop();
        this.Play();
        this.Stop();
    }

    public void RegisterLight(Light light)
    {
        Lights.Add(light);
    }

    public void RenderLighting(Camera camera)
    {  
        foreach (Light light in Lights)
        {
            light.Render(camera);
        }
    }

    public Object FindObject(string @object)
    {
        foreach (Object obj in Objects)
        {
            if (obj.Name == @object)
            {
                return obj;
            }
        }

        return null;
    }

    public void UnregisterLight(Light light)
    {
        Lights.Remove(light);
    }
}