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
    public class TaskController(ITaskService taskService, ILogger<TaskController> logger) : ControllerBase
    {
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var response = await OperationExecutor.ExecuteAsync(async () => await taskService.GetAsync(id), logger, HttpContext, "Get Task");
            if (!response.Success)
            {
                return StatusCode(500, response);
            }
            if (response.Result == null)
            {
                return NotFound(new ResponseModel<TaskDto>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.NotFound, "Task not found.")
                });
            }
            return Ok(response);

            //var task = await taskService.GetAsync(id);
            //if (task == null)
            //{
            //    return NotFound(new ResponseModel<TaskDto>
            //    {
            //        Success = false,
            //        Error = new Error(ErrorCodes.NotFound, "Task not found.")
            //    });
            //}
            //else
            //{
            //    var response = new ResponseModel<TaskDto>
            //    {
            //        Success = true,
            //        Result = task
            //    };
            //    return Ok(response);
            //}
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskCreateDto dto)
        {
            var response = await OperationExecutor.ExecuteAsync(async () => await taskService.AddAsync(dto), logger, HttpContext, "Create Task");
            if (!response.Success)
            {
                return StatusCode(500, response);
            }
            if (response.Result == null)
            {
                return NotFound(new ResponseModel<TaskCreateDto>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.NotFound, "Task couldn't be Created")
                });
            }

            return Ok(response);

            //var result = await taskService.AddAsync(dto);
            //if (result == null)
            //{           
            //    return BadRequest(new ResponseModel<TaskCreateDto>
            //    {
            //        Success = false,
            //        Error = new Error(ErrorCodes.BadRequest, "Invalid task data.")
            //    });
            //}
            //else
            //{
            //    var response = new ResponseModel<TaskDto>
            //    {
            //        Success = true,
            //        Result = result
            //    };
            //    return CreatedAtAction(nameof(Get), new { id = result.Id }, response);
            //}
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TaskUpdateDto dto)
        {
            var response = await OperationExecutor.ExecuteAsync(async () => await taskService.UpdateAsync(id, dto), logger, HttpContext, "Update Task"); //response niye tanımladın dedi
            if (!response.Success)
            {
                return StatusCode(500, "response");
            }
            if (response.Result == null)
            {
                return NotFound(new ResponseModel<TaskUpdateDto>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.NotFound, "The task could not be updated.")
                });
            }
            return Ok(response);


            //var result = await taskService.UpdateAsync(id, dto);
            //if (result == null)
            //{
            //    return NotFound(new ResponseModel<TaskUpdateDto>
            //    {
            //        Success = false,
            //        Error = new Error(ErrorCodes.NotFound, "Task not found.")
            //    });
            //}
            //else
            //{
            //    var response = new ResponseModel<TaskDto>
            //    {
            //        Success = true,
            //        Result = result
            //    };
            //    return Ok(response);
            //}
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await OperationExecutor.ExecuteAsync(async () => taskService.DeleteAsync(id), logger, HttpContext, "Task Delete");
            if (!response.Success)
            {
                return StatusCode(500, response); //error = new error(errorcodes.internalservererror,"test")
            }
            if (response.Result == null)
            {
                return NotFound(new ResponseModel<TaskDto>
                {
                    Success = false,
                    Error = new Error(ErrorCodes.NotFound, "Task couldn't be Deleted")
                });
            }
            return Ok(response);
            
            //var result = await taskService.DeleteAsync(id);

            //if (!result)
            //    return NotFound(new ResponseModel<bool>
            //    {
            //        Success = false,
            //        Error = new Error(ErrorCodes.NotFound, "Task not found.")
            //    });
            //else
            //{
            //    var response = new ResponseModel<bool>
            //    {
            //        Success = true,
            //        Result = result
            //    };
            //    return Ok(response);
            //}
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var response = await OperationExecutor.ExecuteAsync(async () => taskService.GetAllAsync(), logger, HttpContext, "Get All Task");
            {
                if (!response.Success)
                {
                    return StatusCode(500, response);
                }
                if (response.Result == null)
                {
                    return NotFound(new ResponseModel<TaskDto>
                    {
                        Success = false,
                        Error = new Error(ErrorCodes.NotFound, "No Tasks Found")
                    });
                }
                return Ok(response);
            }
            //var result = await taskService.GetAllAsync();
            //if (result == null || result.Count == 0)
            //{
            //    return NotFound(new ResponseModel<List<TaskDto>>
            //    {
            //        Success = false,
            //        Error = new Error(ErrorCodes.NotFound, "No tasks found.")
            //    });
            //}
            //var response = new ResponseModel<IEnumerable<TaskDto>>
            //{
            //    Success = true,
            //    Result = result
            //};
            //return Ok(response);
        }
    }
}
