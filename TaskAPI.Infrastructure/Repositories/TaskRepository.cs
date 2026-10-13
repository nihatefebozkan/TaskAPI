using Microsoft.EntityFrameworkCore;
using TaskAPI.Application.Interfaces;
using TaskAPI.Infrastructure.Data;

namespace TaskAPI.Infrastructure.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;
        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<TaskAPI.Domain.Entity.Task>> GetAll(int userId)
        {
            return await _context.Tasks
                .Where(task => task.Users.Any(user => user.Id == userId))
                .ToListAsync();
        }
        public async Task<Domain.Entity.Task> Add(Domain.Entity.Task task) //bu metod, TaskAPI.Domain.Models.Entity.Task nesnesini alır ve veritabanına ekler. SaveChanges() metodu ile değişiklikleri kaydeder.
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task<Domain.Entity.Task?> Get(int id, int userId)
        {
            return await _context.Tasks
                .FirstOrDefaultAsync(task => task.Id == id && task.Users.Any(user => user.Id == userId));
        }
        public async Task<Domain.Entity.Task> Update(Domain.Entity.Task task) //bu metod, verilen TaskAPI.Domain.Models.Entity.Task nesnesini günceller ve SaveChanges() metodu ile değişiklikleri kaydeder.
        {
            _context.Tasks.Update(task);
            await _context.SaveChangesAsync();
            return task;
        }
        public async Task<Domain.Entity.Task> Delete(Domain.Entity.Task task) //bu metod, verilen TaskAPI.Domain.Models.Entity.Task nesnesini siler ve SaveChanges() metodu ile değişiklikleri kaydeder.
        {
            task.DeletedAt = DateTimeOffset.UtcNow;
            task.Title = string.Concat(task.Title, " (Deleted)");
            await _context.SaveChangesAsync();
            return task;
        }
    }
}
