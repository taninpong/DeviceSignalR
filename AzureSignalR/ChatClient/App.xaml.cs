using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using System.Threading;

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
        private static CancellationTokenSource cts = new CancellationTokenSource();
        public static int SleepTime = 0;

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
            cts = new CancellationTokenSource();
            TimeTicker(cts.Token);
            await DisconnectSignalRInTime(cts.Token);
        }

        private static async Task TimeTicker(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested) break;
                await Task.Delay(1000);
                SleepTime++;
                signalR.AddNewMessage("", SleepTime.ToString());
            }
        }

        private static async Task DisconnectSignalRInTime(CancellationToken cancellationToken)
        {
            await Task.Delay(15000);
            if (shouldDisconnect && !cancellationToken.IsCancellationRequested)
            {
                await signalR.Disconnect();
            }
        }

        protected async override void OnResume()
        {
            cts.Cancel();
            SleepTime = 0;
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
