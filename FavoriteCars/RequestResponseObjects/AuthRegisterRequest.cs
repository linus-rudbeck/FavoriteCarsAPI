namespace FavoriteCars.RequestResponseObjects
{
    public class AuthRegisterRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
