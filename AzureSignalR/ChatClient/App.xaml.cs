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
            // Handle when your app starts
        }

        protected async override void OnSleep()
        {
            if (shouldDisconnect == true)
            {
                await signalR.LogOut();
            }
            // Handle when your app sleeps
        }

        protected async override void OnResume()
        {

            // เช็คได้
            var ResultState = signalR.GetconnectionState();
            //  conid อันเก่า
            var ResultId = signalR.GetconnectionId();
            var current = Connectivity.NetworkAccess;
            // connection = null 

            if (shouldConnect == true)
            {
                if (SignalRService.connection?.State == HubConnectionState.Disconnected)
                {
                    if (current == NetworkAccess.Internet)
                    {
                        Debug.WriteLine("NetworkAccess.Internet");
                        // Connection to internet is available
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
                    // Handle when your app resumes
                }
            }

        }
    }
}
