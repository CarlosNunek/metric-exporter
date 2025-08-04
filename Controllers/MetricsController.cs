using Microsoft.AspNetCore.Mvc;
using metric_exporter.Services;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace metric_exporter.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MetricsController : ControllerBase
    {
        private readonly InfluxService _influx;

        public MetricsController(InfluxService influx)
        {
            _influx = influx;
        }

        [HttpGet("/metrics")]
        public async Task<IActionResult> GetMetrics()
        {
            var metrics = new StringBuilder();
            metrics.Append("# HELP eventos_totales Número total de eventos recibidos\n");
            metrics.Append("# TYPE eventos_totales counter\n");


            var data = await _influx.GetMetricsAsync();

            var grouped = data
                .SelectMany(table => table.Records)
                .GroupBy(r => r.Values["canal"].ToString());

            foreach (var grupo in grouped)
            {
                metrics.Append($"eventos_totales{{canal=\"{grupo.Key}\"}} {grupo.Count()}\n");

            }

            return Content(metrics.ToString(), "text/plain");
        }
    }
}

