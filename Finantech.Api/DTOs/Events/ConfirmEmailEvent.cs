using Finantech.DTOs.User;

namespace Finantech.DTOs.Events
{
    public class ConfirmEmailEvent
    {
        public InfoUserResponse User { get; set; }

        public ConfirmEmailEvent(InfoUserResponse user)
        {
            this.User = user;
        }
    }
}
