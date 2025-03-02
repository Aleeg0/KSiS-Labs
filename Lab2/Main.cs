namespace Lab2;

public partial class Main : Form
{

    private ChatLayout? _chatLayout;
    public Main()
    {
        _chatLayout = null;
        InitializeComponent();
        AuthorizationLayout authLayout = new AuthorizationLayout();
        authLayout.Dock = DockStyle.Fill;
        authLayout.AuthorizationSuccess += ShowChat;
        Controls.Add(authLayout);
        
    }

    private void ShowChat(UserData user)
    {
        _chatLayout = new ChatLayout(user);
        _chatLayout.OnExit += Disable;
        _chatLayout.Dock = DockStyle.Fill;
        Controls.Clear();
        Controls.Add(_chatLayout);
    }

    private void Main_FormClosed(object sender, FormClosedEventArgs e)
    {
        _chatLayout?.DisableChat();
    }

    private void Disable()
    {
        Close();
    }
}