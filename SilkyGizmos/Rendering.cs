using System.Numerics;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Shader = SilkyGizmos.Shaders.Shader;

namespace SilkyGizmos;

public static class Rendering
{
    private static GL _gl;

    internal static void Init(GL gl)
    {
        _gl = gl;
    }
    private static float _lineThickness = 1.0f;
    public static unsafe void DrawLine(Vector3 start, Vector3 end, Vector3 color, Matrix4x4 view, Matrix4x4 projection, Vector4 windowBounds)
    {
        // Because OpenGL decided to remove the fixed function pipeline, we have to do this manually :(
        _gl.Viewport((int)windowBounds.X, (int)windowBounds.Y, (uint)windowBounds.Z, (uint)windowBounds.W);
        _gl.Enable(GLEnum.DepthTest);
        _gl.Enable(GLEnum.LineSmooth);
        _gl.Enable(GLEnum.Blend);
        _gl.LineWidth(_lineThickness);
        _gl.Clear(ClearBufferMask.DepthBufferBit);
        
        Shader.UseColorShader(_gl);
        Shader.SetVector3(_gl, "color", color);

        Shader.SetMatrix4(_gl, "view", view);
        Shader.SetMatrix4(_gl, "projection", projection);

        var model = Matrix4x4.Identity;
        Shader.SetMatrix4(_gl, "model", model);

        // XYZ
        float[] vertices = new float[]
        {
            start.X, start.Y, start.Z,
            end.X, end.Y, end.Z
        };
        
        uint vbo;
        _gl.GenBuffers(1, out vbo);
        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        fixed (float* ptr = vertices)
        {
            _gl.BufferData(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), ptr, GLEnum.StaticDraw);
        }
        
        uint vao;
        _gl.GenVertexArrays(1, out vao);
        _gl.BindVertexArray(vao);
        _gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), 0);
        _gl.EnableVertexAttribArray(0);
        
        _gl.DrawArrays(GLEnum.Lines, 0, 2);
        
        _gl.DeleteVertexArrays(1, ref vao);
        _gl.DeleteBuffers(1, ref vbo);
        
        _gl.BindVertexArray(0);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        
        _gl.UseProgram(0);
        
        _gl.Disable(GLEnum.LineSmooth);
    }

    public static unsafe void DrawBall(Vector3 position, float size, Vector3 color, Matrix4x4 view,
        Matrix4x4 projection, Vector4 windowBounds)
    {
        // Same as the DrawLine function, but with a sphere
        _gl.Viewport((int)windowBounds.X, (int)windowBounds.Y, (uint)windowBounds.Z, (uint)windowBounds.W);
        _gl.Enable(GLEnum.DepthTest);
        _gl.Enable(GLEnum.Blend);
        _gl.LineWidth(_lineThickness);
        _gl.Clear(ClearBufferMask.DepthBufferBit);
        
        Shader.UseColorShader(_gl);
        Shader.SetVector3(_gl, "color", color);
        
        Shader.SetMatrix4(_gl, "view", view);
        Shader.SetMatrix4(_gl, "projection", projection);
        
        var model = Matrix4x4.Identity;
        model *= Matrix4x4.CreateScale(size);
        model *= Matrix4x4.CreateTranslation(position);
        Shader.SetMatrix4(_gl, "model", model);
        
        uint vao;
        _gl.GenVertexArrays(1, out vao);
        _gl.BindVertexArray(vao);   
        
        uint vbo;
        _gl.GenBuffers(1, out vbo);
        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        
        float[] vertices = CreateSphereVertices(16, 16, size);
        fixed (float* ptr = vertices)
        {
            _gl.BufferData(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), ptr, GLEnum.StaticDraw);
        }
        
        _gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), 0);
        _gl.EnableVertexAttribArray(0);
        
        _gl.DrawArrays(GLEnum.Triangles, 0, (uint)vertices.Length / 3);
        
        _gl.DeleteVertexArrays(1, ref vao);
        _gl.DeleteBuffers(1, ref vbo);
        
        _gl.BindVertexArray(0);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        
        _gl.UseProgram(0);
        
        _gl.Disable(GLEnum.LineSmooth);
    }
    
    private static float[] sphereCachedVertices;
    private static Vector3 sphereCachedArgs;
    private static float[] CreateSphereVertices(int i, int i1, float size)
    {
        if (sphereCachedArgs == new Vector3(i, i1, size) && sphereCachedVertices != null)
        {
            return sphereCachedVertices;
        }
        // Create a sphere with the given amount of slices and stacks
        List<float> vertices = new List<float>();
        for (int stack = 0; stack <= i; stack++)
        {
            float stackAngle = stack * MathF.PI / i;
            float stackAngleNext = (stack + 1) * MathF.PI / i;
            for (int slice = 0; slice <= i1; slice++)
            {
                float sliceAngle = slice * 2 * MathF.PI / i1;
                float sliceAngleNext = (slice + 1) * 2 * MathF.PI / i1;
                
                float x1 = MathF.Sin(stackAngle) * MathF.Cos(sliceAngle);
                float y1 = MathF.Cos(stackAngle);
                float z1 = MathF.Sin(stackAngle) * MathF.Sin(sliceAngle);
                
                float x2 = MathF.Sin(stackAngle) * MathF.Cos(sliceAngleNext);
                float y2 = MathF.Cos(stackAngle);
                float z2 = MathF.Sin(stackAngle) * MathF.Sin(sliceAngleNext);
                
                float x3 = MathF.Sin(stackAngleNext) * MathF.Cos(sliceAngleNext);
                float y3 = MathF.Cos(stackAngleNext);
                float z3 = MathF.Sin(stackAngleNext) * MathF.Sin(sliceAngleNext);
                
                float x4 = MathF.Sin(stackAngleNext) * MathF.Cos(sliceAngle);
                float y4 = MathF.Cos(stackAngleNext);
                float z4 = MathF.Sin(stackAngleNext) * MathF.Sin(sliceAngle);
                
                vertices.AddRange(new float[] { x1 * size, y1 * size, z1 * size });
                vertices.AddRange(new float[] { x2 * size, y2 * size, z2 * size });
                vertices.AddRange(new float[] { x3 * size, y3 * size, z3 * size });
                
                vertices.AddRange(new float[] { x1 * size, y1 * size, z1 * size });
                vertices.AddRange(new float[] { x3 * size, y3 * size, z3 * size });
                vertices.AddRange(new float[] { x4 * size, y4 * size, z4 * size });
            }
        }

        sphereCachedArgs = new Vector3(i, i1, size);
        sphereCachedVertices = vertices.ToArray();
        return vertices.ToArray();
    }

    private static float[] circleCachedLines;
    private static int circleCachedSlices;
    private static float[] CreateCircleLines(int i)
    {
        if (circleCachedSlices == i && circleCachedLines != null)
        {
            return circleCachedLines;
        }
        // Create a circle with the given amount of slices
        List<float> lines = new List<float>();
        for (int slice = 0; slice <= i; slice++)
        {
            float sliceAngle = slice * 2 * MathF.PI / i;
            float sliceAngleNext = (slice + 1) * 2 * MathF.PI / i;
            
            float x1 = MathF.Cos(sliceAngle);
            float y1 = MathF.Sin(sliceAngle);
            
            float x2 = MathF.Cos(sliceAngleNext);
            float y2 = MathF.Sin(sliceAngleNext);
            
            lines.AddRange(new float[] { x1, y1, 0 });
            lines.AddRange(new float[] { x2, y2, 0 });
        }

        circleCachedSlices = i;
        circleCachedLines = lines.ToArray();
        return lines.ToArray();
    }
    public static void SetLineThickness(float f)
    {
        _lineThickness = f;
    }

    public static unsafe void DrawCircle(Vector3 rotatePoint, Vector3 forward, Vector3 up, Vector3 right, Vector3 forwardColor, Matrix4x4 view, Matrix4x4 projection, Vector4 renderPositionAndSize)
    {
        // Same as the DrawLine function, but with a circle
        _gl.Viewport((int)renderPositionAndSize.X, (int)renderPositionAndSize.Y, (uint)renderPositionAndSize.Z, (uint)renderPositionAndSize.W);
        _gl.Enable(GLEnum.DepthTest);
        _gl.Enable(GLEnum.LineSmooth);
        _gl.Enable(GLEnum.Blend);
        _gl.LineWidth(_lineThickness);
        _gl.Clear(ClearBufferMask.DepthBufferBit);
        
        Shader.UseColorShader(_gl);
        Shader.SetVector3(_gl, "color", forwardColor);
        
        Shader.SetMatrix4(_gl, "view", view);
        Shader.SetMatrix4(_gl, "projection", projection);
        
        var model = Matrix4x4.Identity;
        Matrix4x4 lookat = Matrix4x4.CreateLookAt(Vector3.Zero, forward, up);
        Quaternion rotation = Quaternion.CreateFromRotationMatrix(lookat);
        rotation = Quaternion.Normalize(rotation);
        rotation = Quaternion.CreateFromAxisAngle(right, MathF.PI / 2) * rotation;
        
        
        model *= Matrix4x4.CreateFromQuaternion(rotation);
        
        model *= Matrix4x4.CreateTranslation(rotatePoint);
        
        Shader.SetMatrix4(_gl, "model", model);
        
        uint vao;
        _gl.GenVertexArrays(1, out vao);
        _gl.BindVertexArray(vao);   
        
        uint vbo;
        _gl.GenBuffers(1, out vbo);
        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        
        float[] vertices = CreateCircleLines(32);
        fixed (float* ptr = vertices)
        {
            _gl.BufferData(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), ptr, GLEnum.StaticDraw);
        }
        
        _gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), 0);
        _gl.EnableVertexAttribArray(0);
        
        _gl.DrawArrays(GLEnum.Lines, 0, (uint)vertices.Length / 3);
        
        _gl.DeleteVertexArrays(1, ref vao);
        _gl.DeleteBuffers(1, ref vbo);
        
        _gl.BindVertexArray(0);
        _gl.BindBuffer(GLEnum.ArrayBuffer, 0);
        
        _gl.UseProgram(0);
        
        _gl.Disable(GLEnum.LineSmooth);
    }

    public static void DrawWireCube(Vector3 position, float scale, Vector3 color, Matrix4x4 view,
        Matrix4x4 projection, Vector4 renderPositionAndSize)
    {
        DrawWireCube(position, new Vector3(scale, scale, scale), color, view, projection, renderPositionAndSize);
    }

    public static void DrawWireCube(Vector3 position, Vector3 scale, Vector3 color, Matrix4x4 view, Matrix4x4 projection, Vector4 renderPositionAndSize)
    {
        // A cube has 12 lines, so we need to draw 12 lines
        DrawLine(new Vector3(position.X - scale.X, position.Y - scale.Y, position.Z - scale.Z), new Vector3(position.X + scale.X, position.Y - scale.Y, position.Z - scale.Z), color, view, projection, renderPositionAndSize);
        DrawLine(new Vector3(position.X + scale.X, position.Y - scale.Y, position.Z - scale.Z), new Vector3(position.X + scale.X, position.Y + scale.Y, position.Z - scale.Z), color, view, projection, renderPositionAndSize);
        DrawLine(new Vector3(position.X + scale.X, position.Y + scale.Y, position.Z - scale.Z), new Vector3(position.X - scale.X, position.Y + scale.Y, position.Z - scale.Z), color, view, projection, renderPositionAndSize);
        DrawLine(new Vector3(position.X - scale.X, position.Y + scale.Y, position.Z - scale.Z), new Vector3(position.X - scale.X, position.Y - scale.Y, position.Z - scale.Z), color, view, projection, renderPositionAndSize);
        
        DrawLine(new Vector3(position.X - scale.X, position.Y - scale.Y, position.Z + scale.Z), new Vector3(position.X + scale.X, position.Y - scale.Y, position.Z + scale.Z), color, view, projection, renderPositionAndSize);
        DrawLine(new Vector3(position.X + scale.X, position.Y - scale.Y, position.Z + scale.Z), new Vector3(position.X + scale.X, position.Y + scale.Y, position.Z + scale.Z), color, view, projection, renderPositionAndSize);
        DrawLine(new Vector3(position.X + scale.X, position.Y + scale.Y, position.Z + scale.Z), new Vector3(position.X - scale.X, position.Y + scale.Y, position.Z + scale.Z), color, view, projection, renderPositionAndSize);
        DrawLine(new Vector3(position.X - scale.X, position.Y + scale.Y, position.Z + scale.Z), new Vector3(position.X - scale.X, position.Y - scale.Y, position.Z + scale.Z), color, view, projection, renderPositionAndSize);
        
        DrawLine(new Vector3(position.X - scale.X, position.Y - scale.Y, position.Z - scale.Z), new Vector3(position.X - scale.X, position.Y - scale.Y, position.Z + scale.Z), color, view, projection, renderPositionAndSize);
        DrawLine(new Vector3(position.X + scale.X, position.Y - scale.Y, position.Z - scale.Z), new Vector3(position.X + scale.X, position.Y - scale.Y, position.Z + scale.Z), color, view, projection, renderPositionAndSize);
        DrawLine(new Vector3(position.X + scale.X, position.Y + scale.Y, position.Z - scale.Z), new Vector3(position.X + scale.X, position.Y + scale.Y, position.Z + scale.Z), color, view, projection, renderPositionAndSize);
        DrawLine(new Vector3(position.X - scale.X, position.Y + scale.Y, position.Z - scale.Z), new Vector3(position.X - scale.X, position.Y + scale.Y, position.Z + scale.Z), color, view, projection, renderPositionAndSize);
    }

    public static void DrawCube(Vector3 position, float scale, Vector3 color, Matrix4x4 view, Matrix4x4 projection,
        Vector4 renderPositionAndSize)
    {
        DrawCube(position, new Vector3(scale, scale, scale), color, view, projection, renderPositionAndSize);
    }

    public static void DrawCube(Vector3 position, Vector3 scale, Vector3 color, Matrix4x4 view, Matrix4x4 projection,
        Vector4 renderPositionAndSize)
    {
        // 6 sides, 6 quads
        DrawQuad(new Vector3(position.X - scale.X, position.Y - scale.Y, position.Z - scale.Z), new Vector3(position.X + scale.X, position.Y - scale.Y, position.Z - scale.Z), new Vector3(position.X + scale.X, position.Y + scale.Y, position.Z - scale.Z), new Vector3(position.X - scale.X, position.Y + scale.Y, position.Z - scale.Z), color, view, projection, renderPositionAndSize);
        DrawQuad(new Vector3(position.X - scale.X, position.Y - scale.Y, position.Z + scale.Z), new Vector3(position.X + scale.X, position.Y - scale.Y, position.Z + scale.Z), new Vector3(position.X + scale.X, position.Y + scale.Y, position.Z + scale.Z), new Vector3(position.X - scale.X, position.Y + scale.Y, position.Z + scale.Z), color, view, projection, renderPositionAndSize);
        
        DrawQuad(new Vector3(position.X - scale.X, position.Y - scale.Y, position.Z - scale.Z), new Vector3(position.X - scale.X, position.Y - scale.Y, position.Z + scale.Z), new Vector3(position.X - scale.X, position.Y + scale.Y, position.Z + scale.Z), new Vector3(position.X - scale.X, position.Y + scale.Y, position.Z - scale.Z), color, view, projection, renderPositionAndSize);
        DrawQuad(new Vector3(position.X + scale.X, position.Y - scale.Y, position.Z - scale.Z), new Vector3(position.X + scale.X, position.Y - scale.Y, position.Z + scale.Z), new Vector3(position.X + scale.X, position.Y + scale.Y, position.Z + scale.Z), new Vector3(position.X + scale.X, position.Y + scale.Y, position.Z - scale.Z), color, view, projection, renderPositionAndSize);
        
        DrawQuad(new Vector3(position.X - scale.X, position.Y - scale.Y, position.Z - scale.Z), new Vector3(position.X + scale.X, position.Y - scale.Y, position.Z - scale.Z), new Vector3(position.X + scale.X, position.Y - scale.Y, position.Z + scale.Z), new Vector3(position.X - scale.X, position.Y - scale.Y, position.Z + scale.Z), color, view, projection, renderPositionAndSize);
        DrawQuad(new Vector3(position.X - scale.X, position.Y + scale.Y, position.Z - scale.Z), new Vector3(position.X + scale.X, position.Y + scale.Y, position.Z - scale.Z), new Vector3(position.X + scale.X, position.Y + scale.Y, position.Z + scale.Z), new Vector3(position.X - scale.X, position.Y + scale.Y, position.Z + scale.Z), color, view, projection, renderPositionAndSize);
    }
    
    public static unsafe void DrawTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 color, Matrix4x4 view, Matrix4x4 projection, Vector4 renderPositionAndSize)
    {
        // Same as the DrawLine function, but with a triangle
        _gl.Viewport((int)renderPositionAndSize.X, (int)renderPositionAndSize.Y, (uint)renderPositionAndSize.Z, (uint)renderPositionAndSize.W);
        _gl.Enable(GLEnum.DepthTest);
        _gl.Enable(GLEnum.LineSmooth);
        _gl.Enable(GLEnum.Blend);
        _gl.LineWidth(_lineThickness);
        _gl.Clear(ClearBufferMask.DepthBufferBit);
        
        Shader.UseColorShader(_gl);
        Shader.SetVector3(_gl, "color", color);
        
        Shader.SetMatrix4(_gl, "view", view);
        Shader.SetMatrix4(_gl, "projection", projection);
        
        var model = Matrix4x4.Identity;
        Shader.SetMatrix4(_gl, "model", model);
        
        uint vao;
        _gl.GenVertexArrays(1, out vao);
        _gl.BindVertexArray(vao);
        
        uint vbo;
        _gl.GenBuffers(1, out vbo);
        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        
        float[] vertices = new float[]
        {
            p1.X, p1.Y, p1.Z,
            p2.X, p2.Y, p2.Z,
            p3.X, p3.Y, p3.Z
        };
        
        fixed (float* ptr = vertices)
        {
            _gl.BufferData(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), ptr, GLEnum.StaticDraw);
        }
        
        _gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), 0);
        
        _gl.EnableVertexAttribArray(0);
        
        _gl.DrawArrays(GLEnum.Triangles, 0, 3);
        
        _gl.DeleteVertexArrays(1, ref vao);
        _gl.DeleteBuffers(1, ref vbo);
        
        _gl.BindVertexArray(0);
        
        _gl.UseProgram(0);
    }

    public static unsafe void DrawQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Vector3 color, Matrix4x4 view,
        Matrix4x4 projection, Vector4 renderPositionAndSize)
    {
        _gl.Viewport((int)renderPositionAndSize.X, (int)renderPositionAndSize.Y, (uint)renderPositionAndSize.Z, (uint)renderPositionAndSize.W);
        _gl.Enable(GLEnum.DepthTest);
        _gl.Enable(GLEnum.LineSmooth);
        _gl.Enable(GLEnum.Blend);
        _gl.LineWidth(_lineThickness);
        _gl.Clear(ClearBufferMask.DepthBufferBit);
        
        Shader.UseColorShader(_gl);
        Shader.SetVector3(_gl, "color", color);
        
        Shader.SetMatrix4(_gl, "view", view);
        Shader.SetMatrix4(_gl, "projection", projection);
        
        var model = Matrix4x4.Identity;
        Shader.SetMatrix4(_gl, "model", model);
        
        uint vao;
        _gl.GenVertexArrays(1, out vao);
        _gl.BindVertexArray(vao);
        
        uint vbo;
        _gl.GenBuffers(1, out vbo);
        _gl.BindBuffer(GLEnum.ArrayBuffer, vbo);
        
        float[] vertices = new float[]
        {
            p1.X, p1.Y, p1.Z,
            p2.X, p2.Y, p2.Z,
            p3.X, p3.Y, p3.Z,
            p1.X, p1.Y, p1.Z,
            p3.X, p3.Y, p3.Z,
            p4.X, p4.Y, p4.Z
        };
        
        fixed (float* ptr = vertices)
        {
            _gl.BufferData(GLEnum.ArrayBuffer, (uint)(vertices.Length * sizeof(float)), ptr, GLEnum.StaticDraw);
        }
        
        _gl.VertexAttribPointer(0, 3, GLEnum.Float, false, 3 * sizeof(float), 0);
        
        _gl.EnableVertexAttribArray(0);
        
        _gl.DrawArrays(GLEnum.Triangles, 0, 6);
        
        _gl.DeleteVertexArrays(1, ref vao);
        _gl.DeleteBuffers(1, ref vbo);
        
        _gl.BindVertexArray(0);
        
        _gl.UseProgram(0);

    }
}