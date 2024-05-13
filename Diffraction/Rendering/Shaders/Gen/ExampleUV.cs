using System.Numerics;
using ShaderGen;

namespace Diffraction.Rendering.Shaders.Gen;

public class ExampleUV
{
    public struct VertexInput
    {
        [PositionSemantic] public Vector3 Position;
        [TextureCoordinateSemantic] public Vector2 UV;
    }
    
    public struct FragmentInput
    {
        [SystemPositionSemantic] public Vector4 Position;
        [TextureCoordinateSemantic] public Vector2 UV;
    }
    
    [VertexShader]
    public FragmentInput VS(VertexInput input)
    {
        FragmentInput output;
        output.Position = new Vector4(input.Position, 1);
        output.UV = input.UV;
        return output;
    }
    
    [FragmentShader]
    public Vector4 FS(FragmentInput input)
    {
        return new Vector4(input.UV, 0, 1);
    }
}