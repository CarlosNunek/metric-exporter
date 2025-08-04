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
            // Crear registros manualmente con valores
            var records = new List<FluxRecord>();

            records.Add(CreateFluxRecord("login"));
            records.Add(CreateFluxRecord("registro"));
            records.Add(CreateFluxRecord("login"));

            // Crear tabla con los registros
            var table = new FluxTable();
            typeof(FluxTable).GetProperty("Records").SetValue(table, records);

            // Crear servicio simulado
            var fakeService = new FakeInfluxService(new List<FluxTable> { table });

            var controller = new MetricsController(fakeService);

            var result = await controller.GetMetrics();

            var content = Assert.IsType<ContentResult>(result);
            Assert.Equal("text/plain", content.ContentType);
            Assert.Contains("eventos_totales{canal=\"login\"} 2", content.Content);
            Assert.Contains("eventos_totales{canal=\"registro\"} 1", content.Content);
        }

        private FluxRecord CreateFluxRecord(string canal)
        {
            var record = (FluxRecord)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(FluxRecord));
            typeof(FluxRecord).GetProperty("Values").SetValue(record, new Dictionary<string, object> { { "canal", canal } });
            return record;
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
