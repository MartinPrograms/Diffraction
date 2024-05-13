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
    public string Code;
    public string Path;
    
    [JsonProperty]
    private sObject _parent;
    
    private Lua _lua;

    private LuaFunction _start;
    private LuaFunction _update;
    
    public static Script Load(string path, sObject parent)
    {
        return new Script(System.IO.File.ReadAllText(path)){Path = path, _parent = parent};
    }
    
    public Script(string code)
    {
        Code = code;
        _lua = new Lua();
        
        _lua.Run(code);
        
        _start = (LuaFunction)_lua.Get("Start");
        _update = (LuaFunction)_lua.Get("Update");
    }
    
    bool _started = false;

    public Dictionary<string, object> Parameters = new();
    
    public override unsafe void Update(double time)
    {
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
        var parent = _parent.GetObject();
        var allFields = parent.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
        var fields = new List<Tuple<string, FieldInfo>>();
        foreach (var field in allFields)
        {
            if (field.GetCustomAttribute<ExposeToLua>() != null)
            {
                fields.Add(new Tuple<string, FieldInfo>(field.GetCustomAttribute<ExposeToLua>().Name, field));
            }
        }
        if (Parameters.Count != fields.Count)
        {
            foreach (var field in fields)
            {
                Parameters[field.Item1] = field.Item2.GetValue(parent);
                Set(field.Item1, field.Item2.GetValue(parent));
            }
        }
    }

    public void Set(string name, object value)
    {
        _lua.Set(name, value);
    }
    
    public void Dispose()
    {
        _lua.Dispose();
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
}