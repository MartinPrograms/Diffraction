using Diffraction.Rendering.Shaders;

namespace Diffraction.Serializables;

public class sTexture
{
    public string TextureName;
    
    public sTexture(string textureName)
    {
        if (string.IsNullOrEmpty(textureName))
        {
            throw new ArgumentNullException(nameof(textureName));
        }

        if (!TextureUtils.Exists(textureName))
        {
            Console.WriteLine($"Texture {textureName} not found");
            Console.WriteLine("Creating texture");
            TextureUtils.GetTexture(textureName);
        }
        TextureName = textureName;
    }
}