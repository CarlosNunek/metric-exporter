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
            // Crear registros falsos con estructura similar
            var fakeRecords = new List<FakeFluxRecord>
            {
                new FakeFluxRecord("login"),
                new FakeFluxRecord("registro"),
                new FakeFluxRecord("login")
            };

            // Adaptarlos a FluxRecord dinámicamente
            var records = fakeRecords.Select(r => r.ToFluxRecord()).ToList();

            // Simular tabla
            var table = new FluxTable();
            var recordsProp = typeof(FluxTable).GetProperty("Records");
            recordsProp.SetValue(table, records);

            // Servicio falso
            var service = new FakeInfluxService(new List<FluxTable> { table });

            var controller = new MetricsController(service);
            var result = await controller.GetMetrics();

            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("text/plain", content.ContentType);
            Assert.Contains("eventos_totales{canal=\"login\"} 2", content.Content);
            Assert.Contains("eventos_totales{canal=\"registro\"} 1", content.Content);
        }

        private class FakeFluxRecord
        {
            public string Canal { get; }

            public FakeFluxRecord(string canal)
            {
                Canal = canal;
            }

            public FluxRecord ToFluxRecord()
            {
                var record = (FluxRecord)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(FluxRecord));
                var field = typeof(FluxRecord).GetField("_values", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var dict = new Dictionary<string, object> { { "canal", Canal } };
                field?.SetValue(record, dict);
                return record;
            }
        }

        private class FakeInfluxService : InfluxService
        {
            private readonly List<FluxTable> _tables;

            public FakeInfluxService(List<FluxTable> tables) : base(null)
            {
                _tables = tables;
            }

            public new Task<List<FluxTable>> GetMetricsAsync()
            {
                return Task.FromResult(_tables);
            }
        }
    }
}
