using Diffraction.Rendering.Meshes;

namespace Diffraction.Physics;

public interface IPhysicsObject
{
    void SetPhysicsTransform(Transform objTransform);
    void Dispose();
}