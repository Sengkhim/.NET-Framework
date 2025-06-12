namespace Web3.Service
{
    public interface IMessageService
    {
        string GetMessage();
    }

    public class MessageService : IMessageService
    {
        public string GetMessage() => "Hello from DI!";
    }
}