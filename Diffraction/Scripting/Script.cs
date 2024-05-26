using System.Collections;
using System.Numerics;
using System.Reflection;
using Diffraction.Rendering;
using Diffraction.Scripting.Globals;
using Diffraction.Serializables;
using Newtonsoft.Json;
using NLua;
using Lua = Diffraction.Scripting.Globals.Lua;
using Object = Diffraction.Rendering.Objects.Object;

namespace Diffraction.Scripting;

public class Script : EventObject
{
    [JsonIgnore]
    public string Code;
    public string Path;
    public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
    
    [JsonProperty]
    private sObject _parent;

    [JsonIgnore]
    public sObject Parent
    {
        set{ _parent = value; }
        get{ return _parent; }
    }
    
    private Lua _lua;

    private LuaFunction _start;
    private LuaFunction _update;
    
    public static Script Load(string path, sObject parent)
    {
        return ScriptUtils.Load(path, parent);
    }
    
    public Script(string code, string path, sObject parent)
    {
        Path = path;
        _parent = parent;
        Setup(code);
    }

    private void Setup(string code)
    {
        Code = code;
        _lua = new Lua();
        
        _lua.Run(File.ReadAllText(Path));
        
        _start = (LuaFunction)_lua.Get("Start");
        _update = (LuaFunction)_lua.Get("Update");
        
        LuaManager.Instance.Add(this);
    }

    bool _started = false;

    public Dictionary<string, object> Parameters = new();
    
    public override unsafe void Update(double time)
    {
        if (!runEvents) return;
        try
        {
            if (!_started)
            {
                _start?.Call();
                _started = true;
            }

            SetParameters();

            _lua.Update(time);
            _update?.Call();
            
            ReceiveParameters();
            
            AfterUpdate?.Invoke();
        }catch(Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private void ReceiveParameters()
    {
        foreach (var parameter in Parameters)
        {
            Parameters[parameter.Key] = Get(parameter.Key);
        }
    }

    private void SetParameters()
    {
        {
            // Values based on the parent:
            var parent = _parent.GetObject();
            // Get all the fields, and if they have the ExposeToLua attribute, set them
            foreach (var field in parent.GetType().GetFields())
            {
                if (field.GetCustomAttribute<ExposeToLua>() != null)
                {
                    Set(field.Name, field.GetValue(parent));
                    Parameters[field.Name] = field.GetValue(parent);
                }
            }
            
            // Get all the functions, and if they have the ExposeToLua attribute, set them
            foreach (var method in parent.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public))
            {
                if (method.GetCustomAttribute<ExposeToLua>() != null)
                {
                    Set(method.Name, method);
                    Parameters[method.Name] = method;
                }
            }
            
            // Get all the properties, and if they have the ExposeToLua attribute, set them
            foreach (var property in parent.GetType().GetProperties())
            {
                if (property.GetCustomAttribute<ExposeToLua>() != null)
                {
                    Set(property.Name, property.GetValue(parent));
                    Parameters[property.Name] = property.GetValue(parent);
                }
            }
            
            Set("this", parent);
            Parameters["this"] = parent;
        }
    }
    
    
    public void Set(string name, object value, bool isGlobal = false)
    {
        _lua.Set(name, value);
        
        if (isGlobal)
            Parameters[name] = value;
    }
    
    public void Dispose()
    {
        _start.Dispose();
        _update.Dispose();
        
        _lua.Dispose();
        
        LuaManager.Instance.Remove(this);
    }
    
    public void Declare(string name, object value)
    {
        // Q: In h
        _lua.Set(name, value);
    }
    
    public object Get(string name)
    {
        return _lua.Get(name);
    }
    
    public T Get<T>(string name)
    {
        return (T)_lua.Get(name);
    }
    
    public event Action AfterUpdate;
    
    private bool runEvents = true;

    public void Reload(string code)
    {
        ObjectScene.Instance.ReloadScene();
    }
    public Dictionary<string, object> GetVariables()
    {
        return Parameters;
    }
}
