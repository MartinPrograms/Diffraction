using System.Numerics;

namespace Diffraction.Serializables;

public class sCollisionShape
{
    public CollisionShapeType ShapeType;

    public List<float> Sizes; // apart from mesh, this is the size of the shape, which is 1 for sphere and 3 for box
    public Vector3 Position;
    public Quaternion Rotation;
    public float Density;
    
    public sCollisionShape(CollisionShapeType shapeType)
    {
        ShapeType = shapeType;
    }
    
    public sCollisionShape(CollisionShapeType shapeType, List<float> sizes)
    {
        ShapeType = shapeType;
        Sizes = sizes;
    }
    
    public sCollisionShape(CollisionShapeType shapeType, List<float> sizes, Vector3 position, Quaternion rotation, float density)
    {
        ShapeType = shapeType;
        Sizes = sizes;
        Position = position;
        Rotation = rotation;
        Density = density;
    } 
    public sCollisionShape(CollisionShapeType shapeType, List<float> sizes, Vector3 position, Quaternion rotation)
    {
        ShapeType = shapeType;
        Sizes = sizes;
        Position = position;
        Rotation = rotation;
    }
    public sCollisionShape(CollisionShapeType shapeType, List<float> sizes, Vector3 position, Vector4 rotation, float density)
    {
        ShapeType = shapeType;
        Sizes = sizes;
        Position = position;
        Rotation = new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
        Density = density;
    } 
    public sCollisionShape(CollisionShapeType shapeType, List<float> sizes, Vector3 position, Vector4 rotation)
    {
        ShapeType = shapeType;
        Sizes = sizes;
        Position = position;
        Rotation = new Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);
    }

    public sCollisionShape()
    {
        
    }
}

public enum CollisionShapeType
{
    Box,
    Sphere,
    Mesh,
}