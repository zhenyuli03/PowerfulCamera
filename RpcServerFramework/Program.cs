using Grpc.Core;
using GrpcGreeter;
using HalconDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;

namespace RpcServerFramework
{
    public class RpcTest : Greeter.GreeterBase
    {
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
        //    if (request.Index == 0)
        //    {
        //        Console.WriteLine("失败");
        //        return Task.FromResult(request);
        //    }
        //    try
        //    {
        //        if(HobjectMap.TryGetValue(request.Index, out image))//如果已存在，即释放
        //        {
        //            image.Dispose();
        //            image = null;
        //            HobjectMap.Remove(request.Index);
        //        }
        //        if (!HobjectMap.TryGetValue(request.Index, out image) && request.Path != "" && request.Path != null)
        //        {
        //            HOperatorSet.ReadImage(out image, request.Path);
        //            HobjectMap.Add(request.Index, image);
        //            HOperatorSet.GetImageSize(image, out width, out height);
        //            request.Width = width.TupleInt();
        //            request.Height = height.TupleInt();
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //    return Task.FromResult(request);
        //}
    }
    class Program
    {
        public static void Main(string[] args)
        {
            //const int LocalHost = 30051;
            //Console.WriteLine("Hello World!");
            //Server server = new Server
            //{
            //    Services = { Greeter.BindService(new RpcTest()) },
            //    Ports = { new ServerPort("localhost", LocalHost, ServerCredentials.Insecure) }
            //};
            //server.Start();
            //Console.WriteLine($"开启GRPC服务成功:端口: {LocalHost}");
            //Console.ReadKey();
            //server.ShutdownAsync().Wait();
            //Console.WriteLine("GRPC关闭");
            List<string> info;
            ReadXml("D:/CameraLite/Halcon_HDevEngine/HevEngineDome/CameraHDevEngine/bin/Debug/CapPolarity2.hdvp", out info);
            AssemblyFunction(info);
            Console.ReadKey();
        }

        public static void ReadXml(string path, out List<string> info)
        {
            string pdfXmlPath = Path.Combine(System.Windows.Forms.Application.StartupPath, path);
            XmlDocument doc = new XmlDocument();
            doc.Load(pdfXmlPath);
            XmlNode xn = doc.SelectSingleNode("hdevelop"); 
            XmlNode xn2 = xn.SelectSingleNode("procedure");

            XmlNode xn3 = xn2.SelectSingleNode("body");

            XmlNodeList list = xn3.ChildNodes;//获取到pdfcontentstring节点下的所有子节点
            info = new List<string>();
            foreach (XmlNode item in list)
            {
                XmlElement xe = (XmlElement)item;
                if (xe.InnerText.ToString() != "")
                    if(xe.InnerText.ToString()[0] == '*') //*号为屏蔽
                        continue;
                info.Add(xe.InnerText.ToString());
                Console.WriteLine(xe.InnerText.ToString());
                //info.Add(xe.GetAttribute("fieldname").ToString());
            }
            XmlNode interfacexml = xn2.SelectSingleNode("interface");
            XmlNode io = interfacexml.SelectSingleNode("io");
            XmlNode oo = interfacexml.SelectSingleNode("oo");
            XmlNodeList iolist = io.ChildNodes;
            foreach(XmlNode item in iolist)
            {
                XmlElement xe = (XmlElement)item;
                Console.WriteLine(xe.InnerText.ToString());
            }
    }

        public static void AssemblyFunction(List<string> info)
        {
            string command = "ReadImage";
            Type HOperatorSet = typeof(HOperatorSet);
            MethodInfo function = HOperatorSet.GetMethod(command);
            //function.Invoke()
            //string command;
            //for (int i = 0; i < info.Count(); i++)
            //{
            //    command = info[i];
            //    if (command[0] == '*') //*号为屏蔽
            //        continue;
            //}
        }
    }
}
