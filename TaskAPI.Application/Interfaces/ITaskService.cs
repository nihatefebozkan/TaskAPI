using TaskAPI.Application.Dtos;

namespace TaskAPI.Application.Interfaces
{
    public interface ITaskService
    {
        Task<TaskDto> Add(TaskDto dto);
        Task<List<TaskDto>> GetAll(); // get
        Task<TaskDto?> Get(int id); // get
        Task<TaskDto> Update(int id, TaskDto dto); // update , put
        Task<bool> Delete(int id); // delete
    }
}
// errorcodeleri yoneticem tamma mı yani errorhandlerdeki yapıyı kullanmayacağım kendi olusturdugum errorcodesleri nasıl nerede yonetebilirim arastiricam