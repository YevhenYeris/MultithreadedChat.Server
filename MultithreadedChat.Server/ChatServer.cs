using System.Collections;
using System.Net.Sockets;
using System.Text;

namespace MultithreadedChat.Server;

public class ChatServer
{
    private static Hashtable ChatClients = new Hashtable();
    private readonly int _port = 8000;

    public static readonly Queue<Message> Messages = new Queue<Message>();
    public static readonly object queueLocker = new object();

    public void StartServer()
    {
        var queueCheckerThread = new Thread(() =>
        {
            while (true)
            {
                lock (queueLocker)
                {
                    Message? message = null;
                    if (ChatServer.Messages.TryDequeue(out message))
                    {
                        SendMany(message);
                    }
                }
            }
        });

        queueCheckerThread.Start();

        var server = new TcpListener(_port);
        var client = default(TcpClient);
        server.Start();
        Console.WriteLine("Server Started ....");

        while ((true))
        {
            client = server.AcceptTcpClient();

            var receivedBytes = new byte[client.ReceiveBufferSize];
            var clientStream = client.GetStream();
            clientStream.Read(receivedBytes, 0, client.ReceiveBufferSize);

            var message = Encoding.ASCII.GetString(receivedBytes);
            message = message.Substring(0, message.IndexOf("$"));
            ChatClients.Add(message, client);

            lock(queueLocker)
            {
                ChatServer.Messages.Enqueue(new Message
                {
                    Data = $"{message} Joined The Chat",
                    Sender = message,
                    IsJoinMessage = true,
                });
            }
            
            Console.WriteLine($"{message} Joined The Chat");
            var clientHandler = new ClientHandler();
            clientHandler.StartClient(client, message, ChatClients);
        }

        client.Close();
        server.Stop();
        Console.WriteLine("Server Stopped");
        Console.ReadLine();
    }

    public static void SendMany(Message message)
    {
        if (message.Receiver is not null)
        {
            if (message.Receiver == message.Sender)
            {
                SendSingle((TcpClient)ChatClients[message.Receiver], message.IsJoinMessage, message.Sender, message.Data, message.Receiver);
                return;
            }

            var client1 = (TcpClient)ChatClients[message.Receiver];
            var client2 = (TcpClient)ChatClients[message.Sender];

            if (client1 is not null)
            {
                SendSingle(client1, message.IsJoinMessage, message.Sender, message.Data, message.Receiver);
                SendSingle(client2, message.IsJoinMessage, message.Sender, message.Data, message.Receiver);
            }
            return;
        }

        foreach (DictionaryEntry Item in ChatClients)
        {
            SendSingle((TcpClient)Item.Value, message.IsJoinMessage, message.Sender, message.Data);
        }
    }

    private static void SendSingle(
        TcpClient client,
        bool isJoinMessage,
        string sender,
        string message,
        string? receiver = null)
    {
        if (client is null)
        {
            return;
        }

        NetworkStream clientStream = client.GetStream();
        Byte[] messageBytes = null;

        if (!isJoinMessage)
        {
            messageBytes = Encoding.ASCII.GetBytes(
                $"From {sender}{(receiver is not null ? " to " + receiver : "")} : {message}");
        }
        else
        {
            messageBytes = Encoding.ASCII.GetBytes(message);
        }

        clientStream.Write(messageBytes, 0, messageBytes.Length);
        clientStream.Flush();
    }
}
