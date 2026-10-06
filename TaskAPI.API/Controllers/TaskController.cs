using Microsoft.AspNetCore.Mvc;
using TaskAPI.Domain.Entity;
using TaskAPI.Application.Services;
using TaskAPI.Core.Helpers;
using TaskAPI.Core.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using TaskAPI.Application.Dtos;
using TaskAPI.Application.Interfaces;
using TaskAPI.Core.Models;

namespace TaskAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController(ITaskService taskService, ILogger<TaskController> logger,IHttpContextAccessor httpContextAccessor) : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id) //dto gidecek
        {
            return await MethodExecutor.ExecuteAsync<IActionResult>(
                async () =>
                {
                    var task = await taskService.Get(id) ?? throw new ArgumentException("Task not found.");
                    
                    return Ok(task);

                },logger, httpContextAccessor, nameof(Get));
            }

        [HttpPost]
        public async Task<IActionResult> Create(TaskDto dto)
        {
            return await MethodExecutor.ExecuteAsync(
                async() =>
                {
                    var task = await taskService.Add(dto) ?? throw new ArgumentException("Failed to create task.");
                    
                    return Ok(task);

                },logger, httpContextAccessor, nameof(Create));
            
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TaskDto dto)
        {
            return await MethodExecutor.ExecuteAsync(
                async () =>
                {
                    var task = await taskService.Update(id, dto) ?? throw new ArgumentException("Task not found.");
                   
                    return Ok(task);

                }, logger, httpContextAccessor, nameof(Update));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) //dto gidecek
        {
            return await MethodExecutor.ExecuteAsync(
                async () => 
                {
                    var deletedTask = await taskService.Delete(id); 
                        if(!deletedTask)
                            throw new ArgumentException("Task not found.");
                    
                    return Ok(deletedTask);
                
                }, logger, httpContextAccessor, nameof(Delete));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return await MethodExecutor.ExecuteAsync(
                async () => 
                {
                    await System.Threading.Tasks.Task.Delay(1000);
                    
                    var tasks = await taskService.GetAll() ?? throw new ArgumentException("No tasks found.");

                    return Ok(tasks);

                }, logger, httpContextAccessor, nameof(GetAll));
        }
    }
}
