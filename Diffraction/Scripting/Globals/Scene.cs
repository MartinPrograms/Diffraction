using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Diffraction.Rendering;
using Diffraction.Rendering.Meshes;

using Newtonsoft.Json;

using Object = Diffraction.Rendering.Objects.Object;

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

    private JsonSerializerSettings _jsonSettings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All,
        Formatting = Formatting.Indented,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        PreserveReferencesHandling = PreserveReferencesHandling.Objects
    };

    public void Play()
    {
        // Convert Objects to JSON
        string json = JsonConvert.SerializeObject(Objects, _jsonSettings);
        
        // Write JSON to file
        System.IO.File.WriteAllText(_backupPath, json);
        
        _isPlaying = true;
    }
     

    public void Stop()
    {
        _isPlaying = false;

        // Convert JSON to Objects
        string json = System.IO.File.ReadAllText(_backupPath);
        Objects = JsonConvert.DeserializeObject<List<Object>>(json, _jsonSettings);
        Reload?.Invoke();
        _started = false;
        
    }
    
    public ObjectScene(string workingDirectory)
    {
        Environment.CurrentDirectory = workingDirectory;
        Instance = this;
    }
    
    public List<Rendering.Objects.Object> Objects = new();
    
    public void AddObject(Rendering.Objects.Object obj)
    {
        Objects.Add(obj);
    }
    
    public void RemoveObject(Rendering.Objects.Object obj)
    {
        Objects.Remove(obj);
    }
    
    public override void Render(Camera camera)
    {
        foreach (Rendering.Objects.Object obj in Objects)
        {
            obj.Render(camera);
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

    public Object GetObject(Guid id)
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
        _isPlaying = false;
        _started = false;
        Objects.Clear();
        
        string json = System.IO.File.ReadAllText(_backupPath);
        Objects = JsonConvert.DeserializeObject<List<Object>>(json, _jsonSettings);
        Reload?.Invoke();
        
        Reload?.Invoke();
    }
}