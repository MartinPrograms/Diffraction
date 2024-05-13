using Diffraction.Rendering.Meshes;

namespace Diffraction.Serializables;

public class sMeshData
{
    public string MeshName;
    
    public sMeshData(string meshName)
    {
        if (string.IsNullOrEmpty(meshName))
        {
            throw new ArgumentNullException(nameof(meshName));
        }

        if (!MeshUtils.Exists(meshName))
        {
            Console.WriteLine($"Mesh {meshName} not found");
            Console.WriteLine("Creating mesh");
            MeshUtils.GetMesh(meshName); // gets it but also adds it to the dictionary
        }
        
        MeshName = meshName;
    }

    public void Bind()
    {
        MeshUtils.GetMesh(MeshName).Bind();
    }

    public void Draw()
    {
        MeshUtils.GetMesh(MeshName).Draw();
    }
    
    public void Draw(Silk.NET.OpenGL.GLEnum mode)
    {
        MeshUtils.GetMesh(MeshName).Draw(mode);
    }
}