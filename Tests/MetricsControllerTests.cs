using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using metric_exporter.Controllers;
using InfluxDB.Client.Core.Flux.Domain;
using metric_exporter.Services;

namespace metric_exporter.Tests
{
    public class MetricsControllerTests
    {
        [Fact]
        public async Task GetMetrics_ReturnsExpectedMetrics()
        {
            // Fake service que devuelve directamente lo que se necesita para pasar el test
            var controller = new MetricsController(new DummyService());

            var result = await controller.GetMetrics();

            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("text/plain", content.ContentType);
            Assert.Contains("eventos_totales{canal=\"login\"} 2", content.Content);
            Assert.Contains("eventos_totales{canal=\"registro\"} 1", content.Content);
        }

        private class DummyService : InfluxService
        {
            public DummyService() : base(null) { }

            public new Task<List<FluxTable>> GetMetricsAsync()
            {
                var record1 = CreateRecord("login");
                var record2 = CreateRecord("registro");
                var record3 = CreateRecord("login");

                var table = new FluxTable();
                var prop = typeof(FluxTable).GetProperty("Records");
                prop?.SetValue(table, new List<FluxRecord> { record1, record2, record3 });

                return Task.FromResult(new List<FluxTable> { table });
            }

            private FluxRecord CreateRecord(string canal)
            {
                var record = (FluxRecord)System.Runtime.Serialization.FormatterServices
                    .GetUninitializedObject(typeof(FluxRecord));

                var values = new Dictionary<string, object> { { "canal", canal } };

                var field = typeof(FluxRecord).GetField("_values",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                field?.SetValue(record, values);
                return record;
            }
        }
    }
}
