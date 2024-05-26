using System.Numerics;
using Diffraction.Scripting;
using Newtonsoft.Json;
using Silk.NET.Maths;
using SilkyGizmos;

namespace Diffraction.Rendering.Meshes;

[Serializable]
public class Transform
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public Vector3 Forward => Vector3.Transform(new Vector3(0, 0, 1), Matrix4x4.CreateFromQuaternion(new Quaternion(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W)));
    public Vector3 Right => - Vector3.Transform(new Vector3(1, 0, 0), Matrix4x4.CreateFromQuaternion(new Quaternion(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W)));
    public Vector3 Up => Vector3.Transform(new Vector3(0, 1, 0), Matrix4x4.CreateFromQuaternion(new Quaternion(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W)));

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

    public void SetModelMatrix(Matrix4x4 matrix)
    {
        Position = Mathf.ExtractTranslation(matrix);
        Rotation = Mathf.ExtractRotation(matrix);
        Scale = Mathf.ExtractScale(matrix);
    }

    public sgTransform AsSGTransform()
    {
        return new sgTransform(Position, Rotation, Scale);
    }

    public static Transform FromSGTransform(sgTransform model)
    {
        return new Transform(model.Position, model.Rotation, model.Scale);
    }
}

