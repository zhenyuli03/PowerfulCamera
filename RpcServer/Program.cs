using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcGreeter;
using HalconDotNet;


namespace RpcServer
{
    public class RpcTest : Greeter.GreeterBase
    {
        public override Task<Empty> PrintfMessage(_String request, ServerCallContext context)
        {
            Console.WriteLine(request.Text);
            return Task.FromResult(new Empty { });
        }
        //Dictionary<int, HObject> HobjectMap = new Dictionary<int, HObject>();
        //public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        //{
        //    return Task.FromResult(new HelloReply { Message = "OLG " + request.Name });
        //}

        //public override Task<PassOrFail> PrintfText(bePrintfText request, ServerCallContext context)
        //{
        //    //return base.PrintfText(request, context);
        //    //throw new NotImplementedException();
        //    Console.WriteLine(request.Text);
        //    return Task.FromResult(new PassOrFail { Result = false });
        //}

        //public override Task<ImageInfo> ReadImage(ImageInfo request, ServerCallContext context)
        //{
        //    HObject image = null;
        //    HTuple width, height;
        //    HOperatorSet.GenEmptyObj(out image);
        //    if(request.Index==0)
        //    {
        //        Console.WriteLine("失败");
        //        return Task.FromResult(request);
        //    }
        //    if(!HobjectMap.TryGetValue(request.Index, out image) && request.Path!="" && request.Path!=null)
        //    {
        //        HOperatorSet.ReadImage(out image, request.Path);
        //        HobjectMap.Add(request.Index, image);
        //        HOperatorSet.GetImageSize(image, out width, out height);
        //        request.Width = width.TupleInt();
        //        request.Height = height.TupleInt();
        //    }
        //    return Task.FromResult(request);
        //}
    }
    public class Program
    {
        static void Main(string[] args)
        {
            const int LocalHost = 30051;
            Console.WriteLine("Hello World!");
            Server server = new Server
            {
                Services = { Greeter.BindService(new RpcTest()) },
                Ports = { new ServerPort("localhost", LocalHost, ServerCredentials.Insecure) }
            };
            server.Start();
            Console.WriteLine($"开启GRPC服务成功:端口: {LocalHost}");
            Console.ReadKey();
            server.ShutdownAsync().Wait();
            Console.WriteLine("关闭GRPC");
        }
    }
}
