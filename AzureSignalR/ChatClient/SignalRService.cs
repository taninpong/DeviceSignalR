using ChatClient.Model;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ChatClient
{
    public class SignalRService
    {
        HttpClient client;
        public delegate void MessageReceivedHandler(object sender, Message message);
        public delegate void ConnectionHandler(object sender, bool successful, string message);
        public event MessageReceivedHandler NewMessageReceived;
        public event ConnectionHandler Connected;
        public event ConnectionHandler ConnectionFailed;
        public bool IsConnected { get; private set; }
        public bool IsBusy { get; private set; }
        public static HubConnection connection;

        public SignalRService()
        {
            client = new HttpClient();
        }

        public async Task LogOut()
        {
            var rrr = GetconnectionId();
            await connection?.StopAsync();
            AddNewMessage("", "DisConnect");
        }

        public async Task ConnectToUserAsync(string userId)
        {
            try
            {
                if (IsBusy) return;

                if (connection != null && connection.State != HubConnectionState.Disconnected)
                {
                    await connection.StopAsync();
                }

                IsBusy = true;
                //"https://signalabhub.azurewebsites.net/ManagementSampleHub?user=gi"
                connection = new HubConnectionBuilder()
                    .WithUrl(Constants.HostName + "/ManagementSampleHub" + $"?user={userId}")
                    .AddNewtonsoftJsonProtocol()
                    .WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60), TimeSpan.FromMinutes(3) })
                    .Build();

                await connection.StartAsync();

                connection.Closed += async (err) =>
                {
                    AddNewMessage("", "Connection Close");
                };


                if (connection.State == HubConnectionState.Disconnected)
                {
                    IsConnected = false;
                    Connected?.Invoke(this, false, "Connection Fail.");
                }
                else
                {
                    IsConnected = true;
                    Connected?.Invoke(this, true, "Connection successful.");
                }
                IsBusy = false;
                connection.On<string>("Target", (message) =>
                {
                    AddNewMessage("", message);
                    Debug.WriteLine(message);
                });

                connection.Reconnected += async connectionId =>
                {
                    AddNewMessage("", "Reconnected");
                    Debug.WriteLine(connection.State.ToString());
                };
            }
            catch (Exception ex)
            {
                ConnectionFailed?.Invoke(this, false, ex.Message);
                IsConnected = false;
                IsBusy = false;
            }
        }

        void AddNewMessage(string user, string message)
        {
            Message messageModel = new Message
            {
                Name = user,
                Text = message
            };
            NewMessageReceived?.Invoke(this, messageModel);
        }

        public string GetconnectionId()
        {
            var conid = connection?.ConnectionId;
            return conid;
        }

        public string GetconnectionState()
        {
            var conid = connection?.State.ToString();
            return conid;
        }

        private CloudTable GetConnectionTable()
        {
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=signalab;AccountKey=xbad3br/3o0AglWZ4iM1WdepVOlm9CSoMRmDbUlvFYmmUmJTlHF2hxqvsnC99fELsLvhQE1YzAi1x3mLOh9Yhg==;EndpointSuffix=core.windows.net");
            var tableClient = storageAccount.CreateCloudTableClient();
            return tableClient.GetTableReference("demotable2");
        }

    }
}
