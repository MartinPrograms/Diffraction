using System.Numerics;
using Diffraction.Rendering;
using Diffraction.Rendering.Meshes;
using Silk.NET.Input;

namespace Diffraction.Scripting.Globals;

public class Lua : EventObject
{
    private NLua.Lua State;
    
    public Lua()
    {
        State = new NLua.Lua();
        State.LoadCLRPackage ();
        State.RegisterLuaClassType(typeof(Vector3), typeof(Vector3));
        State.RegisterLuaClassType(typeof(Quaternion), typeof(Quaternion));
    }
    
    public object[] Run(string code)
    {
        return State.DoString(code);
    }

    public object[] RunFile(string path)
    {
        return State.DoFile(path);
    }
    
    public void Set(string name, object value)
    {
        State[name] = value;
    }
    
    public object Get(string name)
    {
        return State[name];
    }

    public override void Update(double deltaTime)
    { 
        
    }
    
    public void Dispose()
    {
        State.Dispose();
    }
}

public class ExposeToLua : Attribute
{
    public string Name;
    
    public ExposeToLua(string name)
    {
        Name = name;
    }
}

