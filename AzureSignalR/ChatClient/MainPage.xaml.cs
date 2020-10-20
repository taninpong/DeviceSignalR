using System;
using Xamarin.Essentials;
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
                msg = $"{message.Text}";
                AddMessage(msg);
            }
            else if (message.Text == "Reconnected")
            {
                Device.BeginInvokeOnMainThread(() =>
                {
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
            AddMessage($"Server connection changed: {message}");
        }

        async void LoginButton_ClickedAsync(object sender, EventArgs e)
        {
            App.id = DeviceInfo.Name;
            //App.id = Preferences.Get("my_id", string.Empty);
            //if (string.IsNullOrWhiteSpace(App.id))
            //{
            //    App.id = System.Guid.NewGuid().ToString();
            //    Preferences.Set("my_id", App.id);
            //}
            //App.name = userSend.Text.ToString();
            await signalR.ConnectToUserAsync(App.id);
            AddMessage($"Server connection changed: LoginSuccess");
        }

        async void Logout_ClickedAsync(object sender, EventArgs e)
        {
            await signalR.Disconnect();
        }

        void OnCheckBoxCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            checkBox1.IsChecked = e.Value;
            App.shouldConnect = e.Value;
        }

        void OnCheckBoxCheckedChanged2(object sender, CheckedChangedEventArgs e)
        {
            checkBox2.IsChecked = e.Value;
            App.shouldDisconnect = e.Value;
        }

    }
}
