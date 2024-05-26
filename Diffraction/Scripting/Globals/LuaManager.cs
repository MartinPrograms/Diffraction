using System.Reflection;

namespace Diffraction.Scripting.Globals;

public class LuaManager
{
    private static LuaManager _instance;
    public static LuaManager Instance => _instance ??= new LuaManager();

    private LuaManager()
    {
    }

    public List<Script> LuaScripts = new();

    public void Add(Script lua)
    {
        LuaScripts.Add(lua);
    }
    
    public void Remove(Script lua)
    {
        LuaScripts.Remove(lua);
    }

    public static void SetGlobal(string name, object value)
    {
        foreach (var lua in Instance.LuaScripts)
        {
            lua.Set(name, value, true);
        }
    }
    
    public static void ScanGlobals()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in types)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);
             
            foreach (var field in fields)
            {
                var attribute = field.GetCustomAttribute<ExposeToLua>();
                if (attribute != null)
                {
                    if (attribute.IsGlobal)
                    {
                        SetGlobal(attribute.Name, field.GetValue(null));
                        Console.WriteLine($"Set global {attribute.Name} to {field.Name}");
                    }
                }
            }
        }
    }
}