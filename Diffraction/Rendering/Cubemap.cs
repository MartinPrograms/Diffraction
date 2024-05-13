using Diffraction.Rendering.Meshes;
using Silk.NET.OpenGL;
using StbImageSharp;
using Buffer = System.Buffer;

namespace Diffraction.Rendering;

public class Cubemap
{
    public string[] Paths;

    private uint textureid;
    private GL gl;
    public Cubemap(GL gl, string[] paths)
    {
        if (paths.Length != 6)
        {
            throw new Exception("Cubemap must have 6 paths");
        }
        Paths = paths;
        
        this.gl = gl;
        
        textureid = gl.GenTexture(); 
        gl.BindTexture(TextureTarget.TextureCubeMap, textureid);

        for (int i = 0; i < 6; i++)
        {
            ImageResult result = null;
            if (i == 2 || i == 3)
            {
                result = ImageResult.FromMemory(File.ReadAllBytes(paths[i]), ColorComponents.RedGreenBlueAlpha);

                unsafe
                {
                    fixed (byte* ptr = result.Data)
                    {
                        StbImage.stbi__vertical_flip(ptr, result.Width, result.Height, 4);
                    }
                    
                    fixed (byte* ptr = result.Data)
                    {
                        // Now flip it horizontally
                        stbi_kyon_horizontal_flip(ptr, result.Width, result.Height, 4);
                    }
                }
                
            }
            else
                result = ImageResult.FromMemory(File.ReadAllBytes(paths[i]), ColorComponents.RedGreenBlueAlpha);
            unsafe
            {
                fixed (byte* ptr = result.Data)
                {
                    gl.TexImage2D(TextureTarget.TextureCubeMapPositiveX + i, 0, InternalFormat.Rgba, (uint)result.Width,
                        (uint)result.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, ptr);
                }
            }
        }
        gl.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        gl.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        gl.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)GLEnum.ClampToEdge);
    }
    
    public void Bind()
    {
        gl.BindTexture(TextureTarget.TextureCubeMap, textureid);
    }

    private static unsafe void stbi_kyon_horizontal_flip(byte* data, int width, int height, int channels)
    {
        // Here is an example in C:
        //   size_t line_bytes = (size_t)w * bytes_per_pixel;
        // stbi_uc temp[line_bytes];
        // stbi_uc *bytes = (stbi_uc *)image;
        // int lpos, rpos;
        // for (int col = 0; col < h; col++) {
        //   stbi_uc *line = bytes + col * line_bytes;
        //   memcpy(&temp, line, line_bytes);
        //   for (int row = 0; row < w; row++) {
        //     lpos = row * bytes_per_pixel;
        //     rpos = line_bytes - row * bytes_per_pixel - 1;
        //     line[lpos] = temp[rpos - 3];
        //     line[lpos + 1] = temp[rpos - 2];
        //     line[lpos + 2] = temp[rpos - 1];
        //     line[lpos + 3] = temp[rpos];
        //   }
        // }
        
        int line_bytes = width * channels;
        byte[] temp = new byte[line_bytes];
        byte* bytes = data;
        int lpos, rpos;
        for (int col = 0; col < height; col++)
        {
            byte* line = bytes + col * line_bytes;
            
            fixed (byte* tempPtr = temp)
            {
                Buffer.MemoryCopy(line, tempPtr, line_bytes, line_bytes);
            }
            
            for (int row = 0; row < width; row++)
            {
                lpos = row * channels;
                rpos = line_bytes - row * channels - 1;
                line[lpos] = temp[rpos - 3];
                line[lpos + 1] = temp[rpos - 2];
                line[lpos + 2] = temp[rpos - 1];
                line[lpos + 3] = temp[rpos];
            }
        }
        
        // And it is done!
    }
}