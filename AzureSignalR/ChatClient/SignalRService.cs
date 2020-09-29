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
        private string username;
        public static HubConnection connection;

        public SignalRService()
        {
            client = new HttpClient();
        }

        public async Task SendMessageAsync(string username, string message)
        {
            IsBusy = true;
            await connection.InvokeAsync("SendMessages", "broadcast ", message);
            IsBusy = false;
        }

        public async Task SendMessageToUserAsync(string userreceive, string message)
        {
            IsBusy = true;
            await connection.InvokeAsync("send user", userreceive, message);
            IsBusy = false;
        }

        public async Task Login(string username)
        {
            this.username = username;
            var conn = await connection.InvokeAsync<string>("Login", username);
            //await connection.InvokeAsync("Login", username);
        }

        public async Task LogOut()
        {
            var rrr = GetconnectionId();
            await connection?.StopAsync();
            AddNewMessage("", "DisConnect");
            //this.username = username;
            //var conn = await connection.InvokeAsync<string>("Login", username);
            //await connection.InvokeAsync("Login", username);
        }

        public async Task ConnectAsync()
        {
            try
            {
                string userId = "nano";
                IsBusy = true;
                //"https://localhost:5001/ManagementSampleHub?user=gi"
                connection = new HubConnectionBuilder()
                    .WithUrl(Constants.HostName + "/ManagementSampleHub" + $"?user={userId}")
                    .AddNewtonsoftJsonProtocol()
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60), TimeSpan.FromMinutes(3) })
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

                connection.On<string, string>("ReceiveMessage", (user, message) =>
                {
                    var users = $"{user} SayAll :";
                    AddNewMessage(users, message);
                    Debug.WriteLine(message);
                });


                connection.On<string>("Target", (message) =>
                {
                    Debug.WriteLine(message);
                });

                connection.On<string, string>("ReceiveMessageToUser", (usersend, message) =>
                {
                    var users = $"{usersend}  :";
                    AddNewMessage(users, message);
                    Debug.WriteLine(message);
                });

                //connection.Reconnecting += async error =>
                //{
                //    Debug.Assert(connection.State == HubConnectionState.Reconnecting);
                //    AddNewMessage("", "Disconnect Plese Check Network");
                //    //await Task.Delay(random.Next(0, 10) * 1000);
                //};
                //TODO Reconnect

                connection.Reconnected += async connectionId =>
                {
                    AddNewMessage("", "Reconnected");
                    Debug.WriteLine(connection.State.ToString());
                    if (connection.State == HubConnectionState.Connected)
                    {
                        await Login(username);
                    }
                };
            }
            catch (Exception ex)
            {
                ConnectionFailed?.Invoke(this, false, ex.Message);
                IsConnected = false;
                IsBusy = false;
            }
        }


        public async Task ConnectToUserAsync(string userLogin)
        {
            try
            {
                string userId = userLogin;
                IsBusy = true;
                //"https://localhost:5001/ManagementSampleHub?user=gi"
                connection = new HubConnectionBuilder()
                    .WithUrl(Constants.HostName + "/ManagementSampleHub" + $"?user={userId}")
                    .AddNewtonsoftJsonProtocol()
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(60), TimeSpan.FromMinutes(3) })
                    .Build();

                await connection.StartAsync();

                var table = GetConnectionTable();
                EmployeeEntity employeeEntity = new EmployeeEntity(userId, connection.ConnectionId);
                TableOperation insertOperation = TableOperation.InsertOrReplace(employeeEntity);
                table.Execute(insertOperation);

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

                //connection.On<string, string>("ReceiveMessage", (user, message) =>
                //{
                //    var users = $"{user} SayAll :";
                //    AddNewMessage(users, message);
                //    Debug.WriteLine(message);
                //});


                connection.On<string>("Target", (message) =>
                {
                    AddNewMessage("", message);
                    Debug.WriteLine(message);
                });

                //connection.On<string, string>("ReceiveMessageToUser", (usersend, message) =>
                //{
                //    var users = $"{usersend}  :";
                //    AddNewMessage(users, message);
                //    Debug.WriteLine(message);
                //});

                //connection.Reconnecting += async error =>
                //{
                //    Debug.Assert(connection.State == HubConnectionState.Reconnecting);
                //    AddNewMessage("", "Disconnect Plese Check Network");
                //    //await Task.Delay(random.Next(0, 10) * 1000);
                //};
                //TODO Reconnect

                connection.Reconnected += async connectionId =>
                {
                    AddNewMessage("", "Reconnected");
                    Debug.WriteLine(connection.State.ToString());
                    if (connection.State == HubConnectionState.Connected)
                    {
                        await Login(username);
                    }
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
