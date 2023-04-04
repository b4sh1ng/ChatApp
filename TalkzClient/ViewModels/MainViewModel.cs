using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ControlzEx.Standard;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TalkzClient.Models;

namespace TalkzClient.ViewModels
{
    internal partial class MainViewModel : ObservableObject
    {
        public MainViewModel()
        {
        }

        [ObservableProperty]
        private string? answer;
        [ObservableProperty]
        private string? clientid;
        [ObservableProperty]
        private string? xclientid;
        [ObservableProperty]
        private string? data;
        public static ObservableCollection<MessageModel> Messages { get; set; } = new ObservableCollection<MessageModel>();

        [RelayCommand]
        private async Task Anfrage()
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5292");
            var client = new Chat.ChatClient(channel);

            var x = client.Subscribe(new Request { Id = Convert.ToInt32(Clientid) });
            try
            {
                await foreach (var Nachricht in x.ResponseStream.ReadAllAsync())
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Messages.Add(new MessageModel
                        {
                            Username = "Me",
                            //FromId = messagee.FromId,
                            Message = Nachricht.Data,
                            Time = DateTime.Now,
                        });
                    });
                }
            }
            catch (RpcException ex)
            {
                // Handle any gRPC exceptions that may occur
                MessageBox.Show($"gRPC exception: {ex.Status}");
            }
            catch (Exception ex)
            {
                // Handle any other exceptions that may occur
                MessageBox.Show($"Exception: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task Senden()
        {
            var channel = GrpcChannel.ForAddress("http://localhost:5292");
            var client = new Chat.ChatClient(channel);
            await client.SendMessageAsync(new Msg { FromId = Convert.ToInt32(Clientid), ToId = Convert.ToInt32(Xclientid), Text = this.Data });
        }



    }
}
