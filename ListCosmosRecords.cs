using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Text.Json;

public class ListCosmosRecords
{
    private readonly CosmosClient _cosmosClient;

    public ListCosmosRecords(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    [Function("ListCosmosRecords")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
        var container = _cosmosClient.GetContainer("database_name", "container_name");

        var query = new QueryDefinition("SELECT * FROM c");
        var iterator = container.GetItemQueryIterator<dynamic>(query);
        var results = new List<dynamic>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response);
        }

        return new OkObjectResult(JsonSerializer.Serialize(results));
    }
}
