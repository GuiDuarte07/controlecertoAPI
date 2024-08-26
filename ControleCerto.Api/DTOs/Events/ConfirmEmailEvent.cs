using ControleCerto.DTOs.User;

namespace ControleCerto.DTOs.Events
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
