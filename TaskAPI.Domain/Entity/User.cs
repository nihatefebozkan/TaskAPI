namespace TaskAPI.Domain.Entity
{
    public class User
    {
        public User(string username, string passwordHash, DateTime createdAt)
        {
            Username = username;
            PasswordHash = passwordHash;
            CreatedAt = createdAt;
        }
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

