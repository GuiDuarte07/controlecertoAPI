namespace ControleCerto.DTOs.Events
{
    public class ConsoleMessageEvent
    {
        public string Message {  get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();

        public ConsoleMessageEvent(string message) 
        {
            this.Message = message;
        }
    }
}
