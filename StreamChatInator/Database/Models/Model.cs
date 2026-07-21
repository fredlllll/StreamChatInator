using System.ComponentModel.DataAnnotations;
using UUIDNext;

namespace StreamChatInator.Database.Models
{
    public abstract class Model : IEquatable<Model?>
    {
        public Model()
        {
            Created = Updated = DateTime.UtcNow;
        }
        [Key]
        public required string Id { get; set; }
        public DateTime Created { get; set; }
        public DateTime Updated { get; set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Model);
        }

        public bool Equals(Model? other)
        {
            return other is not null &&
                   Id == other.Id &&
                   Created == other.Created &&
                   Updated == other.Updated;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Created, Updated);
        }

        public static bool operator ==(Model? left, Model? right)
        {
            return EqualityComparer<Model>.Default.Equals(left, right);
        }

        public static bool operator !=(Model? left, Model? right)
        {
            return !(left == right);
        }

        public static string GetNewId<T>() where T:Model
        {
            var t = typeof(T);
            var typeId = t.Name.ToLower();
            var id = Uuid.NewDatabaseFriendly(UUIDNext.Database.SQLite);
            return $"{typeId}_{id}";
        }
    }
}
