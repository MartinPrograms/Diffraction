using System.Numerics;
using Diffraction.Physics;
using MagicPhysX.Toolkit;
using Newtonsoft.Json;

namespace Diffraction.Serializables;

public class sRigidstatic
{
    [JsonIgnore]
    public Rigidstatic Rs;
    
    public sCollisionShape Shape;
    
    public sRigidstatic(sCollisionShape shape)
    {
        Shape = shape;
        
        if (shape.ShapeType == CollisionShapeType.Box)
        {
            Vector3 halfExtent = new Vector3(shape.Sizes[0], shape.Sizes[1], shape.Sizes[2]);
            Rs = Simulation.Instance.AddStaticBox(halfExtent, shape.Position, shape.Rotation);
        }
        else if (shape.ShapeType == CollisionShapeType.Sphere)
        {
            //Rs = Simulation.Instance.AddStaticSphere(shape.Sizes[0], shape.Position, shape.Rotation);
        }
    }
    
    public void CreateRigidstatic()
    {
        if (Shape.ShapeType == CollisionShapeType.Box)
        {
            Vector3 halfExtent = new Vector3(Shape.Sizes[0], Shape.Sizes[1], Shape.Sizes[2]);
            Rs = Simulation.Instance.AddStaticBox(halfExtent, Shape.Position, Shape.Rotation);
        }
        else if (Shape.ShapeType == CollisionShapeType.Sphere)
        {
            //Rs = Simulation.Instance.AddStaticSphere(Shape.Sizes[0], Shape.Position, Shape.Rotation);
        }
    }
}