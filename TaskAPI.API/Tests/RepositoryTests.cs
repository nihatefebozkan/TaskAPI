using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TaskAPI.Infrastructure.Data;
using TaskAPI.Infrastructure.Repositories;
using Xunit;
using EntityTask = TaskAPI.Domain.Entity.Task;
using User = TaskAPI.Domain.Entity.User;

namespace TaskAPI.API.Tests
{
    public class RepositoryTests : IDisposable
    {
        private readonly SqliteConnection connection;
        private readonly AppDbContext context;
        private readonly TaskRepository tasks;
        private readonly UserRepository users;

        public RepositoryTests()
        {
            connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;

            context = new AppDbContext(options);
            context.Database.EnsureCreated();

            tasks = new TaskRepository(context);
            users = new UserRepository(context);
        }

        private static EntityTask NewTask(string title = "Test task")
        {
            return new EntityTask(title, "Test description", DateTime.UtcNow, DateTime.UtcNow.AddDays(10));
        }

        [Fact]
        public async Task Add_PersistsTaskAndGeneratesId()
        {
            var entity = NewTask();

            var result = await tasks.Add(entity);

            Assert.Same(entity, result);
            Assert.True(entity.Id > 0);

            context.ChangeTracker.Clear();
            var saved = await context.Tasks.SingleAsync();

            Assert.Equal(entity.Id, saved.Id);
            Assert.Equal(entity.Title, saved.Title);
            Assert.Equal(entity.DueDate, saved.DueDate);
            Assert.Null(saved.DeletedAt);
        }

        [Fact]
        public async Task Get_ExistingTask_ReturnsStoredTask()
        {
            var entity = NewTask();
            context.Tasks.Add(entity);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var result = await tasks.Get(entity.Id);

            Assert.NotNull(result);
            Assert.Equal(entity.Id, result.Id);
            Assert.Equal(entity.Title, result.Title);
        }

        [Fact]
        public async Task Get_MissingTask_ReturnsNull()
        {
            Assert.Null(await tasks.Get(999999));
        }

        [Fact]
        public async Task GetAll_EmptyDatabase_ReturnsEmptyList()
        {
            Assert.Empty(await tasks.GetAll());
        }

        [Fact]
        public async Task GetAll_ReturnsStoredTasks()
        {
            context.Tasks.AddRange(NewTask("First"), NewTask("Second"));
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var result = await tasks.GetAll();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, task => task.Title == "First");
            Assert.Contains(result, task => task.Title == "Second");
        }

        [Fact]
        public async Task Update_PersistsChanges()
        {
            var entity = NewTask();
            context.Tasks.Add(entity);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            entity.DueDate = entity.DueDate.AddDays(5);

            await tasks.Update(entity);
            context.ChangeTracker.Clear();

            var saved = await context.Tasks.SingleAsync();

            Assert.Equal(entity.DueDate, saved.DueDate);
            Assert.Equal(entity.Title, saved.Title);
        }

        [Fact]
        public async Task Delete_KeepsRowAndRecordsDeletionTime()
        {
            var entity = NewTask();
            context.Tasks.Add(entity);
            await context.SaveChangesAsync();

            var before = DateTimeOffset.UtcNow;

            var result = await tasks.Delete(entity);

            Assert.Same(entity, result);

            context.ChangeTracker.Clear();
            var saved = await context.Tasks.SingleAsync();

            Assert.Equal(entity.Id, saved.Id);
            Assert.Equal("Test task (Deleted)", saved.Title);
            Assert.NotNull(saved.DeletedAt);
            Assert.InRange(saved.DeletedAt.Value, before, DateTimeOffset.UtcNow);
        }

        [Fact]
        public async Task GetAll_CurrentlyIncludesSoftDeletedRows()
        {
            var entity = NewTask();
            context.Tasks.Add(entity);
            await context.SaveChangesAsync();

            await tasks.Delete(entity);
            context.ChangeTracker.Clear();

            var result = await tasks.GetAll();

            var saved = Assert.Single(result);
            Assert.NotNull(saved.DeletedAt);
        }

        [Fact]
        public async Task Add_DuplicatePrimaryKey_ReportsDatabaseError()
        {
            var first = NewTask();
            await tasks.Add(first);
            context.ChangeTracker.Clear();

            var duplicate = NewTask("Duplicate");
            duplicate.Id = first.Id;

            await Assert.ThrowsAsync<DbUpdateException>(() => tasks.Add(duplicate));
        }

        [Fact]
        public async Task AddUser_PersistsUserAndGeneratesId()
        {
            var user = new User("test-user", "stored-hash", DateTime.UtcNow);

            await users.AddAsync(user);

            Assert.True(user.Id > 0);

            context.ChangeTracker.Clear();
            var saved = await context.Users.SingleAsync();

            Assert.Equal("test-user", saved.Username);
            Assert.Equal("stored-hash", saved.PasswordHash);
        }

        [Fact]
        public async Task GetUserByUsername_ReturnsMatchingUser()
        {
            context.Users.Add(new User("test-user", "stored-hash", DateTime.UtcNow));
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            var result = await users.GetByUsername("test-user");

            Assert.NotNull(result);
            Assert.Equal("test-user", result.Username);
            Assert.Equal("stored-hash", result.PasswordHash);
        }

        [Fact]
        public async Task GetUserByUsername_MissingUser_ReturnsNull()
        {
            Assert.Null(await users.GetByUsername("missing-user"));
        }

        public void Dispose()
        {
            context.Dispose();
            connection.Dispose();
        }
    }
}