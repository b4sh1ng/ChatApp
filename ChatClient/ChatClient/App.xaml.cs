﻿using Grpc.Net.Client;
using GrpcLogin;
using System;
using System.Configuration;
using System.Windows;

namespace ChatClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly GrpcChannel Channel = GrpcChannel.ForAddress(ConfigurationManager.AppSettings.Get("connectionString"));
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

