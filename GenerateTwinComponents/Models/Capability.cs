using System.Text.Json.Serialization;

namespace AzureDigitalTwinsSample
{
    class Capability : DigitalTwinBase
    {
        [JsonPropertyName("modelId")]
        public string ModelId { get; set; }
    }
}
