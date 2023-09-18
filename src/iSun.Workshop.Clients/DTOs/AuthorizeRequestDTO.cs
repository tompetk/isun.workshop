namespace iSun.Workshop.Clients.DTOs
{
    public class AuthorizeRequestDTO
    {
        public AuthorizeRequestDTO(string username, string password)
        {
            Username = username;
            Password = password;
        }

        public string Username { get; }
        public string Password { get; }
    }
}
