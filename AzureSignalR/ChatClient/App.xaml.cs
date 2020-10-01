using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace ChatClient
{
    public partial class App : Application
    {
        public static SignalRService signalR;
        public static string name = "";
        public static bool shouldConnect = false;
        public static bool shouldDisconnect = false;


        public App()
        {
            InitializeComponent();
            signalR = new SignalRService();
            MainPage = new MainPage();
        }

        protected override void OnStart()
        {
        }

        protected async override void OnSleep()
        {
            if (shouldDisconnect == true)
            {
                await signalR.LogOut();
            }
        }

        protected async override void OnResume()
        {
            // เช็คได้
            var ResultState = signalR.GetconnectionState();
            //  conid อันเก่า
            var ResultId = signalR.GetconnectionId();
            var current = Connectivity.NetworkAccess;

            if (shouldConnect == true)
            {
                if (SignalRService.connection?.State == HubConnectionState.Disconnected)
                {
                    if (current == NetworkAccess.Internet)
                    {
                        Debug.WriteLine("NetworkAccess.Internet");
                        await signalR.ConnectToUserAsync(name);
                    }
                    else if (current == NetworkAccess.None)
                    {
                        Debug.WriteLine("NetworkAccess.None");
                    }
                    else
                    {
                        Debug.WriteLine("NetworkAccess.err");
                    }
                }
            }

        }
    }
}
