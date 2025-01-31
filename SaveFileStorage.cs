using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using System;

public class SaveFileToStorage
{
    private readonly BlobServiceClient _blobServiceClient;

    public SaveFileToStorage(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    [Function("SaveFileToStorage")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
        ILogger log)
    {
        var file = req.Form.Files["file"];
        if (file == null)
            return new BadRequestObjectResult("Arquivo não encontrado.");

        var containerClient = _blobServiceClient.GetBlobContainerClient("uploads");
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(file.FileName);
        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, true);
        }

        return new OkObjectResult($"Arquivo {file.FileName} salvo com sucesso!");
    }
}
