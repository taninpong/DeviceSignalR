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

        void SignalR_ConnectionChanged(object sender, bool success, string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                AddMessage($"Server connection changed: {message}");
            });
        }

        async void LoginButton_ClickedAsync(object sender, EventArgs e)
        {
            App.name = userSend.Text.ToString();
            await signalR.ConnectToUserAsync(userSend.Text);
            AddMessage($"Server connection changed: LoginSuccess");
        }

        async void Logout_ClickedAsync(object sender, EventArgs e)
        {
            await signalR.LogOut();
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
