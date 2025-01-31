using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;

public class SaveToCosmos
{
    private readonly CosmosClient _cosmosClient;

    public SaveToCosmos(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
    }

    [Function("SaveToCosmos")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<dynamic>(requestBody);
        if (data == null)
            return new BadRequestObjectResult("Dados inválidos.");

        var container = _cosmosClient.GetContainer("database_name", "container_name");
        await container.CreateItemAsync(data);

        return new OkObjectResult("Registro salvo com sucesso no CosmosDB!");
    }
}
