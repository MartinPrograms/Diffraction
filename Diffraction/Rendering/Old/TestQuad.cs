using Diffraction.Rendering.Shaders;
using Diffraction.Rendering.Shaders.Gen;
using Silk.NET.OpenGL;
namespace Diffraction.Rendering.Old;

using Shader = Diffraction.Rendering.Shaders.Shader;

public class TestQuad
{
    // XYZ XYZ XY
    // Position, Normal, UV
    private static float[] _vertices = new float[]
    {
        -0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 0.0f, 0.0f,  
         0.5f, -0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 0.0f, 0.0f,
         0.5f,  0.5f, 0.0f, 0.0f, 1.0f, 1.0f, 1.0f, 1.0f,
        -0.5f,  0.5f, 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 1.0f
    };
    
    private static uint[] _indices = new uint[]
    {
        0, 1, 2,
        2, 3, 0
    };
    
    private uint _vao;
    private uint _vbo;
    private uint _ebo;
    
    private GL _gl;

    private Shader shader;
    
    public unsafe TestQuad(GL gl)
    {
        _gl = gl;
        
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();
        _ebo = _gl.GenBuffer();
        
        _gl.BindVertexArray(_vao);
        
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* vertices = _vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(_vertices.Length * sizeof(float)), vertices, BufferUsageARB.StaticDraw);
        }
        
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        fixed (uint* indices = _indices)
        {
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(_indices.Length * sizeof(uint)), indices, BufferUsageARB.StaticDraw);
        }
        
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        _gl.BindVertexArray(0);
        
        shader = ShaderUtils.GetShader("QuadShader");
    }
    
    public unsafe void Render()
    {
        shader.Use();
        
        _gl.BindVertexArray(_vao);
        _gl.DrawElements(PrimitiveType.Triangles, 6, DrawElementsType.UnsignedInt, null);
    }
}