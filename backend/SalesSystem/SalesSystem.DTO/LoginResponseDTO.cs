namespace SalesSystem.DTO
{
    public class LoginResponseDTO
    {
        public string Token { get; set; } = string.Empty;
        public SessionDTO Session { get; set; } = new SessionDTO();
    }
}
