using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrpcLogin;
using System.Windows;
using Grpc.Net.Client;
using GrpcServer;
using System.Configuration;
using System.Windows.Media;

namespace ChatClient.ViewModels;

public partial class LoginView : BaseView
{
    public Action CloseAction { get; set; }
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
        var passwordbox = (o as System.Windows.Controls.PasswordBox);
        var password = passwordbox?.Password;
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
            return;
        }

        if (loginTry.IsOk && !string.IsNullOrEmpty(loginTry.Session))
        {
            SetSettings(loginTry.UserId, loginTry.Session);
            SwitchView();
        }
        else
        {
            LoginMessage = $"Login mit der E-Mail: {LoginEmail} nicht erfolgreich!";
            LoginMessageColor = new(Colors.IndianRed);
        }
    }
    public LoginView()
    {
        App.Current.MainWindow.Loaded += StartupSequence;
        

        
    }

    private void StartupSequence(object? sender, EventArgs e)
    {
        TryAutoLogin();
    }
    public void TryAutoLogin()
    {
        if (int.TryParse(ConfigurationManager.AppSettings.Get("userId"), out int id))
        {
            UserId = id;
        }
        SessionId = ConfigurationManager.AppSettings.Get("sessionId");

        if (string.IsNullOrEmpty(SessionId) || UserId == 0) return;

        var loginTry = SignClient.LoginWithSession(new SessionLogin
        {
            SessionId = this.SessionId,
            UserId = this.UserId,
        });

        if (loginTry.IsOk)
        {
            SwitchView();
            return;
        }
        LoginMessage = "Session invalid! Please Sign in.";
        LoginMessageColor = new(Colors.IndianRed);
        SetSettings();
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
        Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        config.AppSettings.Settings["userId"].Value = Convert.ToString(userId);
        config.AppSettings.Settings["sessionId"].Value = sessionId;
        config.Save(ConfigurationSaveMode.Full, true);
    }
}
