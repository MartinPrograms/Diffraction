using Diffraction.Scripting.Globals;
using Object = Diffraction.Rendering.Objects.Object;

namespace Diffraction.Serializables;

public class sObject
{
    public Guid Id;
    
    public Object? GetObject()
    {
        return ObjectScene.Instance.GetObject(Id);
    }

    public sObject(Guid id)
    {
        Id = id;
    }
}