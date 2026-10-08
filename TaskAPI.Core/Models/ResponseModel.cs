namespace TaskAPI.Core.Models
{
    public class ResponseModel<T>
    {
        public bool Success { get; set; }
        public T? Result { get; set; }
        public ErrorModel? Error { get; set; } = null;
    }
}
