using Diffraction.Rendering;
using Diffraction.Serializables;
using MagicPhysX;
using MagicPhysX.Toolkit;
using Newtonsoft.Json;
using Object = Diffraction.Rendering.Objects.Object;
using Transform = Diffraction.Rendering.Meshes.Transform;

namespace Diffraction.Physics;

public class StaticPhysicsObject : EventObject, IPhysicsObject
{
    [JsonProperty]
    private sObject _parent;
    [JsonProperty]
    private sRigidstatic _body;

    public StaticPhysicsObject()
    { 
        
    }
    
    public StaticPhysicsObject(sObject parent, sRigidstatic body)
    {
        _parent = parent;
        _body = body;
        
        if (_body.Rs == null)
        {
            _body.CreateRigidstatic();
        }
    }
    public override void Update(double time)
    {
        
    }
    public void Dispose()
    {
        Simulation.Instance.RemoveRigidstatic(_body.Rs);
    }

    public unsafe void SetPhysicsTransform(Transform objTransform)
    {
        PxTransform transform = new PxTransform();
        transform.p = objTransform.Position;
        transform.q = new PxQuat(){x = objTransform.Rotation.X, y = objTransform.Rotation.Y, z = objTransform.Rotation.Z, w = objTransform.Rotation.W};
        
        _body.Rs.RigidActorHandler.SetGlobalPoseMut(&transform, true);
        Simulation.Instance.WakeAllRigidbodies();
    }
}