using System.Text.Json;
using System.Text.Json.Serialization;

namespace Diffraction.Rendering;

[Serializable]
public class EventObject
{
    public string Name;
    public virtual void Update(double time)
    {
    }

    public virtual void Render(Camera camera)
    {
    }
}

public class EventObjectConverter : JsonConverter<EventObject>
{
    public override EventObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, EventObject value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteStringValue("Type " + value.GetType().Name);
        writer.WriteEndObject();
    }
}