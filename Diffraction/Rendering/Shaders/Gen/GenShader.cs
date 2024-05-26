using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using ShaderGen;
using ShaderGen.Glsl;

namespace Diffraction.Rendering.Shaders.Gen;

public class GenShader
{
    public string Name { get; set; }
    public string VertexSource { get; set; }
    public string FragmentSource { get; set; }

    public static GenShader FromClass(string name, string shaderClassSource, string vsName = null, string fsName = null, string csName = null)
    {
        var now = DateTime.Now;
        var compilation = CSharpCompilation.Create("ShaderGenTest")
            .WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
            .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(Vector3).Assembly.Location))
            .AddReferences(MetadataReference.CreateFromFile(typeof(ShaderGen.VertexShaderAttribute).Assembly.Location))
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(SourceText.From(shaderClassSource, Encoding.UTF8)));
        
        ShaderGen.Glsl.Glsl450Backend backend = new ShaderGen.Glsl.Glsl450Backend(compilation);

        if (!string.IsNullOrEmpty(vsName)) vsName = vsName.Insert(0, "Shader." + name + ".");
        if (!string.IsNullOrEmpty(fsName)) fsName = fsName.Insert(0, "Shader." + name + ".");
        if (!string.IsNullOrEmpty(csName)) csName = csName.Insert(0, "Shader." + name + ".");
        
        var shader = new ShaderGenerator(compilation, backend, vsName, fsName, csName);
        var shaderGenerationResult = shader.GenerateShaders();
        var shaderOutput = shaderGenerationResult.GetOutput(backend);

        var shaderSet = shaderOutput.First();

        string vsSource = "";
        if (!string.IsNullOrEmpty(vsName))
        {
            vsSource = shaderSet.VertexShaderCode;
        }
        
        string fsSource = "";
        if (!string.IsNullOrEmpty(fsName))
        {
            fsSource = shaderSet.FragmentShaderCode;
        }
        
        string csSource = "";
        if (!string.IsNullOrEmpty(csName))
        {
            csSource = shaderSet.ComputeShaderCode;
        }
        
        if (vsSource.Contains("gl_Position.y = -gl_Position.y;"))
        {
            vsSource = vsSource.Replace("gl_Position.y = -gl_Position.y;", ""); // For some reason, the shader generator adds this line, which is not needed
        }
        
        var finish = DateTime.Now - now;
        Console.WriteLine($"Shader generation took {finish.TotalMilliseconds}ms");
        
        return new GenShader
        {
            VertexSource = vsSource,
            FragmentSource = fsSource,
            Name = name
        };
        
        return null;
    }
}