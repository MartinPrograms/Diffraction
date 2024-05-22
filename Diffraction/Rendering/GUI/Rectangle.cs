using System.Numerics;
using Diffraction.Rendering.Windowing;
using Silk.NET.OpenGL;

namespace Diffraction.Rendering.GUI;

public class Rectangle
{
    public Vector2 Position; // Screen position
    public Vector2 Size; // Rectangle size
    public Vector4 Color; // RGBA color
    public float Rotation; // Rotation in radians
    public bool IsFilled; // Is the rectangle filled
    public float LineWidth; // Line width
    
    public Rectangle(Vector2 position, Vector2 size, Vector4 color, float rotation = 0, bool isFilled = true, float lineWidth = 1)
    {
        Position = position;
        Size = size;
        Color = color;
        Rotation = rotation;
        IsFilled = isFilled;
        LineWidth = lineWidth;
    }
    
    public Rectangle(Vector2 position, Vector2 size, Vector3 color, float rotation = 0, bool isFilled = true, float lineWidth = 1)
    {
        Position = position;
        Size = size;
        Color = new Vector4(color, 1);
        Rotation = rotation;
        IsFilled = isFilled;
        LineWidth = lineWidth;
    }
    
    public Rectangle(Vector2 position, Vector2 size, float rotation = 0, bool isFilled = true, float lineWidth = 1)
    {
        Position = position;
        Size = size;
        Color = new Vector4(1, 1, 1, 1);
        Rotation = rotation;
        IsFilled = isFilled;
        LineWidth = lineWidth;
    }
}

public class RectangleRenderer
{
    public static unsafe void Render(Rectangle rectangle)
    {
        // Render the rectangle
        var gl = Window.Instance.GL;
        
        var shader = Shaders.ShaderUtils.GetShader("RectShader");
        
        uint vao = 0;
        uint vbo = 0;
        
        gl.GenVertexArrays(1, out vao);
        gl.GenBuffers(1, out vbo);
        
        gl.BindVertexArray(vao);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, vbo);

        var model = Matrix4x4.Identity;
        
        model = Matrix4x4.CreateTranslation(rectangle.Position.X, rectangle.Position.Y, 0) * model; // 0.05f to get out of the near plane
        model = Matrix4x4.CreateRotationZ(rectangle.Rotation) * model;
        model = Matrix4x4.CreateScale(rectangle.Size.X, rectangle.Size.Y, 1) * model;
        
        shader.Use();
        
        shader.SetVec4("color", rectangle.Color);
        shader.SetFloat("aspect", Camera.MainCamera.AspectRatio);
        
        shader.SetMat4("model", model);
        
        if (rectangle.IsFilled)
        {
            float[] vertices =
            {
                // Positions in NDC space (between -1 and 1)
                -1.0f, -1.0f, 0.0f,
                1.0f, -1.0f, 0.0f,
                1.0f,  1.0f, 0.0f,
                1.0f,  1.0f, 0.0f,
                -1.0f,  1.0f, 0.0f,
                -1.0f, -1.0f, 0.0f
            };

            
            fixed (float* verticesPtr = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), verticesPtr, BufferUsageARB.StaticDraw);
            }
            
            
            gl.EnableVertexAttribArray(0);
            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            
            gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
            
            gl.DisableVertexAttribArray(0);
        }
        else
        {
            // 4 vertices, and use mode lines
            float[] vertices =
            {
                -1.0f, -1.0f, 0.0f,
                1.0f, -1.0f, 0.0f,
                1.0f,  1.0f, 0.0f,
                -1.0f,  1.0f, 0.0f
            };
            
            fixed (float* verticesPtr = vertices)
            {
                gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(vertices.Length * sizeof(float)), verticesPtr, BufferUsageARB.StaticDraw);
            }
            
            gl.EnableVertexAttribArray(0);

            gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            
            gl.LineWidth(rectangle.LineWidth);
            gl.PolygonMode(GLEnum.FrontAndBack, PolygonMode.Line);
            
            gl.DrawArrays(PrimitiveType.LineLoop, 0, 4);
            
            gl.PolygonMode(GLEnum.FrontAndBack, PolygonMode.Fill);
            
            gl.DisableVertexAttribArray(0);
        }
        
        gl.DeleteVertexArrays(1, ref vao);
        gl.DeleteBuffers(1, ref vbo);
        
        gl.BindVertexArray(0);
        
        gl.PolygonMode(GLEnum.FrontAndBack, PolygonMode.Fill);
        
        gl.DisableVertexAttribArray(0);
        
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
    }
}