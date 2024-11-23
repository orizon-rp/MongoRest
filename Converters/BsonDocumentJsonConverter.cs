using System.Text.Json;
using System.Text.Json.Serialization;
using MongoDB.Bson;

namespace MongoRest.Converters;

public class BsonDocumentJsonConverter : JsonConverter<BsonDocument>
{
    public override BsonDocument Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var jsonString = jsonDoc.RootElement.GetRawText();

        return BsonDocument.Parse(jsonString);
    }

    public override void Write(Utf8JsonWriter writer, BsonDocument value, JsonSerializerOptions options)
    {
        var jsonString = value.ToJson();
        using var doc = JsonDocument.Parse(jsonString);

        doc.WriteTo(writer);
    }
}