namespace TaskAPI.Application.Dtos
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; }
        public DateTime ExpireAt { get; set; }
    }
}
