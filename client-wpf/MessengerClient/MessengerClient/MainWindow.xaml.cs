using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net.Sockets;




namespace MessengerClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string message;
            message = ($"LOGIN|{LoginBox.Text}|{PasswordBox.Password}");
            try
            {
                TcpClient client = new TcpClient("127.0.0.1", 10000);
                NetworkStream stream = client.GetStream();
                byte[] data = Encoding.UTF8.GetBytes(message);  
                stream.Write(data, 0, data.Length);
                byte[] buffer = new byte[1024];
                int byteRead = stream.Read(buffer, 0, buffer.Length);
                string responce = Encoding.UTF8.GetString(buffer, 0, byteRead);
                string[] parts = responce.Split('|');
                if (parts[0] == "SUCCESS") 
                {
                    MessageBox.Show("Добро пожаловать в систему!");
                    ErrorText.Text = "";
                }
                else
                {
                    ErrorText.Text = "Неверный логин или пароль!";
                    PasswordBox.Clear();
                }
                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения: " + ex.Message);
            }
        }

        private void RegistrationButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
        }

        private void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            if (RegPasswordBox.Password == RegPasswordCheckBox.Password)
            {
                string message;
                message = ($"REGISTER|{RegLoginBox.Text}|{RegPasswordBox.Password}|{RegNameBox.Text}");
                try
                {
                    TcpClient client = new TcpClient("127.0.0.1", 10000);
                    NetworkStream stream = client.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                    byte[] buffer = new byte[1024];
                    int byteRead = stream.Read(buffer, 0, buffer.Length);
                    string responce = Encoding.UTF8.GetString(buffer, 0, byteRead);
                    string[] parts = responce.Split('|');
                    if (parts[0] == "SUCCESS")
                    {
                        MessageBox.Show("Добро пожаловать в систему!");
                        ErrorText.Text = "";
                    }
                    else
                    {
                        ErrorText.Text = "Данный логин занят!";
                        RegPasswordBox.Clear();
                        RegPasswordCheckBox.Clear();
                    }
                    client.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка подключения: " + ex.Message);
                }
            }
            else
            {
                RegErrorText.Text = "Пароли не совпадают!";
            }
        }

        private void RegLoginButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Visible;
            RegisterPanel.Visibility = Visibility.Collapsed;
        }
    }   
}