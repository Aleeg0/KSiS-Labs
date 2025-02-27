namespace Lab2;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        AuthorizationLayout authLayout = new AuthorizationLayout();
        authLayout.Dock = DockStyle.Fill;
        authLayout.AuthorizationSuccess += ShowChat;
        this.Controls.Add(authLayout);
        
    }

    private void ShowChat(UserData user)
    {
        ChatLayout chatLayout = new ChatLayout(user);
        chatLayout.Dock = DockStyle.Fill;
        this.Controls.Clear();
        this.Controls.Add(chatLayout);
    }
}