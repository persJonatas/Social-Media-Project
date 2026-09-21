using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CisApi.Data.Entities;

public class Topic
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Idea> Ideas { get; set; } = [];
}
