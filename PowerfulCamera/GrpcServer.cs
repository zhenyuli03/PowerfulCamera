using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcGreeter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerfulCamera.PythonEdit;
using System.CodeDom.Compiler;
using PowerfulCamera.CameraTools;
using HalconWindow = PowerfulCamera.CameraTools.HalconWindow; //指定此次的HalconWindow为CameraTools.HalconWindow

namespace PowerfulCamera
{
    public class GrpcFunction : Greeter.GreeterBase
    {
        public event Action<string> print;
        public event Func<int, string> addHalconWindow;
        public override Task<Empty> PrintfMessage(_String request, ServerCallContext context)
        {
            print?.Invoke(request.Text);
            Console.WriteLine(request.Text);
            return Task.FromResult(new Empty { });
        }
        public override Task<_String> AddHalconWindow(Empty request, ServerCallContext context)
        {
            string key = "";
            key = addHalconWindow?.Invoke(200);
            return Task.FromResult(new _String { Text = key });
        }
    }


    public class GrpcServer
    {
        public Server server;
        private int localHost;
        public int LocalHost
        {
            set => localHost = value;
            get => localHost;
        }
        public bool IsOpen = false;

        /// <summary>
        /// 默认端口30051
        /// </summary>
        /// <param name="host"></param>
        public GrpcServer(Action<string> printfunction, Func<int, string> addwindow, int host = 30051)
        {
            LocalHost = host;
            IsOpen = false;
            var temp = new GrpcFunction();
            temp.print += printfunction;
            temp.addHalconWindow += addwindow;
            server = new Server
            {
                Services = { Greeter.BindService(temp) },
                Ports = { new ServerPort("localhost", LocalHost, ServerCredentials.Insecure) }
            };
        }
        /// <summary>
        /// 打开Grpc服务
        /// </summary>
        /// <param name="host">端口</param>
        public void Open()
        {
            try
            {
                server.Start();
                IsOpen = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        /// <summary>
        /// 重新启动Grpc
        /// </summary>
        public void Reset()
        {
            if (IsOpen)
            {
                Close();
                Open();
            }
            else
                Open();
        }

        /// <summary>
        /// 关闭Grpc服务
        /// </summary>
        public void Close()
        {
            if (IsOpen)
            {
                try
                {
                    server.ShutdownAsync().Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
