using System;
using System.Collections.Generic;
using System.Text;

namespace TaskAPI.Entities.Dtos
{
    public class TaskUpdateDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
    }
}
