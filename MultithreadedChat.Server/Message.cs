namespace MultithreadedChat.Server;

public class Message
{
    public string Data { get; set; }

    public string Sender { get; set; }

    public bool IsJoinMessage { get; set; }

    public string? Receiver { get; set; }
}
