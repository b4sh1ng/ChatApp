using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Grpc.Net.Client;
using GrpcServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TalkzClient.ViewModels
{
    internal partial class MainViewModel : ObservableObject
    {


        [ObservableProperty]
        private string? answer;

        [RelayCommand]
        private async Task Anfrage()
        {
            var input = new HelloRequest { Name = "Michael" };
            var channel = GrpcChannel.ForAddress("http://localhost:5292");
            var client = new Greeter.GreeterClient(channel);
            var reply = await client.SayHelloAsync(input);
            Answer = reply.Message;
        }


    }
}
