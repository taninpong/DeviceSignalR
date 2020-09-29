using System;
using Xamarin.Forms;

namespace ChatClient
{
    public partial class MainPage : ContentPage
    {
        SignalRService signalR;
        public MainPage()
        {
            InitializeComponent();
            signalR = App.signalR;
            signalR.Connected += SignalR_ConnectionChanged;
            signalR.ConnectionFailed += SignalR_ConnectionChanged;
            signalR.NewMessageReceived += SignalR_NewMessageReceived;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            //await signalR.ConnectAsync();
            //connectButton.IsEnabled = false;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        async void AddMessage(string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Label label = new Label
                {
                    Text = message,
                    HorizontalOptions = LayoutOptions.Start,
                    VerticalOptions = LayoutOptions.Start
                };
                messageList.Children.Add(label);
            });
        }

        async void SignalR_NewMessageReceived(object sender, Model.Message message)
        {
            string msg = "";
            if (message.Text == "Disconnect Plese Check Network" || message.Text == "Connection Close")
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    //connectButton.Text = "Connect";
                    //connectButton.IsEnabled = true;
                    //sendButton.IsEnabled = false;
                    //loginButton.IsEnabled = false;
                    //sendToUserButton.IsEnabled = false;
                });
                msg = $"{message.Text}";
                AddMessage(msg);
            }
            else if (message.Text == "Reconnected")
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    //connectButton.Text = "Connect";
                    //connectButton.IsEnabled = false;
                    //sendButton.IsEnabled = true;
                    //loginButton.IsEnabled = true;
                    //sendToUserButton.IsEnabled = true;
                });
                msg = $"{message.Text}";
                AddMessage(msg);
            }
            else
            {
                msg = $"{message.Text}";
                AddMessage(msg);
            }
        }

        void SignalR_ConnectionChanged(object sender, bool success, string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                //connectButton.Text = "Connect";
                //connectButton.IsEnabled = !success;
                //sendButton.IsEnabled = success;
                //loginButton.IsEnabled = success;
                //sendToUserButton.IsEnabled = success;
                AddMessage($"Server connection changed: {message}");
            });
        }

        async void ConnectButton_ClickedAsync(object sender, EventArgs e)
        {
            //connectButton.Text = "Connecting...";
            //connectButton.IsEnabled = false;
            await signalR.ConnectAsync();
        }

        async void SendButton_ClickedAsync(object sender, EventArgs e)
        {
            //await signalR.SendMessageAsync(userSend.Text, messageEntry.Text);
            //messageEntry.Text = "";
            //AddMessage($"Server connection changed: {userSend.Text} Say{messageEntry.Text}");
        }

        async void LoginButton_ClickedAsync(object sender, EventArgs e)
        {
            App.name = userSend.Text.ToString();
            await signalR.ConnectToUserAsync(userSend.Text);

            AddMessage($"Server connection changed: LoginSuccess");
            //messageEntry.Text = "";
            //await signalR.SendMessageAsync(userLogin.Text, messageEntry.Text);
        }


        async void SendToUserButton_ClickedAsync(object sender, EventArgs e)
        {
            //await signalR.SendMessageToUserAsync(userReceive.Text, messageEntry.Text);
            //messageEntry.Text = "";
            //AddMessage($"Server connection changed: {userSend.Text} Say{messageEntry.Text}");
        }

        async void Logout_ClickedAsync(object sender, EventArgs e)
        {
            await signalR.LogOut();
            //messageEntry.Text = "";
            //AddMessage($"Server connection changed: {userSend.Text} Say{messageEntry.Text}");
        }

        void OnCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)

        {
            checkBox1.IsChecked = e.Value;
            App.shouldConnect = e.Value;
            // Perform required operation after examining e.Value
        }

        void OnCheckBoxCheckedChanged2(object sender, CheckedChangedEventArgs e)
        {
            checkBox2.IsChecked = e.Value;
            App.shouldDisconnect = e.Value;
            // Perform required operation after examining e.Value
        }
    }
}
