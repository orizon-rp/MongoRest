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

    /// <summary>
    /// Retrieves a document from the specified collection by id.
    /// </summary>
    /// <param name="collectionName">The name of the collection to retrieve the document from.</param>
    /// <param name="id">The id of the document to be retrieved.</param>
    /// <returns>A successful result with the document if found, otherwise a 404 not found error.</returns>
    [HttpGet("{collectionName}")]
    public async Task<IActionResult> GetAsync(string collectionName, [FromQuery] string? id = null)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            return BadRequest("A collection name is required.");

        var collection = database.GetCollection<BsonDocument>(collectionName);

        if (id is not null)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", ObjectId.Parse(id));
            var document = await collection.Find(filter).FirstOrDefaultAsync();

            if (document is null)
                return NotFound(new { message = "Document not found." });

            return Ok(document);
        }

        var documents = await collection.Find(Builders<BsonDocument>.Filter.Empty).ToListAsync();
        return Ok(documents);
    }
}