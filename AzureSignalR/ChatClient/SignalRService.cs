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
        public delegate void MessageReceivedHandler(object sender, Message message);
        public delegate void ConnectionHandler(object sender, bool successful, string message);
        public event MessageReceivedHandler NewMessageReceived;
        public event ConnectionHandler Connected;
        public event ConnectionHandler ConnectionFailed;
        public bool IsConnected { get; private set; }
        public bool IsBusy { get; private set; }
        public static HubConnection connection;


        public async Task Disconnect()
        {
            if (connection != null && connection.State != HubConnectionState.Disconnected)
            {
                await connection.StopAsync();
                AddNewMessage("", $"DisConnect {DateTime.UtcNow.ToString("HH: mm:ss.fff tt") }");
            }
        }

        public async Task ConnectToUserAsync(string userId)
        {
            var date = DateTime.UtcNow.ToString("HH: mm:ss.fff tt");
            try
            {
                if (IsBusy) return;

                if (connection != null && connection.State != HubConnectionState.Disconnected)
                {
                    await connection.StopAsync();
                }

                IsBusy = true;
                connection = new HubConnectionBuilder()
                    .WithUrl(Constants.HostName + "/ManagementSampleHub" + $"?user={userId}")
                    .AddNewtonsoftJsonProtocol()
                    .WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60), TimeSpan.FromMinutes(3) })
                    .Build();

                await connection.StartAsync();

                connection.Closed += async (err) =>
                {
                    AddNewMessage("", $"Connection Close {date}");
                };


                if (connection.State == HubConnectionState.Disconnected)
                {
                    IsConnected = false;
                    Connected?.Invoke(this, false, "Connection Fail.");
                }
                else
                {
                    IsConnected = true;
                    Connected?.Invoke(this, true, $"Connection successful. {date}");
                }

                IsBusy = false;

                connection.On<string>("Target", (message) =>
                {
                    var newmess = message + "|" + DateTime.UtcNow.ToString("HH: mm:ss.fff tt");
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

        public string GetconnectionId() => connection?.ConnectionId;

        public string GetconnectionState() => connection?.State.ToString();
    }
}
