using System.Data;
using System.Numerics;
using System.Runtime.CompilerServices;
using Diffraction.Rendering;
using Diffraction.Scripting.Globals;
using MagicPhysX;
using MagicPhysX.Toolkit;

namespace Diffraction.Physics;

public unsafe class Simulation : EventObject
{
    public bool IsPaused => ObjectScene.Instance.Paused;
    
    public static Simulation Instance;

    PhysicsSystem physics;
    PhysicsScene scene;
    PxMaterial *material;
    
    public Rigidbody GetRigidbody(PxRigidActor* actor)
    {
        var rigidbody = scene.GetActor(actor);
        return (Rigidbody)rigidbody!;
    }
    
    public Simulation()
    { 
        physics = new PhysicsSystem(enablePvd: false);
        scene = physics.CreateScene();
        
        material = physics.CreateMaterial(1f, 1f, .0f);
        
        Instance = this;
    }
    public double UpdateFrequency = 1.0 / 144.0; // By default, update 144 times per second
    private double _accumulator = 0.0;
    public override void Update(double time)
    {
        if (!IsPaused)
        {
            _accumulator += time;
            while (_accumulator >= UpdateFrequency)
            {
                scene.Update((float)UpdateFrequency);
                _accumulator -= UpdateFrequency;
            }
        }
    }
    public void Dispose()
    {
        
    }
    public Rigidstatic AddStaticPlane(float a, float b, float c, float d, Vector3 position, Quaternion rotation)
    {
        return scene.AddStaticPlane(a, b, c, d, position, rotation, material);
    }
    
    public Rigidbody AddDynamicSphere(float radius, Vector3 position, Quaternion rotation, float density)
    {
        return scene.AddDynamicSphere(radius, position, rotation, density, material);
    }
    
    public Rigidbody AddDynamicBox(Vector3 halfExtents, Vector3 position, Quaternion rotation, float density)
    {
        return scene.AddDynamicBox(halfExtents, position, rotation, density, material);
    }
    
    public Rigidstatic AddStaticBox(Vector3 halfExtents, Vector3 position, Quaternion rotation)
    {
        return scene.AddStaticBox(halfExtents, position, rotation, material);
    }

    public Vector3 GetPosition(Rigidbody sphere)
    {
        return sphere.transform.position;
    }

    public void RemoveRigidbody(Rigidbody rigidbody)
    {
        scene.Destroy(rigidbody);
    }

    public void RemoveRigidstatic(Rigidstatic body)
    {
        scene.Destroy(body);
    }

    public void WakeAllRigidbodies()
    {
        scene.WakeAllRigidbodies();
    }

    public RigidActor? RigidActor(PxRigidActor* rigidActor1)
    {
        return scene.GetActor(rigidActor1);
    }

    public void Clear()
    {
        scene.Clear();
    }

    public void Raycast(Vector3 origin, Vector3 direction, float distance, out HitData data)
    {
        scene.Raycast(origin, direction, distance, out data);
    }
}
