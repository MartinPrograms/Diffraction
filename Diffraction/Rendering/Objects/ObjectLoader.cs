using Diffraction.Rendering.Meshes;

namespace Diffraction.Rendering.Objects;

public class ObjectLoader
{
    public static List<Object> Load(string path)
    {
        if (Supported3DFileFormats.Contains(Path.GetExtension(path)))
        {
            return Load3D(path);
        }
        else
        {
            throw new Exception("Unsupported file format");
        }
    }

    private static List<Object> Load3D(string path)
    {
        var objects = new List<Object>();
        
        var advancedModel = new AdvancedModel(path);
        advancedModel.LoadMeshData();
        
        foreach (var meshData in advancedModel.MeshData)
        {
            var obj = new Object();
            obj.Components.Add(meshData.ToComponent());
            objects.Add(obj);
        }
        
        return objects;
    }

    public static readonly string[] Supported3DFileFormats = new string[8]
    {
        ".obj",
        ".fbx",
        ".3ds",
        ".blend",
        ".dae",
        ".dxf",
        ".gltf",
        ".glb",
    };
}