using MongoDB.Bson;

namespace MongoRest.Models;

public record UpdateRequest(BsonDocument Filter, BsonDocument Update);