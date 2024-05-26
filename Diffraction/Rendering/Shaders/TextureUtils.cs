using Diffraction.Rendering.Windowing;

namespace Diffraction.Rendering.Shaders;

public class TextureUtils
{
    private static Dictionary<string, Texture> _textures = new Dictionary<string, Texture>();
    public static Texture GetTexture(string path)
    {
        if (_textures.ContainsKey(path))
        {
            return _textures[path];
        }
        else
        {
            var texture = new Texture(Window.Instance.GL, path);
            _textures.Add(path, texture);
            return texture;
        }
    }
    
    public static Texture GetTexture(Serializables.sTexture texture)
    {
        return GetTexture(texture.TextureName);
    }

    public static bool Exists(string textureName)
    {
        return _textures.ContainsKey(textureName);
    }

    private static Dictionary<string, Cubemap> _cubemaps = new Dictionary<string, Cubemap>();
    public static Cubemap GetCubemap(string[] cubemapName)
    {
        if (cubemapName.Length != 6)
        {
            throw new Exception("Cubemap name must have 6 elements");
        }
        if (_cubemaps.ContainsKey(string.Join(",", cubemapName)))
        {
            return _cubemaps[string.Join(",", cubemapName)];
        }
        else
        {
            var cubemap = new Cubemap(Window.Instance.GL, cubemapName);
            _cubemaps.Add(string.Join(",", cubemapName), cubemap);
            return cubemap;
        }
    }
}