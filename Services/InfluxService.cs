using System.Threading.Tasks;
using System.Collections.Generic;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using Microsoft.Extensions.Configuration;

namespace metric_exporter.Services
{
    public class InfluxService
    {
        private readonly InfluxDBClient _client;
        private readonly string _org;
        private readonly string _bucket;

        public InfluxService(IConfiguration config)
        {
            var influxConfig = config.GetSection("InfluxDB");
            _org = influxConfig["Org"];
            _bucket = influxConfig["Bucket"];
            var url = influxConfig["Url"];
            var token = influxConfig["Token"];

            _client = new InfluxDBClient(url, token);

        }

        public async Task<List<FluxTable>> GetMetricsAsync()
        {
            var query = $"from(bucket: \"{_bucket}\") |> range(start: -1h)";
            var queryApi = _client.GetQueryApi();
            return await queryApi.QueryAsync(query, _org);
        }
    }
}
