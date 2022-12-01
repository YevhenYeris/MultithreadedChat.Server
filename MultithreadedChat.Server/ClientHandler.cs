using System.Collections;
using System.Net.Sockets;

namespace MultithreadedChat.Server;

public class ClientHandler
{
    private TcpClient _client;
    private string _clientName;
    private Hashtable _chatClients;

    public void StartClient(
        TcpClient client,
        string clientName,
        Hashtable chatClients)
    {
        _client= client;
        _clientName= clientName;
        _chatClients= chatClients;
        Thread chatThread = new Thread(StartChat);
        chatThread.Start();
    }

    private void StartChat()
    {
        var receivedBytes = new byte[] { };
        var receivedMessage = string.Empty;

        while ((true))
        {
            try
            {
                NetworkStream networkStream = _client.GetStream();
                receivedBytes = new byte[_client.ReceiveBufferSize];
                networkStream.Read(receivedBytes, 0, _client.ReceiveBufferSize);
                receivedMessage = System.Text.Encoding.ASCII.GetString(receivedBytes);

                var allData = receivedMessage.Split('$', StringSplitOptions.TrimEntries);
                string? receiver = null;

                if (!string.IsNullOrEmpty(allData.First()))
                {
                    receiver = allData.FirstOrDefault();
                }

                receivedMessage = allData[1];
                Console.WriteLine("From client - " + _clientName + " : " + receivedMessage);

                lock (ChatServer.queueLocker)
                {
                    ChatServer.Messages.Enqueue(new Message
                    {
                        Data = receivedMessage,
                        Sender = _clientName,
                        IsJoinMessage = false,
                        Receiver = receiver,
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
