using Diffraction.Rendering.Shaders.Gen;
using Diffraction.Serializables;

namespace Diffraction.Rendering.Shaders;

public class ShaderUtils
{
    private static Dictionary<string, Shader> _shaders = new Dictionary<string, Shader>();
    public static Dictionary<string, Shader> Shaders => _shaders;
    
    public static Shader CreateShader(string name,string vertexSource, string fragmentSource, string? computeSource = null, string? geometrySource = null)
    {
        return new Shader(name, vertexSource, fragmentSource, computeSource, geometrySource);
    }

    public static Shader GenShaderFromClass(GenShader shader)
    {
        return new Shader(shader.Name, shader.VertexSource, shader.FragmentSource);
    }

    public static void AddShader(GenShader shader)
    {
        _shaders.Add(shader.Name, GenShaderFromClass(shader));
    }
    public static void AddShader(Tuple<string,string?, string?, string?, string?> shader)
    {
        _shaders.Add(shader.Item1, CreateShader(shader.Item1, shader.Item2, shader.Item3, shader.Item4, shader.Item5));
        Console.WriteLine($"Added shader {shader.Item1}");
    }

    public static Shader GetShader(string shaderName)
    {
        if (_shaders.TryGetValue(shaderName, out Shader shader))
        {
            return shader;
        }
        throw new Exception($"Shader {shaderName} not found");
    }
    
    public static Shader GetShader(sShader shader)
    {
        return GetShader(shader.ShaderName);
    }

    public static void AddWatcher(string fileLocation, string shaderName)
    {
        FileSystemWatcher watcher = new FileSystemWatcher();
        watcher.Path = Path.GetDirectoryName(fileLocation);
        watcher.Filter = Path.GetFileName(fileLocation);
        watcher.NotifyFilter = NotifyFilters.LastWrite;
        watcher.Changed += (sender, args) =>
        {
            Console.WriteLine($"Reloading shader {shaderName}");
            Shader shader = GetShader(shaderName);
            shader.Update(File.ReadAllText(fileLocation), shader.GetFragmentSource());
        };
        watcher.EnableRaisingEvents = true;
        
        Console.WriteLine($"Added watcher for {fileLocation}");
    }

    public static void AddWatcher(string shaderName)
    {
        Shader shader = GetShader(shaderName);
        AddWatcher(shader.GetVertexLocation(), shaderName);
        AddWatcher(shader.GetFragmentLocation(), shaderName);
    }

    public static void RecompileAll()
    {
        foreach (var shader in _shaders)
        {
            shader.Value.Recompile();
        }
    }
}