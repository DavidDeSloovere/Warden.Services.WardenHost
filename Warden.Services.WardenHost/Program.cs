using System;
using Warden.Services.WardenHost.Framework;

namespace Warden.Services.WardenHost
{
    class Program
    {
        public static void Main(string[] args)
        {
            Bootstrapper.Initialize();
            Console.WriteLine("Press any key to quit...");
        }
    }
}
