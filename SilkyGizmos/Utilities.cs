using System.Numerics;

namespace SilkyGizmos;

public static class Utilities
{
    public static Vector3 ExtractTranslation(this Matrix4x4 matrix)
    {
        return new Vector3(matrix.M41, matrix.M42, matrix.M43);
    }

    public static Vector3 ExtractScale(this Matrix4x4 matrix)
    {
        return new Vector3(matrix.M11, matrix.M22, matrix.M33);
    }

    public static Quaternion ExtractRotation(this Matrix4x4 matrix)
    {
        var forward = new Vector3(matrix.M31, matrix.M32, matrix.M33);
        var up = new Vector3(matrix.M21, matrix.M22, matrix.M23);
        return Quaternion.CreateFromRotationMatrix(Matrix4x4.CreateLookAt(Vector3.Zero, forward, up));
    }
}

public class sgTransform
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;

    public Vector3 Forward => Vector3.Transform(new Vector3(0, 0, 1), Matrix4x4.CreateFromQuaternion(new Quaternion(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W)));
    public Vector3 Right => - Vector3.Transform(new Vector3(1, 0, 0), Matrix4x4.CreateFromQuaternion(new Quaternion(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W)));
    public Vector3 Up => Vector3.Transform(new Vector3(0, 1, 0), Matrix4x4.CreateFromQuaternion(new Quaternion(Rotation.X, Rotation.Y, Rotation.Z, Rotation.W)));
    public sgTransform(Vector3 position, Quaternion rotation, Vector3 scale)
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
        Position = matrix.ExtractTranslation();
        Rotation = matrix.ExtractRotation();
        Scale = matrix.ExtractScale();
    }
}

