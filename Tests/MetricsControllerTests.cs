using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
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
            // Crear mock de FluxRecord
            var mockRecord1 = new Mock<FluxRecord>(MockBehavior.Strict, 0);
            mockRecord1.SetupGet(r => r.Values).Returns(new Dictionary<string, object> { { "canal", "login" } });

            var mockRecord2 = new Mock<FluxRecord>(MockBehavior.Strict, 0);
            mockRecord2.SetupGet(r => r.Values).Returns(new Dictionary<string, object> { { "canal", "registro" } });

            var mockRecord3 = new Mock<FluxRecord>(MockBehavior.Strict, 0);
            mockRecord3.SetupGet(r => r.Values).Returns(new Dictionary<string, object> { { "canal", "login" } });

            // Mock de FluxTable usando interfaz
            var fluxTable = new Mock<FluxTable>();
            var records = new List<FluxRecord> { mockRecord1.Object, mockRecord2.Object, mockRecord3.Object };
            fluxTable.SetupGet(t => t.Records).Returns(records);

            // Mock del servicio
            var influxServiceMock = new Mock<InfluxService>(null);
            influxServiceMock.Setup(s => s.GetMetricsAsync()).ReturnsAsync(new List<FluxTable> { fluxTable.Object });

            // Instanciar controller
            var controller = new MetricsController(influxServiceMock.Object);

            // Ejecutar acción
            var result = await controller.GetMetrics();

            // Verificaciones
            var contentResult = Assert.IsType<ContentResult>(result);
            Assert.Equal("text/plain", contentResult.ContentType);
            Assert.Contains("eventos_totales{canal=\"login\"} 2", contentResult.Content);
            Assert.Contains("eventos_totales{canal=\"registro\"} 1", contentResult.Content);
        }
    }
}

