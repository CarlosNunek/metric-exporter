using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using metric_exporter.Controllers;
using metric_exporter.Services;
using InfluxDB.Client.Core.Flux.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace metric_exporter.Tests
{
    public class MetricsControllerTests
    {
        [Fact]
        public async Task GetMetrics_ReturnsExpectedMetrics()
        {
            // Simulamos el comportamiento de la base con datos falsos
            var simulatedRecords = new List<FakeRecord>
            {
                new FakeRecord("login"),
                new FakeRecord("registro"),
                new FakeRecord("login")
            };

            var fakeService = new FakeInfluxService(simulatedRecords);

            var controller = new MetricsController(fakeService);

            var result = await controller.GetMetrics();

            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("text/plain", content.ContentType);
            Assert.Contains("eventos_totales{canal=\"login\"} 2", content.Content);
            Assert.Contains("eventos_totales{canal=\"registro\"} 1", content.Content);
        }

        private class FakeRecord
        {
            public Dictionary<string, object> Values { get; }

            public FakeRecord(string canal)
            {
                Values = new Dictionary<string, object> { { "canal", canal } };
            }
        }

        private class FakeInfluxService : InfluxService
        {
            private readonly List<FakeRecord> _fakeRecords;

            public FakeInfluxService(List<FakeRecord> records) : base(new EmptyConfiguration())
            {

                _fakeRecords = records;
            }

            private class EmptyConfiguration : IConfiguration
            {

                public string this[string key]
                {
                    get
                    {
                        if (key == "InfluxDB:Url") return "http://localhost:8086"; // dummy URL
                        if (key == "InfluxDB:Token") return "ICusY4BPCbyxTKIxAtp7ciEalt96codUyofHs6wzANAmAH0TX9vTOGzMXor3Ryzgys-gfAHCUszY1WOcNz4v5A==";     // dummy token
                        return null;
                    }
                    set { }
                }
                public IEnumerable<IConfigurationSection> GetChildren() => Enumerable.Empty<IConfigurationSection>();
                public IChangeToken GetReloadToken() => null;
                public IConfigurationSection GetSection(string key) => new EmptySection();
                private class EmptySection : IConfigurationSection
                {
                    public string this[string key] { get => null; set { } }
                    public string Key => string.Empty;
                    public string Path => string.Empty;
                    public string Value
                    {
                        get
                        {
                            if (Key == "InfluxDB:Url") return "http://localhost:8086";
                            if (Key == "InfluxDB:Token") return "ICusY4BPCbyxTKIxAtp7ciEalt96codUyofHs6wzANAmAH0TX9vTOGzMXor3Ryzgys-gfAHCUszY1WOcNz4v5A==";
                            return null;
                        }
                        set { }
                    }
                    public IEnumerable<IConfigurationSection> GetChildren() => Enumerable.Empty<IConfigurationSection>();
                    public IChangeToken GetReloadToken() => null;
                    public IConfigurationSection GetSection(string key) => this;
                }
            }
        
            public new Task<List<FluxTable>> GetMetricsAsync() {
                var fluxTable = new FluxTable();

                var records = _fakeRecords.Select(fake =>
                {
                    var record = (FluxRecord)System.Runtime.Serialization.FormatterServices
                    .GetUninitializedObject(typeof(FluxRecord));
                    var valuesField = typeof(FluxRecord).GetField("_values", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    valuesField?.SetValue(record, fake.Values);
                    return record;
                }).ToList();

                var recordsProp = typeof(FluxTable).GetProperty("Records");
                recordsProp?.SetValue(fluxTable, records);

                return Task.FromResult(new List<FluxTable> { fluxTable }); }

        }


    }
}

