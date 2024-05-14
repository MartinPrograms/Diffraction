using System.Numerics;
using System.Runtime.InteropServices;
using Diffraction.Rendering.Windowing;
using FreeTypeSharp;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using static FreeTypeSharp.FT;
using static FreeTypeSharp.FT_LOAD;
using static FreeTypeSharp.FT_Render_Mode_;

namespace Diffraction.Rendering.GUI.Text;

public class TextRenderer
{
    public struct Character
    {
        public uint TextureID;
        public Vector2D<uint> Size;
        public Vector2D<int> Bearing;
        public uint Advance;
    }

    public static Dictionary<string, Dictionary<char, Character>> Characters = new();
    public static string DefaultFont => Characters.First().Key;

    public static unsafe void LoadFont(GL gl, string fontName, string fontPath, int fontSize)
    {
        FT_LibraryRec_* library;
        FT_FaceRec_* face;

        FT_Error error = FT_Init_FreeType(&library);

        if (error != 0)
        {
            Console.WriteLine("Failed to initialize FreeType library");
            return;
        }

        error = FT_New_Face(library, (byte*)Marshal.StringToHGlobalAnsi(fontPath).ToPointer(), 0,
            &face);

        if (error != 0)
        {
            Console.WriteLine("Failed to load font face");
            return;
        }

        FT_Set_Pixel_Sizes(face, 0, (uint)fontSize);

        gl.PixelStore(GLEnum.UnpackAlignment, 1); // Disable byte-alignment restriction

        for (uint c = 0; c < 128; c++)
        {
            error = FT_Load_Char(face, c, FT_LOAD_RENDER);

            if (error != 0)
            {
                Console.WriteLine("Failed to load glyph");
                continue;
            }

            uint texture;
            gl.GenTextures(1, &texture);
            gl.BindTexture(GLEnum.Texture2D, texture);

            gl.TexImage2D(
                TextureTarget.Texture2D,
                0,
                InternalFormat.R8,
                face->glyph->bitmap.width,
                face->glyph->bitmap.rows,
                0,
                PixelFormat.Red,
                PixelType.UnsignedByte,
                face->glyph->bitmap.buffer
            );

            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapS, (int)GLEnum.ClampToEdge);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureWrapT, (int)GLEnum.ClampToEdge);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMinFilter, (int)GLEnum.Linear);
            gl.TexParameter(GLEnum.Texture2D, GLEnum.TextureMagFilter, (int)GLEnum.Linear);

            Character character = new Character
            {
                TextureID = texture,
                Size = new Vector2D<uint>(face->glyph->bitmap.width, face->glyph->bitmap.rows),
                Bearing = new Vector2D<int>(face->glyph->bitmap_left, face->glyph->bitmap_top),
                Advance = (uint)face->glyph->advance.x
            };

            if (!Characters.ContainsKey(fontName))
            {
                Characters.Add(fontName, new Dictionary<char, Character>());
            }

            Characters[fontName].Add((char)c, character);
        }

        FT_Done_Face(face);
        FT_Done_FreeType(library);
    }

    private static Shaders.Shader _shader;

    public static void SetShader(Shaders.Shader shader)
    {
        _shader = shader;
    }

    static uint VAO, VBO;
    public static unsafe void Init(GL gl)
    {
        gl.GenVertexArrays(1, out VAO);
        gl.GenBuffers(1, out VBO);
        gl.BindVertexArray(VAO);
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
        gl.BufferData(BufferTargetARB.ArrayBuffer, 6 * sizeof(float) * 4, null, BufferUsageARB.DynamicDraw);
        
        gl.EnableVertexAttribArray(0);
        gl.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);
        
        gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
        gl.BindVertexArray(0);
    }
/// <summary>
/// X = 0, Y = 0 is the bottom left corner. (pixels)
/// In order to render text in the top left corner, you need to set the Y value to the height of the window.
/// </summary>
/// <param name="gl"></param>
/// <param name="fontName"></param>
/// <param name="text"></param>
/// <param name="x"></param>
/// <param name="y"></param>
/// <param name="scale"></param>
/// <param name="color"></param>
    public static unsafe void RenderText(GL gl, string fontName, string text, float x, HorizontalAlignment horizontalAlignment, float y, VerticalAlignment verticalAlignment, float scale, Vector3 color)
    {
        if (!Characters.ContainsKey(fontName))
        {
            Console.WriteLine("Font not loaded");
            return;
        }
        
        _shader.Use();
        _shader.SetVec3("textColor", color);
        gl.ActiveTexture(TextureUnit.Texture0);
        gl.BindVertexArray(VAO);
        
        gl.Enable(EnableCap.Blend);
        gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        
        Matrix4x4 projection = Matrix4x4.CreateOrthographicOffCenter(0, Window.Instance.IWindow.FramebufferSize.X, Window.Instance.IWindow.FramebufferSize.Y, 0, -1, 1);
        Window.Instance.GL.Viewport(0, 0, (uint)Window.Instance.IWindow.FramebufferSize.X, (uint)Window.Instance.IWindow.FramebufferSize.Y);
        _shader.SetMat4("projection", projection);
        
        float textWidth = 0;
        foreach (char c in text)
        {
            if (!Characters[fontName].ContainsKey(c))
            {
                continue;
            }

            Character ch = Characters[fontName][c];
            textWidth += (ch.Advance >> 6) * scale;
        }
        
        float textHeight = Characters[fontName].Values.Max(c => c.Size.Y) * scale;
        
        int screenWidth = Window.Instance.IWindow.Size.X;
        int screenHeight = Window.Instance.IWindow.Size.Y;
        
        
        
        float xPos = 0;
        float yPos = 0;
        
        switch (horizontalAlignment)
        {
            case HorizontalAlignment.Left:
                xPos = x;
                break;
            case HorizontalAlignment.Center:
                xPos = x + (screenWidth - textWidth) / 2;
                break;
            case HorizontalAlignment.Right:
                xPos = x + screenWidth - textWidth;
                break;
        }
        
        switch (verticalAlignment)
        {
            case VerticalAlignment.Bottom:
                yPos = y;
                break;
            case VerticalAlignment.Center:
                yPos = y + (screenHeight - textHeight) / 2;
                break;
            case VerticalAlignment.Top:
                yPos = y + screenHeight - textHeight;
                break;
        }
        
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (!Characters[fontName].ContainsKey(c))
            {
                continue;
            }

            Character ch = Characters[fontName][c];
            
            float x0 = xPos + ch.Bearing.X * scale;
            float y0 = yPos - (ch.Size.Y - ch.Bearing.Y) * scale;
            float x1 = x0 + ch.Size.X * scale;
            float y1 = y0 + ch.Size.Y * scale;
            
            float[] vertices = new float[]
            {
                x0, y0, 0, 0,
                x0, y1, 0, 1,
                x1, y1, 1, 1,
                
                x0, y0, 0, 0,
                x1, y1, 1, 1,
                x1, y0, 1, 0
            };
            
            gl.BindTexture(TextureTarget.Texture2D, ch.TextureID);
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, VBO);
            fixed (void* v = vertices)
            {
                gl.BufferSubData(BufferTargetARB.ArrayBuffer, 0,(uint) vertices.Length * sizeof(float), v);
            }
            gl.BindBuffer(BufferTargetARB.ArrayBuffer, 0);
            
            gl.DrawArrays(PrimitiveType.Triangles, 0, 6);
            
            xPos += (ch.Advance >> 6) * scale;
        }
        
        gl.BindVertexArray(0);
        gl.Disable(EnableCap.Blend);
        
        gl.BindTexture(TextureTarget.Texture2D, 0);
    }
}

public enum HorizontalAlignment
{
    Left,
    Center,
    Right
}

public enum VerticalAlignment
{
    Top,
    Center,
    Bottom
}
