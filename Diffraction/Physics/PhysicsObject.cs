using System.Numerics;
using Diffraction.Audio;
using Diffraction.Rendering;
using Diffraction.Serializables;
using MagicPhysX;
using MagicPhysX.Toolkit;
using Newtonsoft.Json;
using Object = Diffraction.Rendering.Objects.Object;
using Transform = Diffraction.Rendering.Meshes.Transform;

namespace Diffraction.Physics;

public class PhysicsObject : EventObject, IPhysicsObject
{
    [JsonProperty]
    private sObject _parent;
    [JsonProperty]
    private sRigidbody _rigidbody;
    public PhysicsObject(sObject parent, sRigidbody body)
    {
        _parent = parent;
        _rigidbody = body;
        
        if (_rigidbody.Rb == null)
        {
            _rigidbody.CreateRigidbody();
        }
    }

    public PhysicsObject()
    {
        
    }

    public Vector3 Velocity
    {
        set
        {
            _rigidbody.Rb.velocity = value;
        }
        
        get
        {
            return _rigidbody.Rb.velocity;
        }
    }

    public sRigidbody Rigidbody  => _rigidbody;

    public void Dispose()
    {
        Simulation.Instance.RemoveRigidbody(_rigidbody.Rb);
    }
    public override void Update(double time)
    {
        _parent.GetObject().Transform.Position = _rigidbody.Rb.position;
        _parent.GetObject().Transform.Rotation = new Quaternion(_rigidbody.Rb.rotation.X, _rigidbody.Rb.rotation.Y, _rigidbody.Rb.rotation.Z, _rigidbody.Rb.rotation.W);
        
        Console.WriteLine(_rigidbody.Rb.velocity);
    }

    public void SetPhysicsTransform(Transform objTransform)
    {
        _rigidbody.Rb.position = objTransform.Position;
        _rigidbody.Rb.rotation = new PxQuat(){x = objTransform.Rotation.X, y = objTransform.Rotation.Y, z = objTransform.Rotation.Z, w = objTransform.Rotation.W};
    }

    public void AddForce(Vector3 force)
    {
        _rigidbody.Rb.velocity = Vector3.Zero;
        _rigidbody.Rb.velocity += force;
    }
}