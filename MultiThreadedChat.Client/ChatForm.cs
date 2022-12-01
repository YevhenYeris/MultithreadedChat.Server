using System.Net.Sockets;
using System.Text;

namespace MultiThreadedChat.Client
{
    public partial class ChatForm : Form
    {
        private readonly string _hostName = "127.0.0.1";
        private readonly int _port = 8000;

        private TcpClient _server = new TcpClient();
        private NetworkStream _stream = default(NetworkStream)!;
        private string _allChat = string.Empty;

        public ChatForm()
        {
            InitializeComponent();
        }

        private void joinChatButton_Click(object sender, EventArgs e)
        {
            _allChat = "Conected to Chat ...";
            UpdateChat();
            _server.Connect(_hostName, _port);
            _stream = _server.GetStream();

            var stream = Encoding.ASCII.GetBytes(nameTextBox.Text + "$");
            _stream.Write(stream, 0, stream.Length);
            _stream.Flush();

            Thread ctThread = new Thread(getMessage);
            ctThread.Start();
        }

        private void getMessage()
        {
            while (true)
            {
                _stream = _server.GetStream();
                var inStream = new byte[_server.ReceiveBufferSize];
                var bufferSize = _server.ReceiveBufferSize;
                _stream.Read(inStream, 0, bufferSize);
                var returndata = Encoding.ASCII.GetString(inStream);
                _allChat = "" + returndata;
                UpdateChat();
            }
        }

        private void UpdateChat()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(UpdateChat));
            }
            else
            {
                chatTextBox.Text = chatTextBox.Text + "\n" + _allChat;
            }
        }

        private void sendMessageButton_Click(object sender, EventArgs e)
        {
            var streamIn = Encoding.ASCII.GetBytes(receiverTextBox.Text + "$");
            var streamOut = Encoding.ASCII.GetBytes(messageTextBox.Text + "$");

            _stream.Write(streamIn.Concat(streamOut).ToArray(), 0, streamIn.Length + streamOut.Length);
            _stream.Flush();
        }
    }
}