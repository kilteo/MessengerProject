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
using System.IO;




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
        private const string ServerIp = "127.0.0.1";
        private const int ServerPort = 10000;
        public int CurrentUserId;
        public MainWindow()
        {
            InitializeComponent();
            List<ChatModel> myChats = new List<ChatModel>();

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
                    TcpClient client = new TcpClient(ServerIp, ServerPort);
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
                        CurrentUserId = int.Parse(parts[1]);
                        LoadMyChats();
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
                    TcpClient client = new TcpClient(ServerIp,ServerPort);
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
                        CurrentUserId = int.Parse(parts[1]);
                        LoadMyChats();
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
            public DateTime Time { get; set; }
            public bool IsMine { get; set; }
            public string DateBannerText { get; set; }
            public Visibility DateBannerVisibility { get; set; } = Visibility.Collapsed;
        }



        private void Chats_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Chats.SelectedItem is ChatModel selectedChat)
            {
                CurrentChatTittle.Visibility = Visibility.Visible;
                MessagesList.Visibility = Visibility.Visible;
                MessageInputPanel.Visibility = Visibility.Visible;
                CurrentChatTittle.Text = selectedChat.Title;
                LoadChatHistory(selectedChat.Id);
            }
        }

        private void MessageInputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(MessageInputBox.Text))
            {
                PlaceholderText.Visibility = Visibility.Visible;
            }
            else
            {
                PlaceholderText.Visibility = Visibility.Collapsed;
            }
            if (string.IsNullOrWhiteSpace(MessageInputBox.Text))
            {
                SendCustomButton.Visibility = Visibility.Collapsed;
            }
            else
            {
                SendCustomButton.Visibility = Visibility.Visible;
            }
        }

        private void LoadChatHistory(int chatId)
        {
            try
            {
                TcpClient client = new TcpClient(ServerIp, ServerPort);
                NetworkStream stream = client.GetStream();

                string request = "GET_MESSAGES|" + chatId;
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
                    for (int i = 0; i < history.Count; i++)
                    {
                        var currentMsg = history[i];
                        if (currentMsg.SenderId == CurrentUserId)
                        {
                            currentMsg.IsMine = true;
                        }
                        bool isNewDay = false;
                        if (i == 0)
                        {
                            isNewDay = true;
                        }
                        else
                        {
                            if (currentMsg.Time.Date != history[i - 1].Time.Date)
                            {
                                isNewDay = true;
                            }
                        }

                        if (isNewDay)
                        {
                            currentMsg.DateBannerVisibility = Visibility.Visible;
                            DateTime msgDate = currentMsg.Time.Date;

                            if (msgDate == DateTime.Today)
                            {
                                currentMsg.DateBannerText = "Сегодня";
                            }
                            else if (msgDate == DateTime.Today.AddDays(-1))
                            {
                                currentMsg.DateBannerText = "Вчера";
                            }
                            else
                            {
                                currentMsg.DateBannerText = msgDate.ToString("d MMMM");
                            }
                        }
                        else
                        {
                            currentMsg.DateBannerVisibility = Visibility.Collapsed;
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
        private void SendMessageToServer()
        {
            if (string.IsNullOrWhiteSpace(MessageInputBox.Text))
            {
                return;
            }
            if (Chats.SelectedItem is ChatModel selectedChat)
            {
                string message = $"SEND_MESSAGE|{selectedChat.Id}|{CurrentUserId}|{MessageInputBox.Text.Trim()}";

                try
                {
                    TcpClient client = new TcpClient(ServerIp,ServerPort);
                    NetworkStream stream = client.GetStream();

                    byte[] data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);

                    byte[] buffer = new byte[8192];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string[] parts = response.Split(new char[] { '|' }, 2);
                    if (parts[0] == "SEND_MESSAGE_SUCCESS")
                    {
                        MessageInputBox.Text = ""; 
                        LoadChatHistory(selectedChat.Id); 
                    }

                    client.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при отправке сообщений: " + ex.Message);
                }
            }
        }
        private void SendCustomButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SendMessageToServer();
        }
        private void MessageInputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessageToServer();
            }
        }

        private void CreateChatButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchUserBox.Text))
            {
                return;
            }
            else
            {
                string message = $"CREATE_CHAT|{CurrentUserId}|{SearchUserBox.Text.Trim()}";
                try
                {
                    TcpClient client = new TcpClient(ServerIp, ServerPort);
                    NetworkStream stream = client.GetStream();
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    stream.Write(data, 0, data.Length);
                    byte[] buffer = new byte[8192];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    string[] parts = response.Split('|');
                    if (parts[1] == "SUCCESS" || parts[1] == "CHAT_ALREADY_EXISTS")
                    {
                        SearchUserBox.Text = "";
                        LoadMyChats(); 

                        if (parts.Length > 2 && int.TryParse(parts[2], out int targetChatId))
                        {
                            foreach (ChatModel chat in Chats.Items)
                            {
                                if (chat.Id == targetChatId)
                                {
                                    Chats.SelectedItem = chat;
                                    break;
                                }
                            }
                        }
                    }
                    else if (parts[1] == "USER_NOT_FOUND")
                    {
                        MessageBox.Show("Пользователь с таким логином не найден!");
                    }
                    else if (parts[1] == "CANNOT_CHAT_WITH_SELF")
                    {
                        MessageBox.Show("Нельзя создать чат с самим собой!");
                    }
                    client.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при создании чата: " + ex.Message);
                }
            }
        }

        private void LoadMyChats()
        {
            try
            {
                TcpClient client = new TcpClient(ServerIp, ServerPort);
                NetworkStream stream = client.GetStream();
                byte[] chat = Encoding.UTF8.GetBytes("GET_USERS|" + CurrentUserId.ToString());
                stream.Write(chat, 0, chat.Length);
                byte[] chatsBuffer = new byte[8192];
                int byteChats = stream.Read(chatsBuffer, 0, chatsBuffer.Length);
                string chatt = Encoding.UTF8.GetString(chatsBuffer, 0, byteChats);
                string[] chatParts = chatt.Split(new char[] { '|' }, 2);
                if (chatParts[0] == "GET_USERS_SUCCESS")
                {
                    string json = chatParts[1];
                    List<ChatModel> myChats = JsonSerializer.Deserialize<List<ChatModel>>(json);
                    Chats.ItemsSource = myChats;
                }
                client.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке списка чатов: " + ex.Message);
            }
        }

    }   
}
