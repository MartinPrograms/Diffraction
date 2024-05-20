using System.Numerics;
using Silk.NET.OpenGL;

namespace SilkyGizmos.Shaders;

public static class Shader
{
    internal static string VertexShader = @"
        #version 330 core
        layout(location = 0) in vec3 aPos;
        
        out vec3 color;
        
        uniform mat4 model;
        uniform mat4 view;
        uniform mat4 projection;
        
        void main()
        {
            gl_Position = projection * view * model * vec4(aPos, 1.0);
        }
";

    internal static string FragmentShader = @"
        #version 330 core
        uniform vec3 color;
        out vec4 FragColor;
        
        void main()
        {
            FragColor = vec4(color, 1.0);
        }
";
    
    private static uint _shaderProgram;
    private static bool _initialized = false;
    
    public static void UseColorShader(GL gl)
    {
        if (!_initialized)
        {
            CreateShaderProgram(gl);
        }
        
        gl.UseProgram(_shaderProgram);
    }
    
    public static void SetVector3(GL gl, string name, Vector3 vector)
    {
        int location = gl.GetUniformLocation(_shaderProgram, name);
        gl.Uniform3(location, vector);
    }
    
    public static unsafe void SetMatrix4(GL gl, string name, Matrix4x4 matrix)
    {
        int location = gl.GetUniformLocation(_shaderProgram, name);
        unsafe
        {
            gl.UniformMatrix4(location, 1, false, &matrix.M11);
        }
    }
    
    

    private static void CreateShaderProgram(GL gl)
    {
        uint vertexShader = gl.CreateShader(ShaderType.VertexShader);
        gl.ShaderSource(vertexShader, VertexShader);
        gl.CompileShader(vertexShader);
        
        uint fragmentShader = gl.CreateShader(ShaderType.FragmentShader);
        gl.ShaderSource(fragmentShader, FragmentShader);
        gl.CompileShader(fragmentShader);
        
        _shaderProgram = gl.CreateProgram();
        gl.AttachShader(_shaderProgram, vertexShader);
        gl.AttachShader(_shaderProgram, fragmentShader);
        gl.LinkProgram(_shaderProgram);
        
        gl.DeleteShader(vertexShader);
        gl.DeleteShader(fragmentShader);
        
        _initialized = true;
        
        if (gl.GetProgram(_shaderProgram, GLEnum.LinkStatus) == 0)
        {
            throw new Exception("Failed to link shader program!");
        }
    }
}