using Microsoft.AspNetCore.Mvc;
using TaskAPI.Entities.Interfaces;
using TaskAPI.Entities.Dtos;
using TaskAPI.Entities.Entity;
using TaskAPI.Service.Services;

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
        public IActionResult Get(int id)
        {
            var task = _taskService.Get(id);
            if (task == null)
                return NotFound();

            return Ok(task);
        }
        [HttpPost]
        public IActionResult Create(TaskCreateDto dto)
        {
            var task = _taskService.Create(dto);
            return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
        }
        [HttpPut("{id}")]
        public IActionResult Update(int id, TaskUpdateDto dto)
        {
            var task = _taskService.Update(id, dto);
            return Ok(task);
        }
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var result = _taskService.Delete(id);
            if (!result)
                return NotFound();
            return NoContent();
        }
        [HttpGet]
        public IActionResult GetAll()
        {
            var tasks = _taskService.GetAll();
            return Ok(tasks);
        }
    }
}
