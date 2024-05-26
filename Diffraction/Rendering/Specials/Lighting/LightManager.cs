using System.Numerics;
using Diffraction.Rendering.GUI;
using Diffraction.Scripting.Globals;
using Object = Diffraction.Rendering.Objects.Object;

namespace Diffraction.Rendering.Specials.Lighting;

public class LightManager : Object
{
    [ExposeToLua("AmbientLight")]
    [ShowInEditor]
    public Vector3 AmbientLight = new Vector3(0.1f, 0.1f, 0.1f);
    public const int MaxLights = 16; // this can only change before the engine starts, and all the shaders need to be recompiled if it does, which is too much of a hassle
    // Eventually ill look into it, or i might just set it to some ridiculous number like 1000 (who needs more than 1000 lights anyway)
    // Depends on hardware specs though, because we might also be able to change it at startup, detect the gpu and set it accordingly (250 for low end, 500 for mid, 1000 for high)
    public List<Light> Lights => ObjectScene.Instance.Lights;
    public int LightCount => Lights.Count;
    
    [ExposeToLua("LightEnabled")]
    [ShowInEditor]
    public bool LightEnabled = true;
    
    public LightManager()
    {
        Name = "Light Manager";
        Transform.Position = new Vector3(0, 0, 0);
    }
}