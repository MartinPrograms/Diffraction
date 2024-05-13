using Diffraction.Rendering.Buffers;
using Silk.NET.OpenGL;

namespace Diffraction.Rendering.Meshes;

public class MeshData
{
    private uint _vao;
    private uint _vbo;
    private uint _ebo;
    private GL _gl;
    
    int _vertexCount;
    
    public int Length => _vertexCount;
    
    public unsafe MeshData(GL gl, string modelPath)
    {
        _gl = gl;
        var meshData = ModelLoader.LoadModel(modelPath);

        var vertices = meshData.Item1;
        var indices = meshData.Item2;
        
        _vertexCount = indices.Length;
        
        _vao = _gl.GenVertexArray();
        _vbo = _gl.GenBuffer();
        _ebo = _gl.GenBuffer();
        
        _gl.BindVertexArray(_vao);
        
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
        fixed (float* verticesPtr = vertices)
        {
            _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), verticesPtr, BufferUsageARB.StaticDraw);
        }
        
        _gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
        fixed (uint* indicesPtr = indices)
        {
            _gl.BufferData(BufferTargetARB.ElementArrayBuffer, (uint)(indices.Length * sizeof(uint)), indicesPtr, BufferUsageARB.StaticDraw);
        }
        
        _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
        _gl.EnableVertexAttribArray(0);
        _gl.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
        _gl.EnableVertexAttribArray(1);
        _gl.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 6 * sizeof(float));
        _gl.EnableVertexAttribArray(2);
        
        _gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    }
    
    public void Bind()
    {
        _gl.CullFace(GLEnum.Back);
        _gl.BindVertexArray(_vao);
    }

    public unsafe void Draw()
    {
        _gl.PolygonMode(TriangleFace.FrontAndBack, PolygonMode.Fill);
        _gl.DrawElements(PrimitiveType.Triangles, (uint)_vertexCount, DrawElementsType.UnsignedInt, null);
    }
    public unsafe void Draw(GLEnum mode)
    {
        _gl.PolygonMode(TriangleFace.FrontAndBack, mode);
        
        _gl.DrawElements(PrimitiveType.Triangles, (uint)_vertexCount, DrawElementsType.UnsignedInt, null);
    }
    
    public void Unbind()
    {
        _gl.BindVertexArray(0);
    }
}