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
        public static string id = "";

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
            if (shouldDisconnect)
            {
                await signalR.Disconnect();
            }
        }

        protected async override void OnResume()
        {
            var networkAccess = Connectivity.NetworkAccess;

            if (shouldConnect)
            {
                if (SignalRService.connection?.State == HubConnectionState.Disconnected)
                {
                    if (networkAccess == NetworkAccess.Internet)
                    {
                        Debug.WriteLine("NetworkAccess.Internet");
                        await signalR.ConnectToUserAsync(id);
                    }
                    else if (Connectivity.NetworkAccess == NetworkAccess.None)
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
