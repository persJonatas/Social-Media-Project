using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CisApi.Data.Entities;

public class Vote
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = string.Empty;
    public string IdeaId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}