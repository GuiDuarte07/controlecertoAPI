using Finantech.DTOs.Events;
using MassTransit;

namespace Finantech.Bus
{
    public class ConsoleMessageEventConsumer: IConsumer<ConsoleMessageEvent>
    {
        public async Task Consume(ConsumeContext<ConsoleMessageEvent> context)
        {
            Console.WriteLine("Processando mensagem...");
            await Task.Delay(1000);
            Console.WriteLine(context.Message.Id + " = " + context.Message.Message);
        }
    }
}
