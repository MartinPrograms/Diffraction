using Silk.NET.Assimp;

namespace Diffraction.Rendering.Meshes;

public class AssimpLoader
{
    private Assimp _assimp;
    public AssimpLoader()
    {
        _assimp = Assimp.GetApi();
    }
    
    public unsafe float[] LoadVertices(string modelPath)
    {
        var scene = _assimp.ImportFile(modelPath, (uint)PostProcessSteps.Triangulate | (uint)PostProcessSteps.GenerateUVCoords | (uint)PostProcessSteps.CalculateTangentSpace | (uint)PostProcessSteps.GenerateSmoothNormals | (uint)PostProcessSteps.JoinIdenticalVertices | (uint)PostProcessSteps.FlipUVs);
        
        var vertices = new List<float>();

        var meshes = scene->MMeshes; // Mesh **
        for (uint i = 0; i < scene->MNumMeshes; i++)
        {
            var mesh = meshes[i];
            var verticesPtr = mesh->MVertices; // Vector3D *
            for (uint j = 0; j < mesh->MNumVertices; j++)
            {
                var vertex = verticesPtr[j];
                vertices.Add(vertex.X);
                vertices.Add(vertex.Y);
                vertices.Add(vertex.Z);
                
                var normalsPtr = mesh->MNormals; // Vector3D *
                var normal = normalsPtr[j];
                vertices.Add(normal.X);
                vertices.Add(normal.Y);
                vertices.Add(normal.Z);
                
                var texCoordsPtr = mesh->MTextureCoords[0]; // Vector3D *
                var texCoord = texCoordsPtr[j];
                vertices.Add(texCoord.X);
                vertices.Add(texCoord.Y);
            }
        }
        
        return vertices.ToArray();
    }

    public unsafe uint[] LoadIndices(string modelPath)
    {
        var scene = _assimp.ImportFile(modelPath, (uint)PostProcessSteps.Triangulate | (uint)PostProcessSteps.GenerateUVCoords | (uint)PostProcessSteps.CalculateTangentSpace | (uint)PostProcessSteps.GenerateSmoothNormals | (uint)PostProcessSteps.JoinIdenticalVertices | (uint)PostProcessSteps.FlipUVs);
        
        var indices = new List<uint>();

        var meshes = scene->MMeshes; // Mesh **
        for (uint i = 0; i < scene->MNumMeshes; i++)
        {
            var mesh = meshes[i];
            var faces = mesh->MFaces; // Face **
            for (uint j = 0; j < mesh->MNumFaces; j++)
            {
                var face = faces[j];
                var indicesPtr = face.MIndices; // uint *
                for (uint k = 0; k < face.MNumIndices; k++)
                {
                    indices.Add(indicesPtr[k]);
                }
            }
        }
        
        return indices.ToArray();
    }
}