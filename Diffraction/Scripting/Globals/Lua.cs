using System.Numerics;
using Diffraction.Rendering;
using Diffraction.Rendering.Meshes;
using Silk.NET.Input;

namespace Diffraction.Scripting.Globals;

public class Lua : EventObject
{
    private NLua.Lua _state;
    
    public Lua()
    {
        _state = new NLua.Lua();
        _state.LoadCLRPackage ();
    }
    
    public object[] Run(string code)
    {
        return _state.DoString(code);
    }

    public object[] RunFile(string path)
    {
        return _state.DoFile(path);
    }
    
    public void Set(string name, object value)
    {
        _state[name] = value;
    }
    
    public object Get(string name)
    {
        return _state[name];
    }

    public override void Update(double deltaTime)
    { 
        
    }
    
    public void Dispose()
    {
        _state.Dispose();
    }
}

public class ExposeToLua : Attribute
{
    public string Name;
    public bool IsGlobal;
    
    public ExposeToLua(string name)
    {
        Name = name;
        IsGlobal = false;
    }

    public ExposeToLua(string name, bool isGlobal)
    {
        Name = name;
        IsGlobal = isGlobal;
    }
}

