using Elastic.Clients.Elasticsearch;
using LabWeb.SettingOptions;

namespace LabWeb.Services
{
    public class ElasticService
    {
        private readonly ElasticsearchClient _client;
        private readonly ElasticSettings _elasticSettings;
    }
}
