namespace Diffraction.Rendering.Meshes;

public class ModelLoader
{
    public static Dictionary<string, Tuple<float[], uint[]>> Models = new();
    public static Tuple<float[], uint[]> LoadModel(string path)
    {
        if (Models.ContainsKey(path))
        {
            return Models[path];
        }
        
        var loader = new AssimpLoader();
        float[] vertices = loader.LoadVertices(path);
        uint[] indices = loader.LoadIndices(path);
        
        Models.Add(path, new Tuple<float[], uint[]>(vertices, indices));
        
        return new Tuple<float[], uint[]>(vertices, indices);
    }
}