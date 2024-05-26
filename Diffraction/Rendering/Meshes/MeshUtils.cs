using Diffraction.Rendering.Windowing;

namespace Diffraction.Rendering.Meshes;

public class MeshUtils
{
    private static Dictionary<string, MeshData> _meshes = new Dictionary<string, MeshData>();
    
    public static bool Exists(string meshName)
    {
        return _meshes.ContainsKey(meshName);
    }

    public static MeshData GetMesh(string meshName)
    {
        if (_meshes.ContainsKey(meshName))
        {
            return _meshes[meshName];
        }
        else
        {
            var mesh = new MeshData(Window.Instance.GL, meshName);
            _meshes.Add(meshName, mesh);
            return mesh;
        }
    }
}