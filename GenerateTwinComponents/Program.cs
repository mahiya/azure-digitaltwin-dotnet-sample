using Azure.DigitalTwins.Core;
using Azure.Identity;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AzureDigitalTwinsSample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var command = new RootCommand();
            command.Description = "Register sample models, twins, relations to your Azure Digital Twin instance";
            command.AddOption(new Option<string>(new[] { "--instance-name", "-n" }, "Your Azure Digtal Twin instance name") { IsRequired = true });
            command.Handler = CommandHandler.Create<string>(Run);
            await command.InvokeAsync(args);
        }

        static async Task Run(string instanceName)
        {
            // Digital Twin API へアクセスするためのクライアントを作成する
            var adtInstanceUrl = new Uri($"https://{instanceName}.api.sea.digitaltwins.azure.net");
            var credential = new DefaultAzureCredential();
            var client = new DigitalTwinsClient(adtInstanceUrl, credential);

            // モデルを作成する
            var modelDefinitions = Directory.GetFiles("Models/Definitions", "*.json").Select(file => File.ReadAllText(file));
            Console.WriteLine($"Create model: {string.Join("\n", modelDefinitions)}");
            await client.CreateModelsAsync(modelDefinitions);

            // デジタルツインを作成する
            var twins = GenerateSampleTwins();
            foreach (var twin in twins)
            {
                Console.WriteLine($"Create digital twin: {twin.Metadata.ModelId}");
                await client.CreateOrReplaceDigitalTwinAsync(twin.Id, twin);
            }

            // リレーションシップを作成する
            var relations = GenerateSampleRelations();
            foreach (var relation in relations)
            {
                Console.WriteLine($"Create relationship: {relation.Id}: {relation.SourceId} -> {relation.TargetId}");
                await client.CreateOrReplaceRelationshipAsync(relation.SourceId, relation.Id, relation);
            }

            // 指定した設備のケーパビリティを取得するして表示する
            const string dtId = "EquipmentId1";
            var query = $"SELECT target.$dtId, target.$metadata.$model AS modelId FROM digitaltwins d JOIN target RELATED d.hasCapability WHERE d.$dtId = '{dtId}'";
            var queriedTwins = await client.QueryAsync<JsonDocument>(query).ToListAsync();
            var capabilities = queriedTwins.Select(t => JsonSerializer.Deserialize<Capability>(t.RootElement.ToString()));
            Console.WriteLine(JsonSerializer.Serialize(capabilities, new JsonSerializerOptions { WriteIndented = true }));
        }

        /// <summary>
        /// Azure Digital Twins に登録するサンプルのデジタルツインを生成する
        /// </summary>
        static IEnumerable<SimpleTwin> GenerateSampleTwins()
        {
            return new[]
            {
                new { model = "Building", count = 1 },
                new { model = "Floor", count = 1 },
                new { model = "Room", count = 1 },
                new { model = "Equipment", count = 2 },
                new { model = "CapabilityA", count = 2 },
                new { model = "CapabilityB", count = 2 }
            }
            .SelectMany(c => Enumerable.Range(1, c.count).Select(i => new
            {
                id = $"{c.model}Id{i}",
                modelId = $"dtmi:sample:{c.model};1",
                name = $"{c.model}{i}"
            }))
            .Select(o => new SimpleTwin
            {
                Id = o.id,
                Name = o.name,
                Metadata = { ModelId = o.modelId },
            });
        }

        /// <summary>
        /// Azure Digital Twins に登録するサンプルのリレーションシップを生成する
        /// </summary>
        static IEnumerable<BasicRelationship> GenerateSampleRelations()
        {
            var i = 0;
            Func<string, string, string, BasicRelationship> generateRelation = (name, sourceId, targetId) =>
            {
                return new BasicRelationship
                {
                    Id = $"RelId{i++}",
                    Name = name,
                    SourceId = sourceId,
                    TargetId = targetId,
                };
            };

            return new[]
            {
                generateRelation("hasPart", "BuildingId1", "FloorId1"),
                generateRelation("hasPart", "FloorId1", "RoomId1"),
                generateRelation("isLocationOf", "RoomId1", "EquipmentId1"),
                generateRelation("isLocationOf", "RoomId1", "EquipmentId2"),
                generateRelation("hasCapability", "EquipmentId1", "CapabilityAId1"),
                generateRelation("hasCapability", "EquipmentId1", "CapabilityBId1"),
                generateRelation("hasCapability", "EquipmentId2", "CapabilityAId2"),
                generateRelation("hasCapability", "EquipmentId2", "CapabilityBId2"),
                generateRelation("isPartOf", "FloorId1", "BuildingId1"),
                generateRelation("isPartOf", "RoomId1", "FloorId1"),
                generateRelation("locatedIn", "EquipmentId1", "RoomId1"),
                generateRelation("locatedIn", "EquipmentId2", "RoomId1"),
                generateRelation("isCapabilityOf", "CapabilityAId1", "EquipmentId1"),
                generateRelation("isCapabilityOf", "CapabilityBId1", "EquipmentId1"),
                generateRelation("isCapabilityOf", "CapabilityAId2", "EquipmentId2"),
                generateRelation("isCapabilityOf", "CapabilityBId2", "EquipmentId2"),
            };
        }
    }
}
