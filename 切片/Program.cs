using System;

namespace 切片
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            int[] array = new int[3] { 3, 4, 5 };
            var d = array[0..2];
            Console.WriteLine(d[0]);
        }
    }
}
