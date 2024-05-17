using System.Numerics;
using MagicPhysX.Toolkit;
using MagicPhysX.Toolkit.Colliders;

namespace Diffraction.Physics;

public class HitData
{
	public bool Hit;
	public Vector3 Position;
	public Vector3 Normal;
	public float Distance;
	public RigidActor Object;
}

public static class Raycast
{
	// This class is used to perform raycasts in the physics scene
	public static HitData RaycastScene(Vector3 origin, Vector3 direction, float distance)
	{
		Simulation.Instance.Raycast(origin, direction, distance, out var hitData);

		return hitData;
	}
}