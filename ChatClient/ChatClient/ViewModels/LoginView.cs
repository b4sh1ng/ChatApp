using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grpc.Net.Client;
using GrpcLogin;
using System;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ChatClient.ViewModels;

public partial class LoginView : BaseView
{
    private static readonly GrpcChannel Channel = GrpcChannel.ForAddress("http://localhost:5292");
    public Sign.SignClient SignClient { get; } = new Sign.SignClient(Channel);
    [ObservableProperty]
    private string loginEmail;
    [ObservableProperty]
    private int userId;
    [ObservableProperty]
    private string sessionId;
    [ObservableProperty]
    private string loginMessage;
    [ObservableProperty]
    private SolidColorBrush loginMessageColor = new(Colors.Transparent);

    [RelayCommand]
    private void Login(object o)
    {
        var passwordbox = (o as PasswordBox);
        if (string.IsNullOrEmpty(passwordbox?.Password) || string.IsNullOrEmpty(LoginEmail))
        {
            LoginMessage = "E-Mail and Password as to be filled in!";
            return;
        }
        var password = Convert.ToBase64String(SHA512.HashData(Encoding.Unicode.GetBytes(passwordbox?.Password)));
        passwordbox?.Clear();
        SuccessMessage? loginTry;
        
        try
        {
            loginTry = SignClient.LoginWithUsername(new LoginUser
            {
                Email = LoginEmail,
                PasswordHash = password,
            });
        }
        catch
        {
            LoginMessage = "Server not Online or reachable!";
            return;
        }

        if (loginTry.IsOk && !string.IsNullOrEmpty(loginTry.Session))
        {
            SetSettings(loginTry.UserId, loginTry.Session);
            SwitchView();
            Application.Current.MainWindow.Close();
        }
        else
        {
            LoginMessage = $"Login with E-Mail \"{LoginEmail}\" not successful!";
            LoginMessageColor = new(Colors.IndianRed);
        }
    }
    public LoginView()
    {
        App.Current.MainWindow.Loaded += StartupSequence;
    }

    private void StartupSequence(object? sender, EventArgs e)
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        if (int.TryParse(ConfigurationManager.AppSettings.Get("userId"), out int id))
        {
            UserId = id;
        }
        SessionId = config.AppSettings.Settings["sessionId"].Value;
        if (UserId != 0 || !string.IsNullOrEmpty(SessionId))
        {
            LoginMessage = "Session invalid or expired! Please Sign in.";
            SetSettings();
        }
    }
    public void SwitchView()
    {
        var window = new MainWindow();
        window.Show();
    }
    private void SetSettings()
    {
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        config.AppSettings.Settings["userId"].Value = "";
        config.AppSettings.Settings["sessionId"].Value = "";
        config.Save(ConfigurationSaveMode.Full, true);
    }
    private void SetSettings(int userId, string sessionId)
    {
        ConfigurationManager.AppSettings.Set("userId", Convert.ToString(userId));
        ConfigurationManager.AppSettings.Set("sessionId", sessionId);
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        config.AppSettings.Settings["userId"].Value = Convert.ToString(userId);
        config.AppSettings.Settings["sessionId"].Value = sessionId;
        config.Save(ConfigurationSaveMode.Full, true);
    }
}
