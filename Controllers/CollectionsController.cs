using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoRest.Controllers;

[ApiController]
[Route($"{Constant.APIRootPath}/[controller]")]
public sealed class CollectionsController(IMongoDatabase database) : ControllerBase
{
    /// <summary>
    /// Creates a new document in the specified collection.
    /// </summary>
    /// <param name="collectionName">The name of the collection to create the document in.</param>
    /// <param name="document">The document to be created.</param>
    /// <returns>A successful result with a message indicating the document was created successfully.</returns>
    [HttpPost("{collectionName}/create")]
    public async Task<IActionResult> CreateAsync(string collectionName, [FromBody] BsonDocument document)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            return BadRequest("A collection name is required.");

        try
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            await collection.InsertOneAsync(document);

            return Ok(new { message = "Document created successfully." });   
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Insertion failed.", error = ex.Message });
        }
    }
}