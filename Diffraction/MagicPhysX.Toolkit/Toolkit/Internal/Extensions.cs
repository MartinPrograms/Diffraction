using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MagicPhysX.Toolkit.Internal;

internal static class Extensions
{
    internal static unsafe PxVec3* AsPxPointer(this Vector3 v)
    {
        PxVec3 pxVec3 = new PxVec3();
        pxVec3.x = v.X;
        pxVec3.y = v.Y;
        pxVec3.z = v.Z;
        
        IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(pxVec3));
        Marshal.StructureToPtr(pxVec3, ptr, false);
        
        return (PxVec3*)ptr;
    }
}
