using MQTTnet.Server;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MQTTnet.Diagnostics;
using System.Reflection.Emit;

namespace MQTTServer
{
    public partial class LogCenter : Form
    {
        MqttServer server;
        public static ListBox LOGLIST;

        new Dictionary<string, string> users = new Dictionary<string,string>(); 

        public LogCenter()
        {
            InitializeComponent();
            LOGLIST = lb_LOG;
            //users.Add("asha", "1234"); // username, password
            //users.Add("sepehr", "5678");
        }

        private async void btn_Open_Click(object sender, EventArgs e)
        {
            try
            {
                btn_Close.Enabled = true;
                btn_Open.Enabled = false;
                LOGLIST.Items.Insert(0, "Broker Starting...");
                await OpenServer();
            }
            catch (Exception ex)
            {
                btn_Close.Enabled = false;
                btn_Open.Enabled = true;
                LOGLIST.Items.Add(ex.Message);

            }
        }

        private async Task OpenServer()
        {
            // Create the options for MQTT Broker
            var options = new MqttServerOptionsBuilder()
                //Set endpoint to localhost
                .WithDefaultEndpoint();
              //.WithDefaultEndpointPort(5004);
            // Create a new mqtt server
            server = new MqttFactory().CreateMqttServer(options.Build());
            //Add Interceptor for logging incoming messages
            server.InterceptingPublishAsync += Server_InterceptingPublishAsync;
            // Add connection Validator
            //server.ValidatingConnectionAsync += Server_ValidatingConnectionAsync;
            // Start the server
            await server.StartAsync();
            if (server.IsStarted)
                LOGLIST.Items.Insert(0, "Broker Started");
            else 
                LOGLIST.Items.Insert(0, "Broker Start Failed");

            // Keep application running until user press a key
          

            Task Server_InterceptingPublishAsync(InterceptingPublishEventArgs arg)
            {
                // Convert Payload to string
                var payload = arg.ApplicationMessage?.Payload == null ? null : Encoding.UTF8.GetString(arg.ApplicationMessage?.Payload);
                LOGLIST.Invoke(new MethodInvoker(delegate ()
                {
                    LOGLIST.Items.Insert(0,
                   string.Format("TimeStamp: {0} -- Message: ClientId = {1}, Topic = {2}, Payload = {3}, QoS = {4}, Retain-Flag = {5}",
                    DateTime.Now,
                    arg.ClientId,
                    arg.ApplicationMessage?.Topic,
                    payload,
                    arg.ApplicationMessage?.QualityOfServiceLevel,
                    arg.ApplicationMessage?.Retain));
                }));

                return Task.CompletedTask;
            }

            //Task Server_ValidatingConnectionAsync(ValidatingConnectionEventArgs arg)
            //{
            //    if (!string.IsNullOrWhiteSpace(arg.Username) && !string.IsNullOrWhiteSpace(arg.Password))
            //    {
            //        if (!(users.TryGetValue(arg.Username, out var password) && password == arg.Password))
            //        {
            //            arg.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadUserNameOrPassword;
            //            return Task.CompletedTask;
            //        }
            //    }
            //    arg.ReasonCode = MQTTnet.Protocol.MqttConnectReasonCode.BadAuthenticationMethod;
            //    return Task.CompletedTask;
            //}
        }

        private async void btn_Close_Click(object sender, EventArgs e)
        {
            try
            {
                btn_Close.Enabled = false;
                btn_Open.Enabled = true;
                LOGLIST.Items.Insert(0, "Broker Close.");
                await server.StopAsync();
            }
            catch (Exception ex)
            {
                btn_Close.Enabled = true;
                btn_Open.Enabled = false;
                LOGLIST.Items.Insert(0, ex.Message);

            }
        }

        private void btn_LOGClear_Click(object sender, EventArgs e)
        {
            LOGLIST.Items.Clear();
        }
    }

}
