using System.Text.Json;
using System.Text.Json.Serialization;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using AmmoGen.Converters;

namespace AmmoGen.Helpers;

public static class PropertiesHelper
{
    public static TemplateItemProperties? DeserializeProperties(JsonElement properties)
    {
        if (properties.ValueKind == JsonValueKind.Undefined || properties.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<TemplateItemProperties>(properties.GetRawText(), new JsonSerializerOptions
        {
            Converters = { new MongoIdJsonConverter(), new JsonStringEnumConverter() },
        });
    }
}
