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
using System.Windows.Xps.Serialization;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Globalization;




namespace MessengerClient
{
    public class MessageAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMine && isMine == true)
                return HorizontalAlignment.Right;
            return HorizontalAlignment.Left;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isMine && isMine == true)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D9FDD3"));
            return Brushes.White;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public int CurrentUserId;
        public MainWindow()
        {
            InitializeComponent();
            List<ChatModel> myChats = new List<ChatModel>();
            myChats.Add(new ChatModel { Title = "Мама", LastMessage = "Купи хлеб!", Date = "19:05" });
            Chats.ItemsSource = myChats;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(LoginBox.Text) || string.IsNullOrWhiteSpace(PasswordBox.Password) )
            {
                ErrorText.Text = "Введите данные во все поля!";
            }
            else
            {
                string message;
                message = ($"LOGIN|{LoginBox.Text.Trim()}|{PasswordBox.Password.Trim()}");
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
                        AuthScreen.Visibility = Visibility.Collapsed;
                        ChatScreen.Visibility = Visibility.Visible;
                        ErrorText.Text = "";
                        byte[] chat = Encoding.UTF8.GetBytes("GET_USERS|" + parts[1]);
                        stream.Write(chat, 0, chat.Length);
                        byte[] chatsBuffer = new byte[8192];
                        int byteChats = stream.Read(chatsBuffer, 0, chatsBuffer.Length);
                        string chatt = Encoding.UTF8.GetString(chatsBuffer, 0, byteChats);
                        string[] chatParts = chatt.Split(new char[] { '|' }, 2);
                        CurrentUserId = int.Parse(parts[1]);
                        if (chatParts[0] == "GET_USERS_SUCCESS")
                        {
                            string json = chatParts[1];
                            List<ChatModel> myChats = JsonSerializer.Deserialize<List<ChatModel>>(json);
                            Chats.ItemsSource = myChats;
                        }
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
        }

        private void RegistrationButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            LoginPanel.Visibility = Visibility.Collapsed;
            RegisterPanel.Visibility = Visibility.Visible;
        }

        private void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            if ( string.IsNullOrWhiteSpace(RegLoginBox.Text)==true || string.IsNullOrWhiteSpace(RegNameBox.Text) || string.IsNullOrWhiteSpace(RegPasswordBox.Password) || string.IsNullOrWhiteSpace(RegPasswordCheckBox.Password) )
            {
                RegErrorText.Text = "Введите данные во все поля!";
            }
            
            else if (RegPasswordBox.Password == RegPasswordCheckBox.Password)
            {
                string message;
                message = ($"REGISTER|{RegLoginBox.Text.Trim()}|{RegPasswordBox.Password.Trim()}|{RegNameBox.Text.Trim()}");
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
                        AuthScreen.Visibility = Visibility.Collapsed;
                        ChatScreen.Visibility = Visibility.Visible;
                        ErrorText.Text = "";
                        byte[] chat = Encoding.UTF8.GetBytes("GET_USERS|" + parts[1]);
                        stream.Write(chat, 0, chat.Length);
                        byte[] chatsBuffer = new byte[8192];
                        int byteChats = stream.Read(chatsBuffer, 0, chatsBuffer.Length); 
                        string chatt = Encoding.UTF8.GetString(chatsBuffer, 0, byteChats);
                        string[] chatParts = chatt.Split(new char[] { '|' }, 2);
                        CurrentUserId = int.Parse(parts[1]);
                        if (chatParts[0] == "GET_USERS_SUCCESS")
                        {
                            string json = chatParts[1];
                            List<ChatModel> myChats = JsonSerializer.Deserialize<List<ChatModel>>(json);
                            Chats.ItemsSource = myChats;
                        }
                    }
                    else
                    {
                        RegErrorText.Text = "Данный логин занят!";
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


        public class ChatModel
        {
            [JsonPropertyName("chat_id")]
            public int Id { get; set; }
            [JsonPropertyName("name")] 
            public string Title { get; set; }
            [JsonPropertyName("last_message")]
            public string LastMessage { get; set; }
            [JsonPropertyName("sent_at")] 
            public string Date { get; set; }
        }

        public class MessageModel
        {
            [JsonPropertyName("sender_id")]
            public int SenderId { get; set; } 
            [JsonPropertyName("sender_name")]
            public string SenderName { get; set; }
            [JsonPropertyName("text")]
            public string Text { get; set; }
            [JsonPropertyName("time")]
            public string Time { get; set; }
            public bool IsMine { get; set; }
        }



        private void Chats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Chats.SelectedItem is ChatModel selectedChat)
            {
                CurrentChatTittle.Visibility = Visibility.Visible;
                MessagesList.Visibility = Visibility.Visible;
                CurrentChatTittle.Text = selectedChat.Title;
                try
                {
                    TcpClient client = new TcpClient("127.0.0.1", 10000);
                    NetworkStream stream = client.GetStream();

                    string request = "GET_MESSAGES|" + selectedChat.Id;
                    byte[] data = Encoding.UTF8.GetBytes(request);
                    stream.Write(data, 0, data.Length);

                    byte[] buffer = new byte[8192];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    string[] parts = response.Split(new char[] { '|' }, 2);
                    if (parts[0] == "GET_MESSAGES_SUCCESS")
                    {
                        string json = parts[1];
                        List<MessageModel> history = JsonSerializer.Deserialize<List<MessageModel>>(json);

                        foreach (var msg in history)
                        {
                            if (msg.SenderId == CurrentUserId)
                            {
                                msg.IsMine = true;
                            }
                        }

                        MessagesList.ItemsSource = history;
                    }

                    client.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке сообщений: " + ex.Message);
                }
            }
        }

    }   
}