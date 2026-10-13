namespace TaskAPI.Application.Dtos
{
    public class LoginResponseDto
    {
        public required string AccessToken { get; set; }
        public required DateTime ExpireAt { get; set; }
    }
}
