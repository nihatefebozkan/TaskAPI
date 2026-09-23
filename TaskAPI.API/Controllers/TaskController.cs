using Microsoft.AspNetCore.Mvc;
using TaskAPI.Entities.Interfaces;
using TaskAPI.Entities.Dtos;
using TaskAPI.Entities.Entity;
using TaskAPI.Service.Services;
using TaskAPI.Core.Helpers;
using TaskAPI.Core.Middleware;

namespace TaskAPI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        public TaskController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var task = await _taskService.GetAsync(id);
            if (task == null)
            {
                return NotFound(new ResponseModel<TaskDto>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.NotFound, "Task not found.")
                });
            }
            else
            {
                var response = new ResponseModel<TaskDto>
                {
                    Success = true,
                    Result = task
                };
                return Ok(response);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskCreateDto dto)
        {
            var result = await _taskService.AddAsync(dto);
            if (result == null)
            {           
                return BadRequest(new ResponseModel<TaskCreateDto>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.BadRequest, "Invalid task data.")
                });
            }
            else
            {
                var response = new ResponseModel<TaskDto>
                {
                    Success = true,
                    Result = result
                };
                return CreatedAtAction(nameof(Get), new { id = result.Id }, response);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TaskUpdateDto dto)
        {
            var result = await _taskService.UpdateAsync(id, dto);
            if (result == null)
            {
                return NotFound(new ResponseModel<TaskUpdateDto>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.NotFound, "Task not found.")
                });
            }
            else
            {
                var response = new ResponseModel<TaskDto>
                {
                    Success = true,
                    Result = result
                };
                return Ok(response);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _taskService.DeleteAsync(id);

            if (!result)
                return NotFound(new ResponseModel<bool>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.NotFound, "Task not found.")
                });
            else
            {
                var response = new ResponseModel<bool>
                {
                    Success = true,
                    Result = result
                };
                return Ok(response);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _taskService.GetAllAsync();
            if (result == null || result.Count == 0)
            {
                return NotFound(new ResponseModel<List<TaskDto>>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.NotFound, "No tasks found.")
                });
            }
            var response = new ResponseModel<IEnumerable<TaskDto>>
            {
                Success = true,
                Result = result
            };
            return Ok(response);
        }
    }
}
