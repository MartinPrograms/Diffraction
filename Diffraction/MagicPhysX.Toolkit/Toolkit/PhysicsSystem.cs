using MagicPhysX.Toolkit.Internal;
using MagicPhysX;
using System.Text;
using static MagicPhysX.NativeMethods;
using System.Numerics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using Diffraction.Physics;

namespace MagicPhysX.Toolkit;

public sealed unsafe class PhysicsSystem : IDisposable
{
    static readonly uint VersionNumber = (5 << 24) + (1 << 16) + (3 << 8);
    PxDefaultCpuDispatcher* dispatcher;
    internal PxPhysics* physics;

    bool disposedValue;
    bool enablePvd;

    static UnorderedKeyedCollection<PhysicsScene> scenes = new UnorderedKeyedCollection<PhysicsScene>();

    public PhysicsSystem()
        : this(false)
    {
    }

    public PhysicsSystem(bool enablePvd)
        : this(enablePvd, "127.0.0.1", 5425)
    {
    }

    public PhysicsSystem(string pvdIp, int pvdPort)
        : this(true, pvdIp, pvdPort)
    {
    }

    PhysicsSystem(bool enablePvd, string pvdIp, int pvdPort)
    {
        this.enablePvd = enablePvd;

        if (!enablePvd)
        {
            this.physics = physx_create_physics(PhysicsFoundation.GetFoundation());
            this.dispatcher = phys_PxDefaultCpuDispatcherCreate(1, null, PxDefaultCpuDispatcherWaitForWorkMode.WaitForWork, 0);
            return;
        }

        PxPvd* pvd = default;

        // create pvd
        pvd = phys_PxCreatePvd(PhysicsFoundation.GetFoundation());

        fixed (byte* bytePointer = Encoding.UTF8.GetBytes(pvdIp))
        {
            var transport = phys_PxDefaultPvdSocketTransportCreate(bytePointer, pvdPort, 10);
            pvd->ConnectMut(transport, PxPvdInstrumentationFlags.All);
        }

        var tolerancesScale = new PxTolerancesScale
        {
            length = 1,
            speed = 10
        };

        this.physics = phys_PxCreatePhysics(
            VersionNumber,
            PhysicsFoundation.GetFoundation(),
            &tolerancesScale,
            true,
            pvd,
            null);
        phys_PxInitExtensions(physics, pvd);

        this.dispatcher = phys_PxDefaultCpuDispatcherCreate(1, null, PxDefaultCpuDispatcherWaitForWorkMode.WaitForWork, 0);
    }

    public event Action<PhysicsScene>? SceneCreated;

    public PhysicsScene CreateScene()
    {
        var scene = CreateScene(new Vector3(0.0f, -9.81f, 0.0f));
        lock (scenes)
        {
            scenes.Add(scene);
        }

        SceneCreated?.Invoke(scene);
        return scene;
    }

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    delegate void OnContactModifyDelegate(PxContactModifyPair* pairs, uint count);

    static void OnContactModify(PxContactModifyPair* pairs, uint count)
    {
        Console.WriteLine("OnContactModify");
    }
    
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void CustomFilterShaderDelegate(FilterShaderCallbackInfo* callbackInfo, PxFilterFlags filterFlags);
    
    static void CustomFilterShader(FilterShaderCallbackInfo* callbackInfo, PxFilterFlags filterFlags)
    {
        PxPairFlags flags = PxPairFlags.ContactDefault | PxPairFlags.NotifyTouchFound | PxPairFlags.NotifyContactPoints | PxPairFlags.NotifyThresholdForceFound | PxPairFlags.NotifyThresholdForceLost | PxPairFlags.NotifyTouchLost;
        
        callbackInfo->pairFlags[0] = flags;
    }
    
    public CustomFilterShaderDelegate CustomFilterShaderInstance = CustomFilterShader; // Assign the managed delegate to the unmanaged delegate instance
    
    
    
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    // public delegate*unmanaged[Cdecl]<void*, PxContactPairHeader*, PxContactPair*, uint, void> OnContactDelegate;
    public delegate void CollisionCallbackDelegate(IntPtr userData, PxContactPairHeader* pairHeader, PxContactPair* contact, PxPairFlags flags);    

    static void CollisionCallback(IntPtr userData, PxContactPairHeader* pairHeader, PxContactPair* contact, PxPairFlags flags)
    {
        var impactFlags = contact->flags;
        // If impactFlags = 0, we lost contact
        // If impactFlags = 4, we found first contact between two objects

        if ((impactFlags & PxContactPairFlags.ActorPairHasFirstTouch) != 0)
        {
            IntPtr bs = new IntPtr(pairHeader->actors);
            var actor0 = Marshal.ReadIntPtr(bs);
            var actor1 = Marshal.ReadIntPtr(bs, IntPtr.Size);

            var rigidActor0 = (PxRigidActor*)actor0;
            var rigidActor1 = (PxRigidActor*)actor1;

            RigidActor rb0 = null;
            RigidActor rb1 = null;

            if (rigidActor0 != null)
            {
                rb0 = Simulation.Instance.RigidActor(rigidActor0);
            }

            if (rigidActor1 != null)
            {
                rb1 = Simulation.Instance.RigidActor(rigidActor1);
            }

            if (rb0 != null && rb1 != null)
            {
                rb0.OnCollisionEnter(rb0, rb1);
                rb1.OnCollisionEnter(rb1, rb0);
            }
        }
        
if ((impactFlags & PxContactPairFlags.ActorPairLostTouch) != 0)
        {
            IntPtr bs = new IntPtr(pairHeader->actors);
            var actor0 = Marshal.ReadIntPtr(bs);
            var actor1 = Marshal.ReadIntPtr(bs, IntPtr.Size);

            var rigidActor0 = (PxRigidActor*)actor0;
            var rigidActor1 = (PxRigidActor*)actor1;

            RigidActor rb0 = null;
            RigidActor rb1 = null;

            if (rigidActor0 != null)
            {
                rb0 = Simulation.Instance.RigidActor(rigidActor0);
            }

            if (rigidActor1 != null)
            {
                rb1 = Simulation.Instance.RigidActor(rigidActor1);
            }

            if (rb0 != null && rb1 != null)
            {
                rb0.OnCollisionExit(rb0, rb1);
                rb1.OnCollisionExit(rb1, rb0);
            }
        }
    }
    
    public CollisionCallbackDelegate OnContactDelegateInstance = CollisionCallback; // Assign the managed delegate to the unmanaged delegate instance
    
    public PhysicsScene CreateScene(Vector3 gravity)
    {
        var sceneDesc = PxSceneDesc_new(PxPhysics_getTolerancesScale(physics));
        sceneDesc.gravity = gravity;
        sceneDesc.cpuDispatcher = (PxCpuDispatcher*)dispatcher;
        sceneDesc.filterShader = get_default_simulation_filter_shader();
        sceneDesc.solverType = PxSolverType.Pgs; // PGS = Projected Gauss Seidel
        sceneDesc.broadPhaseType = PxBroadPhaseType.Sap; // Sap = Sweep and Prune
        
        sceneDesc.flags |= PxSceneFlags.EnableStabilization;

        var simcallback = new SimulationEventCallbackInfo();
        simcallback.collision_callback = (delegate* unmanaged[Cdecl]<void*, PxContactPairHeader*, PxContactPair*, uint, void>)Marshal.GetFunctionPointerForDelegate<CollisionCallbackDelegate>(OnContactDelegateInstance).ToPointer();
        var callbackinfoithink = create_simulation_event_callbacks(&simcallback);
        sceneDesc.simulationEventCallback = callbackinfoithink;
        
        var thing = (delegate* unmanaged[Cdecl]<FilterShaderCallbackInfo*, PxFilterFlags>)Marshal.GetFunctionPointerForDelegate<CustomFilterShaderDelegate>(CustomFilterShaderInstance).ToPointer();
        enable_custom_filter_shader(&sceneDesc, thing, (uint)1);

        sceneDesc.flags |= PxSceneFlags.EnableCcd; // Enable continuous collision detection
        sceneDesc.flags |= PxSceneFlags.EnablePcm; // Enable persistent contact manifold
        
        var scene = physics->CreateSceneMut(&sceneDesc);
        
        scene->SetVisualizationParameterMut(PxVisualizationParameter.Scale, 1.0f);
        
        if (enablePvd)
        {
            var pvdClient = scene->GetScenePvdClientMut();

            if (pvdClient != null)
            {
                pvdClient->SetScenePvdFlagMut(PxPvdSceneFlag.TransmitConstraints, true);
                pvdClient->SetScenePvdFlagMut(PxPvdSceneFlag.TransmitContacts, true);
                pvdClient->SetScenePvdFlagMut(PxPvdSceneFlag.TransmitScenequeries, true);
            }
        }

        // The flag we need to enable for the callback below to function: PxPairFlag.ModifyContacts;
        
        return new PhysicsScene(this, scene);
    }
    
    internal void RemoveScene(PhysicsScene scene)
    {
        lock (scenes)
        {
            scenes.Remove(scene);
        }
    }

    public PhysicsScene[] GetPhysicsScenes()
    {
        lock (scenes)
        {
            return scenes.ToArray();
        }
    }

    public void ForEachPhysicsScenes(Action<PhysicsScene> action)
    {
        lock (scenes)
        {
            foreach (var item in scenes.AsSpan())
            {
                action(item);
            }
        }
    }

    public bool TryCopyPhysicsScenes(Span<PhysicsScene> dest)
    {
        lock (scenes)
        {
            var span = scenes.AsSpan();
            return span.TryCopyTo(dest);
        }
    }

    public PxMaterial* CreateMaterial(float staticFriction, float dynamicFriction, float restitution)
    {
        return physics->CreateMaterialMut(staticFriction, @dynamicFriction, restitution);
    }

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // cleanup managed code
                foreach (var item in scenes.AsSpan())
                {
                    item.Dispose();
                }
                scenes.Clear();
                scenes = null!;
            }

            // cleanup unmanaged resource
            PxDefaultCpuDispatcher_release_mut(dispatcher);
            PxPhysics_release_mut(physics);

            dispatcher = null;
            physics = null;

            disposedValue = true;
        }
    }

    ~PhysicsSystem()
    {
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}
