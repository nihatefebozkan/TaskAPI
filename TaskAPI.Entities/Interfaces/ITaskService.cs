using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TaskAPI.Entities.Dtos; // service katmanı için gerekli olan DTO sınıflarını içe aktarır

namespace TaskAPI.Entities.Interfaces
{
    public interface ITaskService
    {
        Task<TaskDto> AddAsync(TaskCreateDto dto);
        Task<List<TaskDto>> GetAllAsync(); // get
        Task<TaskDto> GetAsync(int id); // get
        Task<TaskDto> UpdateAsync(int id, TaskUpdateDto dto); // update , put
        Task<bool> DeleteAsync(int id); // delete
    }
}
