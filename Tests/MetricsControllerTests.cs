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
            // Simular Records como diccionarios
            var record1 = CreateFakeRecord("login");
            var record2 = CreateFakeRecord("registro");
            var record3 = CreateFakeRecord("login");

            var records = new List<FluxRecord> { record1, record2, record3 };

            // Crear tabla con los registros simulados
            var table = new FluxTable();
            var prop = typeof(FluxTable).GetProperty("Records");
            prop.SetValue(table, records);

            var fakeService = new FakeInfluxService(new List<FluxTable> { table });

            var controller = new MetricsController(fakeService);
            var result = await controller.GetMetrics();

            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("text/plain", content.ContentType);
            Assert.Contains("eventos_totales{canal=\"login\"} 2", content.Content);
            Assert.Contains("eventos_totales{canal=\"registro\"} 1", content.Content);
        }

        // Crea un FluxRecord simulado con canal
        private FluxRecord CreateFakeRecord(string canal)
        {
            var rec = (FluxRecord)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(FluxRecord));
            var field = typeof(FluxRecord).GetField("_values", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(rec, new Dictionary<string, object> { { "canal", canal } });
            return rec;
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
