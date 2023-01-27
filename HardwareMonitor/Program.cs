using ConsoleService;
using OpenHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HardwareMonitor
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Service service = new Service();

            service.RunRefreshing(400);

            Console.Write("set datakey : ");

            service.RunDataPost(Console.ReadLine());

            service.RunListener("8088");

            Console.ReadLine();

        }
    }
}
