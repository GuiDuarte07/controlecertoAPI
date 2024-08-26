namespace ControleCerto.DTOs.Events
{
    public class ForgotPasswordEvent(string email)
    {
        public string Email { get; set; } = email;  
    }
}
