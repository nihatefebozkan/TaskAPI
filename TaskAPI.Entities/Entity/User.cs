using System;
using System.Collections.Generic;
using System.Text;

namespace TaskAPI.Entities.Entity
{
    public class User
    {
        public User(
            string username, string passwordHash, DateTime createdAt)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
              throw new ArgumentException("Kullanıcı adı boş olamaz.");
            }
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                throw new ArgumentException("Şifre boş olamaz.");
            }
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

