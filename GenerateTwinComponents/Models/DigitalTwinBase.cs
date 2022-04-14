using Azure.DigitalTwins.Core;
using System.Text.Json.Serialization;

namespace AzureDigitalTwinsSample
{
    class DigitalTwinBase
    {
        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinId)]
        public string Id { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinETag)]
        public string ETag { get; set; }

        [JsonPropertyName(DigitalTwinsJsonPropertyNames.DigitalTwinMetadata)]
        public DigitalTwinMetaData Metadata { get; set; } = new DigitalTwinMetaData();

        public class DigitalTwinMetaData
        {
            [JsonPropertyName(DigitalTwinsJsonPropertyNames.MetadataModel)]
            public string ModelId { get; set; }
        }
    }
}
