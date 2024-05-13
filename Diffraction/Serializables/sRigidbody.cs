using System.Numerics;
using Diffraction.Physics;
using MagicPhysX;
using MagicPhysX.Toolkit;
using Newtonsoft.Json;

namespace Diffraction.Serializables;

public class sRigidbody
{
    [JsonIgnore] public Rigidbody Rb;
    public sCollisionShape Shape;

    public sRigidbody(sCollisionShape shape)
    {
        Shape = shape;

        if (shape.ShapeType == CollisionShapeType.Box)
        {
            Vector3 halfExtent = new Vector3(shape.Sizes[0], shape.Sizes[1], shape.Sizes[2]);
            Rb = Simulation.Instance.AddDynamicBox(halfExtent, shape.Position, shape.Rotation, shape.Density);
        }
        else if (shape.ShapeType == CollisionShapeType.Sphere)
        {
            Rb = Simulation.Instance.AddDynamicSphere(shape.Sizes[0], shape.Position, shape.Rotation, shape.Density);
        }
    }

    public void CreateRigidbody()
    {
        if (Shape.ShapeType == CollisionShapeType.Box)
        {
            Vector3 halfExtent = new Vector3(Shape.Sizes[0], Shape.Sizes[1], Shape.Sizes[2]);
            Rb = Simulation.Instance.AddDynamicBox(halfExtent, Shape.Position, Shape.Rotation, Shape.Density);
        }
        else if (Shape.ShapeType == CollisionShapeType.Sphere)
        {
            Rb = Simulation.Instance.AddDynamicSphere(Shape.Sizes[0], Shape.Position, Shape.Rotation, Shape.Density);
        }
    }
}