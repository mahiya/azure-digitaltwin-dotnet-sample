using System.Text.Json.Serialization;

namespace AzureDigitalTwinsSample
{
    class SimpleTwin : DigitalTwinBase
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
