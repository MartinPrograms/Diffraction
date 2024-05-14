using System.Numerics;
using Silk.NET.Maths;

namespace Diffraction.Rendering.Meshes;

[Serializable]
public class Transform
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public Vector3 Forward
    {
        get
        {
            var forward = Vector3.Transform(Vector3.UnitZ, new Quaternion(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W));
            return Vector3.Normalize(forward);
        }
        
        set
        {
            var quat = Quaternion.CreateFromRotationMatrix(Matrix4x4.CreateLookAt(Position, Position + value,
                Vector3.UnitY));
            Rotation = new Quaternion(quat.X, quat.Y, quat.Z, quat.W);
        }
    }
    
    public Transform(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        Position = position;
        Rotation = rotation;
        Scale = scale;
    }
    
    public Matrix4x4 GetModelMatrix()
    {
        var translation = Matrix4x4.CreateTranslation(Position);
        var rotation = Matrix4x4.CreateFromQuaternion(new Quaternion(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W));
        var scale = Matrix4x4.CreateScale(Scale);
        
        return scale * rotation * translation;
    }
}