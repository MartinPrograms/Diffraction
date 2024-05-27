using System.Numerics;
using System.Runtime.InteropServices;
using Diffraction.Serializables;
using Silk.NET.Assimp;

using Material = Diffraction.Rendering.Shaders.Materials.Material;

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
                
                // Now for the tangent and bitangent
                var tangentsPtr = mesh->MTangents; // Vector3D *
                var tangent = tangentsPtr[j];
                vertices.Add(tangent.X);
                vertices.Add(tangent.Y);
                vertices.Add(tangent.Z);
                
                var bitangentsPtr = mesh->MBitangents; // Vector3D *
                var bitangent = bitangentsPtr[j];
                vertices.Add(bitangent.X);
                vertices.Add(bitangent.Y);
                vertices.Add(bitangent.Z);
                
                // And for the time being that's it I hope
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

public class AdvancedModel
{
    // Meant for getting vertices, and other mesh data, including textures, supports multiple meshes and materials.
    private string _modelPath;
    public AdvancedModel(string modelPath)
    {
        _modelPath = modelPath;
    }
    
    private List<ModelData> _modelData = new();
    public List<ModelData> MeshData => _modelData;

    public unsafe void LoadMeshData()
    {
        var assimp = Assimp.GetApi();
        var scene = assimp.ImportFile(_modelPath, (uint)PostProcessSteps.Triangulate | (uint)PostProcessSteps.GenerateUVCoords | (uint)PostProcessSteps.CalculateTangentSpace | (uint)PostProcessSteps.GenerateSmoothNormals | (uint)PostProcessSteps.JoinIdenticalVertices | (uint)PostProcessSteps.FlipUVs);
        
        var meshes = scene->MMeshes; // Mesh **
        // instead of combining all the vertices and indices into one array, we will create a list of ModelData objects

        Dictionary<int,Material> materials = new(); // material id + material
        
        for (uint i = 0; i < scene->MNumMaterials; i++)
        {
            Silk.NET.Assimp.Material* material = scene->MMaterials[i];
            
            // we need to get a few things from the material, the texture paths, and the material properties
            // the material above has these properties: material.MProperties (MaterialProperty **) and material.MNumProperties (uint) which is the number of properties
            
            // To get the texture we have to jump through a few hoops
            
            var diffuseTextures = new List<string>();
            // assimp.GetMaterialTextureCount(material, TextureType.Diffuse, 0) returns the number of textures of type Diffuse
            var diffuseCount = assimp.GetMaterialTextureCount(material, TextureType.Diffuse);
            for (uint j = 0; j < diffuseCount; j++)
            {
                AssimpString* path = stackalloc AssimpString[1];
                TextureMapping* mapping = stackalloc TextureMapping[1];
                var texture = assimp.GetMaterialTexture(material, TextureType.Diffuse, j, path, mapping, null, null,
                    null, null, null); 
                if (texture == Return.Success)
                {
                    string texturePath = path->ToString();
                    if (!diffuseTextures.Contains(texturePath))
                    {
                        diffuseTextures.Add(texturePath);
                        Console.WriteLine("Diffuse texture path: " + texturePath + " for material " + i);
                    }
                }
            }
            
            var normalTextures = new List<string>();
            var normalCount = assimp.GetMaterialTextureCount(material, TextureType.Normals);
            for (uint j = 0; j < normalCount; j++)
            {
                AssimpString* path = stackalloc AssimpString[1];
                TextureMapping* mapping = stackalloc TextureMapping[1];
                var texture = assimp.GetMaterialTexture(material, TextureType.Normals, j, path, mapping, null, null,
                    null, null, null); 
                if (texture == Return.Success)
                {
                    string texturePath = path->ToString();
                    if (!normalTextures.Contains(texturePath))
                    {
                        normalTextures.Add(texturePath);
                    }
                }
            }
            
            // now to create a material object for each texture.
            if (normalCount == 0  && diffuseCount == 0)
            {
                continue;
            }
            
            string normalTexturePath = null;
            string diffuseTexturePath = null;
            
            string directoryName = Path.GetDirectoryName(_modelPath);
            if (normalTextures.Count > 0)
            {
                normalTexturePath = Path.Combine(directoryName, normalTextures[0]);
            }
            
            if (diffuseTextures.Count > 0)
            {
                diffuseTexturePath = Path.Combine(directoryName, diffuseTextures[0]);
            }
            
            if (normalTexturePath == null)
            {
                normalTexturePath = "Textures/default.png";
            }
            
            if (diffuseTexturePath == null)
            {
                diffuseTexturePath = "Textures/default.png";
            }
            
            var mat = new Material(new sShader("LitShader"), new sTexture(diffuseTexturePath),
                new sTexture(normalTexturePath));
            
            materials.Add((int)i, mat);
        }
        
        Console.WriteLine("Materials loaded successfully : " + materials.Count);

        for (uint i = 0; i < scene->MNumMeshes; i++)
        {
            var mesh = meshes[i];
            var vertices = new List<float>();
            var indices = new List<uint>();

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

                // Now for the tangent and bitangent
                var tangentsPtr = mesh->MTangents; // Vector3D *
                var tangent = tangentsPtr[j];
                vertices.Add(tangent.X);
                vertices.Add(tangent.Y);
                vertices.Add(tangent.Z);

                var bitangentsPtr = mesh->MBitangents; // Vector3D *
                var bitangent = bitangentsPtr[j];
                vertices.Add(bitangent.X);
                vertices.Add(bitangent.Y);
                vertices.Add(bitangent.Z);

                // And for the time being that's it I hope
            }

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

            if (materials.TryGetValue((int)mesh->MMaterialIndex, out var material))
            {
                _modelData.Add(new ModelData
                {
                    Material = material,
                    Vertices = vertices.ToArray(),
                    Indices = indices.ToArray(),
                    Path = _modelPath,
                    Transform = null
                });
            }
        }
    }
}

public class ModelData
{
    public Material Material { get; set; }
    public float[] Vertices { get; set; }
    public uint[] Indices { get; set; }
    public string Path { get; set; }
    public Transform? Transform { get; set; }

    public Mesh ToComponent()
    {
        return new Mesh(this);
    }
}