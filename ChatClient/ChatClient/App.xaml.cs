using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GrpcLogin;
using Grpc;
using Grpc.Net.Client;
using System.Windows.Media;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly GrpcChannel Channel = GrpcChannel.ForAddress("http://localhost:5292");
        public Sign.SignClient SignClient { get; } = new Sign.SignClient(Channel);
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            TryAutoLogin();
        }
        private void SetSettings()
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["userId"].Value = "";
            config.AppSettings.Settings["sessionId"].Value = "";
            config.Save(ConfigurationSaveMode.Full, true);
        }
        private void TryAutoLogin()
        {
            int UserId = 0;
            string SessionId;

            if (int.TryParse(ConfigurationManager.AppSettings.Get("userId"), out int id))
            {
                UserId = id;
            }
            SessionId = ConfigurationManager.AppSettings.Get("sessionId")!;

            if (string.IsNullOrEmpty(SessionId) || UserId == 0)
            {
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                return;
            }
            var loginTry = SignClient.LoginWithSession(new SessionLogin
            {
                SessionId = SessionId,
                UserId = UserId,
            }, deadline: DateTime.UtcNow.AddSeconds(5));
            if (loginTry.IsOk)
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                return;
            }
            var loginWindow2 = new LoginWindow();
            loginWindow2.Show();
            SetSettings();
        }
    }


}

