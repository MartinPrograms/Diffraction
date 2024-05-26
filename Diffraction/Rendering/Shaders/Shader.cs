using System.Collections;
using System.Numerics;
using Diffraction.Rendering.Windowing;
using Diffraction.Scripting.Globals;
using Silk.NET.OpenGL;

namespace Diffraction.Rendering.Shaders;

public class Shader : EventObject
{
    private uint _id;
    private string _vertexSource;
    private string _fragmentSource;
    private string _computeSource;
    private string _geometrySource;
    
    private string _vertexLocation;
    private string _fragmentLocation;
    private string _computeLocation;
    private string _geometryLocation;

    private GL _gl;
    
    private ShaderType _type;
    public ShaderType Type => _type;
    public string Name { get; set; }
    
    private List<ShaderAttribute> _inputs = new();
    public ShaderAttribute[] Inputs { get => _inputs.ToArray(); }
    
    private List<ShaderAttribute> _uniforms = new();
    public ShaderAttribute[] Uniforms { get => _uniforms.ToArray(); }

    private string _errors;
    public Shader(string name, string vertexSource = null, string fragmentSource = null, string computeSource = null, string geometrySource = null)
    {
        Create(name, vertexSource, fragmentSource, computeSource, geometrySource);
    }

    private void Create(string name, string vertexSource, string fragmentSource, string computeSource, string geometrySource)
    {
        Name = name;
        _gl = Window.Instance.GL;
        
        if (File.Exists(vertexSource))
        {
            _vertexLocation = vertexSource;
            vertexSource = File.ReadAllText(vertexSource);
            
        }
        else if (vertexSource != null)
        {
            Console.WriteLine($"Vertex shader {Name} not found, parsing as source");
        }
        
        if (File.Exists(fragmentSource))
        {
            _fragmentLocation = fragmentSource;
            fragmentSource = File.ReadAllText(fragmentSource);
        }
        else if (fragmentSource != null)
        {
            Console.WriteLine($"Fragment shader {Name} not found, parsing as source");
        }

        if (File.Exists(computeSource))
        {
            _computeLocation = computeSource;
            computeSource = File.ReadAllText(computeSource);
           
        }
        else if (computeSource != null)
        {
            Console.WriteLine($"Compute shader {Name} not found, parsing as source");
        }
        
        if (File.Exists(geometrySource))
        {
            _geometryLocation = geometrySource;
            geometrySource = File.ReadAllText(geometrySource);
        }
        else if (geometrySource != null)
        {
            Console.WriteLine($"Geometry shader {Name} not found, parsing as source");
        }
        
        _vertexSource = vertexSource;
        _fragmentSource = fragmentSource;
        _computeSource = computeSource;
        _geometrySource = geometrySource;
        
        if (vertexSource != null)
            _type |= ShaderType.Vertex;
        if (fragmentSource != null)
            _type |= ShaderType.Fragment;
        if (computeSource != null)
            _type |= ShaderType.Compute;
        if (geometrySource != null)
            _type |= ShaderType.Geometry;
        
        if (_type == 0)
        {
            throw new Exception("Shader must have at least one type");
        }
        
        Compile();
    }

    public void Compile()
    {
        _id = _gl.CreateProgram();
        uint vertexShader = 0;
        uint fragmentShader = 0;
        uint computeShader = 0;
        uint geometryShader = 0;
        if (_type.HasFlag(ShaderType.Vertex))
        {
            vertexShader = _gl.CreateShader(GLEnum.VertexShader);
            _gl.ShaderSource(vertexShader, _vertexSource);
            _gl.CompileShader(vertexShader);
            _gl.AttachShader(_id, vertexShader);
        }
        
        if (_type.HasFlag(ShaderType.Fragment))
        {
            fragmentShader = _gl.CreateShader(GLEnum.FragmentShader);
            _gl.ShaderSource(fragmentShader, _fragmentSource);
            _gl.CompileShader(fragmentShader);
            _gl.AttachShader(_id, fragmentShader);
        }
        
        if (_type.HasFlag(ShaderType.Compute))
        {
            computeShader = _gl.CreateShader(GLEnum.ComputeShader);
            _gl.ShaderSource(computeShader, _computeSource);
            _gl.CompileShader(computeShader);
            _gl.AttachShader(_id, computeShader);
        }
        
        if (_type.HasFlag(ShaderType.Geometry))
        {
            geometryShader = _gl.CreateShader(GLEnum.GeometryShader);
            _gl.ShaderSource(geometryShader, _geometrySource);
            _gl.CompileShader(geometryShader);
            _gl.AttachShader(_id, geometryShader);
        }
        
        _gl.LinkProgram(_id);
        
        if (_type.HasFlag(ShaderType.Vertex))
        {
            _gl.DeleteShader(vertexShader);
        }
        
        if (_type.HasFlag(ShaderType.Fragment))
        {
            _gl.DeleteShader(fragmentShader);
        }
        
        if (_type.HasFlag(ShaderType.Compute))
        {
            _gl.DeleteShader(computeShader);
        }
        
        if (_type.HasFlag(ShaderType.Geometry))
        {
            _gl.DeleteShader(geometryShader);
        }
        
        _gl.GetProgram(_id, GLEnum.LinkStatus, out int status);
        
        if (status == 0)
        {
            string info = _gl.GetProgramInfoLog(_id);
            _errors = info;
        }
        else
        {
            _errors = null;
            
            _inputs.Clear();
            _uniforms.Clear();
            
            int count = 0;
            _gl.GetProgram(_id, GLEnum.ActiveAttributes, out count);
            for (int i = 0; i < count; i++)
            {
                // In C:     glGetActiveAttrib(program, (GLuint)i, bufSize, &length, &size, &type, name);
                
                _gl.GetActiveAttrib(_id, (uint)i, (uint)256, out uint length, out int size, out GLEnum type, out string name);
                int location = _gl.GetAttribLocation(_id, name);
                _inputs.Add(new ShaderAttribute(name, type, location, size));
            }
            
            count = 0;
            _gl.GetProgram(_id, GLEnum.ActiveUniforms, out count);
            for (int i = 0; i < count; i++)
            {
                // In C:     glGetActiveUniform(program, (GLuint)i, bufSize, &length, &size, &type, name);
                
                _gl.GetActiveUniform(_id, (uint)i, (uint)256, out uint length, out int size, out GLEnum type, out string name);
                int location = _gl.GetUniformLocation(_id, name);
                _uniforms.Add(new ShaderAttribute(name, type, location, size));
            }
        }
    }
    
    public void Use()
    {
        if (!string.IsNullOrEmpty(_errors))
        {
            // Error happened, we must not use the shader
            return;
            // This will cause the shader to not be used but will not throw an exception
        }
        _gl.UseProgram(_id);
        
        SetFloat("time", (float) Time.TimeSinceStart);
        SetFloat("deltaTime", (float) Time.DeltaTime);
    }

    public void SetInt(string name, int value)
    {
        int location = _gl.GetUniformLocation(_id, name);
        if (location == -1)
        {
            return;
        }
        _gl.Uniform1(location, value);
    }

    public void SetFloat(string name, float value)
    {
        int location = _gl.GetUniformLocation(_id, name);
        if (location == -1)
        {
            return;
        }
        _gl.Uniform1(location, value);
    }

    public void SetVec2(string name, Vector2 value)
    {
        int location = _gl.GetUniformLocation(_id, name);
        if (location == -1)
        {
            return;
        }
        _gl.Uniform2(location, value);
    }

    public void SetVec3(string name, Vector3 value)
    {
        int location = _gl.GetUniformLocation(_id, name);
        if (location == -1)
        {
            return;
        }
        _gl.Uniform3(location, value);
    }
    
    public void SetVec4(string name, Vector4 value)
    {
        int location = _gl.GetUniformLocation(_id, name);
        if (location == -1)
        {
            return;
        }
        _gl.Uniform4(location, value);
    }
    
    public unsafe void SetMat4(string name, Matrix4x4 value)
    {
        int location = _gl.GetUniformLocation(_id, name);
        if (location == -1)
        {
            return;
        }
        _gl.UniformMatrix4(location, 1, false, (float*)&value);
    }
    
    public void Dispose()
    {
        _gl.DeleteProgram(_id);
    }

    public string GetFragmentSource()
    {
        return _fragmentSource;
    }
    
    public string GetVertexSource()
    {
        return _vertexSource;
    }
    
    public string GetComputeSource()
    {
        return _computeSource;
    }
    
    public string GetVertexLocation()
    {
        return _vertexLocation;
    }
    
    public string GetFragmentLocation()
    {
        return _fragmentLocation;
    }
    
    public string GetComputeLocation()
    {
        return _computeLocation;
    }
    
    public string GetGeometrySource()
    {
        return _geometrySource;
    }
    
    public string GetGeometryLocation()
    {
        return _geometryLocation;
    }

    public string GetErrors()
    {
        return _errors;
    }
    
    public void Update(string vertexSource, string fragmentSource)
    {
        _vertexSource = vertexSource;
        _fragmentSource = fragmentSource;
        _shouldCompile = true;
    }
    
    private bool _shouldCompile;
    
    public override void Update(double time)
    {
        if (_shouldCompile)
        {
            Compile();
            _shouldCompile = false;
        }
    }

    public void Recompile()
    {
        Create(Name, _vertexLocation, _fragmentLocation, _computeLocation, _geometryLocation);
    }
}

[Flags]
public enum ShaderType
{
    Vertex = 1,
    Fragment = 2,
    Compute = 4,
    Geometry = 8
}