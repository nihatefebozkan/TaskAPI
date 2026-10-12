using TaskAPI.Application.Dtos;
using Xunit;
using EntityTask = TaskAPI.Domain.Entity.Task;
using User = TaskAPI.Domain.Entity.User;

namespace TaskAPI.API.Tests
{
    public class MappingTests
    {
        [Fact]
        public void EntityToDto_MapsAllResponseFields()
        {
            var entity = TestSupport.CreateEntity();
            entity.IsCompleted = true;
            entity.DeletedAt = DateTimeOffset.UtcNow;

            var result = TestSupport.CreateMapper().Map<TaskDto>(entity);

            Assert.Equal(entity.Id, result.Id);
            Assert.Equal(entity.Title, result.Title);
            Assert.Equal(entity.Description, result.Description);
            Assert.Equal(entity.CreatedAt, result.CreatedAt);
            Assert.Equal(entity.DueDate, result.DueDate);
            Assert.Equal(entity.IsCompleted, result.IsCompleted);
        }

        [Fact]
        public void DtoToEntity_UsesServerCreationTimeAndIgnoresProtectedFields()
        {
            var dto = TestSupport.CreateDto();
            dto.Id = 999;
            dto.CreatedAt = DateTime.UtcNow.AddYears(-20);
            dto.IsCompleted = true;

            var before = DateTime.UtcNow;
            var entity = TestSupport.CreateMapper().Map<EntityTask>(dto);

            Assert.Equal(dto.Title, entity.Title);
            Assert.Equal(dto.Description, entity.Description);
            Assert.Equal(dto.DueDate, entity.DueDate);
            Assert.Equal(0, entity.Id);
            Assert.False(entity.IsCompleted);
            Assert.Null(entity.DeletedAt);
            Assert.InRange(entity.CreatedAt, before, DateTime.UtcNow);
        }

        [Fact]
        public void NullStrings_AreConvertedToEmptyStrings()
        {
            var dto = TestSupport.CreateDto();
            dto.Title = null;
            dto.Description = null;

            var entity = TestSupport.CreateMapper().Map<EntityTask>(dto);

            Assert.Equal(string.Empty, entity.Title);
            Assert.Equal(string.Empty, entity.Description);
        }

        [Fact]
        public void EntityList_IsMappedToDtoList()
        {
            var entities = new List<EntityTask>
            {
                TestSupport.CreateEntity(),
                TestSupport.CreateEntity()
            };

            entities[1].Id = 8;

            var result = TestSupport.CreateMapper().Map<List<TaskDto>>(entities);

            Assert.Equal(2, result.Count);
            Assert.Equal(7, result[0].Id);
            Assert.Equal(8, result[1].Id);
        }
    }

    public class UserTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void InvalidUsername_IsRejected(string? username)
        {
            var exception = Assert.Throws<ArgumentException>(() => new User(username!, "hash", DateTime.UtcNow));

            Assert.Equal("Kullanıcı adı boş olamaz.", exception.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void InvalidPasswordHash_IsRejected(string? passwordHash)
        {
            var exception = Assert.Throws<ArgumentException>(() => new User("test-user", passwordHash!, DateTime.UtcNow));

            Assert.Equal("Şifre boş olamaz.", exception.Message);
        }
    }
}