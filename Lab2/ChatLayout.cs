using System.Net;
using Timer = System.Timers.Timer;

namespace Lab2;

public partial class ChatLayout : UserControl
{
    private readonly UdpService _udpService;
    private readonly TcpService _tcpService;
    private readonly UserData _localUser;
    private Dictionary<IPAddress, string> _userTable;
    private readonly List<Message> _messages;
    private readonly Stack<Task> _tasks;
    private readonly DateTime _startTime;
    
    
    public event Action? OnExit;
    
    public ChatLayout(UserData userData)
    {
        _localUser = userData;
        _userTable = new Dictionary<IPAddress, string>();
        _udpService = new UdpService(_localUser, _userTable);
        _messages = new List<Message>();
        _tcpService = new TcpService(_localUser, _messages, _userTable);
        _udpService.UserDiscovered += OnUserDiscovered;
        _tcpService.MessageReceived += OnMessageReceived;
        _tcpService.OnConnect += RepaintUserListHandler;
        _tasks = new Stack<Task>();
        _startTime = DateTime.Now;
        InitializeComponent();
    }

    public async Task DisableChat()
    {
        // получаем текущее время 
        DateTime time = DateTime.Now;
        // создаем сообщение
        Message sendMessage = new Message(
            _localUser.username,
            "покинул чат\r\n",
            time
        );
        // отправляем сообщение
        await _tcpService.SendDialogMessageAsync(sendMessage, MessageType.TCP_DISCONNECT);
        _udpService.Disable();
        Task.Delay(300).Wait();
        _tcpService.Disable();
        while (_tasks.Count > 0)
        {
            await _tasks.Pop();
        }
    }

    private void ChatLayout_Load(object sender, EventArgs e)
    {
        lb_info.Text += $"\n{_localUser.username}\n{_localUser.ipAddress}";
        _= InitializeUdpAsync();
    }

    private void tb_message_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            if (e.Shift)
            {
                // Shift + Enter => перенос строки
                int position = tb_message.SelectionStart;
                tb_message.Text = tb_message.Text.Insert(position, Environment.NewLine);
                tb_message.SelectionStart = position + Environment.NewLine.Length;
                e.SuppressKeyPress = true;
            }
            else
            {
                btn_send.PerformClick();
                e.SuppressKeyPress = true;
            }
        }
    }

    private async Task InitializeUdpAsync()
    {
        // запустить прослушку udp
        _tasks.Push(Task.Run(() => _udpService.ListenBroadcastAsync()));
        // отсылаем udp broadcast
        await _udpService.SendBroadcastAsync();
        // запускаем прослушку tcp
        _tasks.Push(Task.Run(() => _tcpService.ListenAsync()));
    }

    private void OnUserDiscovered(IPAddress address)
    {
        
        // начинаем подключение
        Task.Run(async () =>
        {
            try
            {
                // подключаемся к обнаруженному пользователю
                await _tcpService.ConnectAsync(address);
                // формируем сообщение с информацией о входе
                Message message = new Message(_localUser.username, "присоединился к чату\r\n", _startTime);
                await _tcpService.SendJoinMessage(message);
                //btn_send.Enabled = false;
                await _tcpService.RequestHistoryAsync();
                //btn_send.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
            }
        });
    }
    
    private void OnMessageReceived(string message)
    {
        // Добавляем в UI через Invoke
        if (tb_dialog.InvokeRequired)
        {
            tb_dialog.Invoke(new Action(() =>
            {
                tb_dialog.AppendText(message);
            }));
        }
        else
        {
            tb_dialog.AppendText(message);
        }
    }

    private void btn_send_Click(object sender, EventArgs e)
    {
        // получаем текущее время 
        DateTime time = DateTime.Now;
        // получаем сообщение из TextBox
        string text = tb_message.Text;
        Message sendMessage = new Message(
            _localUser.username,
            text + "\r\n",
            time
        );
        // записываем сообщение в UI
        tb_dialog.AppendText(sendMessage.ToString());
        // записываем в массив сообщений
        _messages.Add(sendMessage);
        // очищаем TextBox
        tb_message.Text = string.Empty;
        // отправляем сообщение
        Task.Run(async () =>
        {
            try
            {
                await _tcpService.SendDialogMessageAsync(sendMessage, MessageType.TCP_DIALOG);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection error: {ex.Message}");
            }
        });
    }

    private void btn_leave_Click(object sender, EventArgs e)
    {
        OnExit?.Invoke();
    }

    private void RepaintUserListHandler()
    {
        // Добавляем в UI через Invoke
        if (tb_dialog.InvokeRequired)
        {
            tb_dialog.Invoke(RepaintUserList);
        }
        else
        {
            RepaintUserList();
        }
    }
    
    private void RepaintUserList()
    {
        lb_users.Text = "Пользователи:\r\n";
        foreach (var user in _userTable.Values)
        {
            lb_users.Text += $"{user}\r\n";
        }
    }
}