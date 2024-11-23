using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoRest.Models;

namespace MongoRest.Controllers;

[ApiController]
[Route($"{Constants.APIRootPath}/[controller]/{{collectionName}}")]
public sealed class CollectionsController(IMongoDatabase database) : ControllerBase
{
    /// <summary>
    /// Creates a new document in the specified collection.
    /// </summary>
    /// <param name="collectionName">The name of the collection to create the document in.</param>
    /// <param name="document">The document to be created.</param>
    /// <returns>A successful result with a message indicating the document was created successfully.</returns>
    [HttpPost("create")]
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
    [HttpGet("get")]
    public async Task<IActionResult> GetAsync(string collectionName, [FromQuery] string? id = null)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            return BadRequest("A collection name is required.");

        var collection = database.GetCollection<BsonDocument>(collectionName);

        if (id is not null)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
            var document = await collection.Find(filter).FirstOrDefaultAsync();

            if (document is null)
                return NotFound(new { message = "Document not found." });

            return Ok(document);
        }

        var documents = await collection.Find(Builders<BsonDocument>.Filter.Empty).ToListAsync();
        return Ok(documents);
    }

    /// <summary>
    /// Updates documents in the specified collection according to the filter and update documents.
    /// </summary>
    /// <param name="collectionName">The name of the collection to update the documents in.</param>
    /// <param name="updateRequest">A request containing the filter and update documents.</param>
    /// <returns>A successful result with the number of documents matched and modified.</returns>
    [HttpPost("update")]
    public async Task<IActionResult> UpdateAsync(string collectionName, [FromBody] UpdateRequest updateRequest)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            return BadRequest("A collection name is required.");

        var collection = database.GetCollection<BsonDocument>(collectionName);
        var result =
            await collection.UpdateManyAsync(updateRequest.Filter, new BsonDocument("$set", updateRequest.Update));

        return Ok(new
        {
            message = "Update successful.",
            matchedCount = result.MatchedCount,
            modifiedCount = result.ModifiedCount
        });
    }
    
    /// <summary>
    /// Deletes documents from the specified collection according to the filter.
    /// </summary>
    /// <param name="collectionName">The name of the collection to delete the documents from.</param>
    /// <param name="id">The id of the document to be deleted. If not provided, all documents matching the filter will be deleted.</param>
    /// <returns>A successful result with the number of documents deleted.</returns>
    [HttpDelete("delete/{id}")]
    public async Task<IActionResult> DeleteByIdAsync(string collectionName, string id)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            return BadRequest("A collection name is required.");

        var collection = database.GetCollection<BsonDocument>(collectionName);
        var filter = Builders<BsonDocument>.Filter.Eq("_id", id);
        var result = await collection.DeleteManyAsync(filter);

        return Ok(new
        {
            message = "Delete successful.",
            deletedCount = result.DeletedCount
        });
    }
}