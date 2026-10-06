using AutoMapper;
using TaskAPI.Application.Dtos;

namespace TaskAPI.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TaskAPI.Domain.Entity.Task, TaskDto>();

            CreateMap<TaskDto, TaskAPI.Domain.Entity.Task>()
                .ConstructUsing(dto => new TaskAPI.Domain.Entity.Task(dto.Title ?? string.Empty, dto.Description ?? string.Empty, DateTime.UtcNow, dto.DueDate))
                .ForMember(task => task.Id, option => option.Ignore())
                .ForMember(task => task.CreatedAt, option => option.Ignore())
                .ForMember(task => task.IsCompleted, option => option.Ignore())
                .ForMember(task => task.DeletedAt, option => option.Ignore());
        }
    }
}