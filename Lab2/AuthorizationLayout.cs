using System.Net;

namespace Lab2;

public partial class AuthorizationLayout : UserControl
{
    public event Action<UserData>? AuthorizationSuccess;
    
    public AuthorizationLayout()
    {
        InitializeComponent();
    }

    private void btn_accept_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(tb_username.Text))
        {
            MessageBox.Show("Введите данные!");
            return;
        }
        IPAddress ipAddress = IPAddress.Parse(NetworkTools.GetActiveIPAddress());
        UserData user = new UserData(tb_username.Text, ipAddress);
        Console.WriteLine(user.ToString());
        AuthorizationSuccess?.Invoke(user);
    }
}