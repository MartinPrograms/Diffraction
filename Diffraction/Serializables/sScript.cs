using Diffraction.Rendering;
using Diffraction.Scripting;
using Object = Diffraction.Rendering.Objects.Object;

namespace Diffraction.Serializables;

public class sScript
{
    // the lua scripts are a bit of a pain to serialize, so we just serialize the name of the script, and some in and out parameters
    public string ScriptName;
    public sObject Parent;
    
    public sScript(string scriptName, sObject parent)
    {
        ScriptName = scriptName;
        Parent = parent;
    }
    
    public Dictionary<string, object> InParameters = new();
    
    private Script _script;
    
    public Script GetScript()
    {
        if (_script == null)
        {
            _script = Script.Load(ScriptName, Parent);
        }
        return _script;
    }
    
    public void Set(string name, object value)
    {
        GetScript().Set(name, value);
        InParameters[name] = value;
    }
    
    public void Update(double deltaTime)
    {
        GetScript().Update(deltaTime);
    }
}