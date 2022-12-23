

using Grpc.Net.Client;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static grpcFileClient.FileService;

namespace grpcFileClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:7095");
            var client = new FileServiceClient(channel);

            string downloadPath = @"D:\grpcFileTransportClient\grpcFileTransportClient\DownloadFiles";

            var fileInfo = new grpcFileClient.FileInfo
            {
                FileExtension = ".pdf",
                FileNmae = "Otel_Otomasyon_Projesi (1)"
            };

            FileStream fileStream = null;

            var request = client.FileDownload(fileInfo);
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
            int count = 0;
            decimal chunksize = 0;
            while (await request.ResponseStream.MoveNext(cancellationTokenSource.Token))
            {
                if (count++ == 0)
                {
                    fileStream = new FileStream($@"{downloadPath}\{request.ResponseStream.Current.Info.FileNmae}{request.ResponseStream.Current.Info.FileExtension}", FileMode.CreateNew);
                    fileStream.SetLength(request.ResponseStream.Current.FileSize);
                }

                var buffer = request.ResponseStream.Current.Buffer.ToByteArray();
                await fileStream.WriteAsync(buffer, 0, request.ResponseStream.Current.ReadedByte);
                Console.WriteLine($"{Math.Round(((chunksize += request.ResponseStream.Current.ReadedByte) * 100) / request.ResponseStream.Current.FileSize)} %");
            }
            Console.WriteLine("Yüklendi... ");
            await fileStream.DisposeAsync();
            fileStream.Close();

        } 
    }
}
