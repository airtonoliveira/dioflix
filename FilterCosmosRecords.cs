using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Text.Json;

public class FilterCosmosRecords
{
    private readonly CosmosClient _cosmosClient;

    public FilterCosmosRecords(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    [Function("FilterCosmosRecords")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
        ILogger log)
    {
        string filterField = req.Query["field"];
        string filterValue = req.Query["value"];

        if (string.IsNullOrEmpty(filterField) || string.IsNullOrEmpty(filterValue))
            return new BadRequestObjectResult("Parâmetros inválidos.");

        var container = _cosmosClient.GetContainer("database_name", "container_name");

        var query = new QueryDefinition($"SELECT * FROM c WHERE c.{filterField} = @value")
                    .WithParameter("@value", filterValue);

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
