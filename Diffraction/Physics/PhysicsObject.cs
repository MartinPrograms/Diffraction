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

    public LockRotationFlags LockRotation;
    public bool IsGrounded;
    
    public PhysicsObject(sObject parent, sRigidbody body)
    {
        _parent = parent;
        _rigidbody = body;
        
        if (_rigidbody.Rb == null)
        {
            _rigidbody.CreateRigidbody();
        }
        
        Name = "PhysicsObject";
        
        body.AddEnterEvent((a, b) =>
        {
            var objB = b as Rigidstatic;
            if (objB != null)
            {
                IsGrounded = true;
            }
        });
        
        body.AddExitEvent((a, b) =>
        {
            var objB = b as Rigidstatic;
            if (objB != null)
            {
                IsGrounded = false;
            }
        });
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
        
        Quaternion rotation = new Quaternion(_rigidbody.Rb.rotation.X, _rigidbody.Rb.rotation.Y, _rigidbody.Rb.rotation.Z, _rigidbody.Rb.rotation.W);

        if (LockRotation.HasFlag(LockRotationFlags.X))
        {
            rotation.X = _parent.GetObject().Transform.Rotation.X;
        }
        
        if (LockRotation.HasFlag(LockRotationFlags.Y))
        {
            rotation.Y = _parent.GetObject().Transform.Rotation.Y;
        }
        
        if (LockRotation.HasFlag(LockRotationFlags.Z))
        {
            rotation.Z = _parent.GetObject().Transform.Rotation.Z;
        }

        _parent.GetObject().Transform.Rotation = rotation;
    }

    public void SetPhysicsTransform(Transform objTransform)
    {
        _rigidbody.Rb.position = objTransform.Position;
        _rigidbody.Rb.rotation = new PxQuat(){x = objTransform.Rotation.X, y = objTransform.Rotation.Y, z = objTransform.Rotation.Z, w = objTransform.Rotation.W};
    }

    public void AddForce(Vector3 force)
    {
        _rigidbody.Rb.AddForce(force, ForceMode.Impulse);
    }
    public void AddForce(Vector3 force, ForceMode mode)
    {
        _rigidbody.Rb.AddForce(force, mode);
    }
}

[Flags]
public enum LockRotationFlags
{
    None = 0,
    X = 1,
    Y = 2,
    Z = 4,
    All = X | Y | Z
}