﻿using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoRest.Models;

namespace MongoRest.Controllers;

[ApiController]
[Route($"{Constants.APIRootPath}/[controller]/{{collectionName}}")]
public sealed class CollectionsController(IMongoDatabase database) : ControllerBase
{
    /// <summary>
    /// Inserts one or more documents into the specified collection.
    /// </summary>
    /// <param name="collectionName">The name of the collection to insert the documents into.</param>
    /// <param name="documents">One or more documents to be inserted.</param>
    /// <returns>A successful result with a message indicating the number of documents inserted.</returns>
    [HttpPost("insert")]
    public async Task<IActionResult> InsertAsync(string collectionName, [FromBody] List<BsonDocument> documents)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            return BadRequest("A collection name is required.");

        try
        {
            var collection = database.GetCollection<BsonDocument>(collectionName);
            await collection.InsertManyAsync(documents);

            return Ok(new { message = "Document inserted successfully." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Insertion failed.", error = ex.Message });
        }
    }
    
    /// <summary>
    /// Retrieves documents from the specified collection according to the filter.
    /// </summary>
    /// <param name="collectionName">The name of the collection to retrieve the documents from.</param>
    /// <param name="filter">The filter to apply to the documents to be retrieved.</param>
    /// <param name="limit">The maximum number of documents to be retrieved. Defaults to 100.</param>
    /// <returns>A successful result with the retrieved documents.</returns>
    [HttpPost("get")]
    public async Task<IActionResult> GetAsync(string collectionName, [FromBody] BsonDocument filter, [FromQuery] int limit = 100)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            return BadRequest("A collection name is required.");
        
        var collection = database.GetCollection<BsonDocument>(collectionName);
        var documents = await collection.Find(filter).Limit(limit).ToListAsync();
        
        return Ok(documents);
    }
    
    /// <summary>
    /// Updates documents in the specified collection according to the filter and update documents.
    /// </summary>
    /// <param name="collectionName">The name of the collection to update the documents in.</param>
    /// <param name="updateRequest">A request containing the filter and update documents.</param>
    /// <returns>A successful result with the number of documents matched and modified.</returns>
    [HttpPut("update")]
    public async Task<IActionResult> UpdateAsync(string collectionName, [FromBody] UpdateRequest updateRequest)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            return BadRequest("A collection name is required.");

        var collection = database.GetCollection<BsonDocument>(collectionName);
        
        var result =
            await collection.UpdateManyAsync(updateRequest.Filter, new BsonDocument("$set", updateRequest.Update));

        if (result.ModifiedCount is 0)
            return NotFound();

        return Ok();
    }

    /// <summary>
    /// Deletes one document from the specified collection according to the filter.
    /// </summary>
    /// <param name="collectionName">The name of the collection to delete the document from.</param>
    /// <param name="filter">The filter to apply to the document to be deleted.</param>
    /// <returns>A successful result with the number of documents deleted.</returns>
    [HttpPost("delete")]
    public async Task<IActionResult> DeleteAsync(string collectionName, [FromBody] BsonDocument filter)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            return BadRequest("A collection name is required.");

        var collection = database.GetCollection<BsonDocument>(collectionName);
        var result = await collection.DeleteManyAsync(filter);

        if (result.DeletedCount is 0)
            return NotFound(new { message = "Document not found." });

        return Ok();
    }

    /// <summary>
    /// Retrieves the count of all documents in the specified collection that match the specified filter.
    /// </summary>
    /// <param name="collectionName">The name of the collection to count the documents in.</param>
    /// <param name="filter">The filter to apply to the documents to be counted.</param>
    /// <returns>A successful result with the count of documents matching the specified filter.</returns>
    [HttpPost("count")]
    public async Task<IActionResult> CountAllAsync(string collectionName, [FromBody] BsonDocument filter)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            return BadRequest("A collection name is required.");

        var collection = database.GetCollection<BsonDocument>(collectionName);
        var count = await collection.CountDocumentsAsync(filter);

        return Ok(count);
    }
}