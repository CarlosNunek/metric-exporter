using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using metric_exporter.Controllers;
using metric_exporter.Services;
using InfluxDB.Client.Core.Flux.Domain;

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

            public FakeInfluxService(List<FakeRecord> records) : base(null)
            {
                _fakeRecords = records;
            }

            public new Task<List<FluxTable>> GetMetricsAsync(){
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

                return Task.FromResult(new List<FluxTable> { fluxTable });}

        }
    }
}
