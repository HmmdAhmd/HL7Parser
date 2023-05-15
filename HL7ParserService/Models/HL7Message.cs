using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace HL7ParserService.Models
{
    public class HL7Message
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Username { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string FacilityId { get; set; } = null!;

        public string Message { get; set; } = null!;

        public DateTime CreatedDate { get; set; }
    }
}
