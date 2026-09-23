using System;
using System.Collections.Generic;
using System.Text;
using TaskAPI.Entities.Dtos; // service katmanı için gerekli olan DTO sınıflarını içe aktarır

namespace TaskAPI.Entities.Interfaces
{
    public interface ITaskService
    {
        TaskDto Create(TaskCreateDto dto); // post
        List<TaskDto> GetAll(); // get
        TaskDto Get(int id); // get
        TaskDto Update(int id, TaskUpdateDto dto); // update , put
        bool Delete(int id); // delete
        
    }
}
