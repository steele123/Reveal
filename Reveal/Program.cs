using System;
using System.Threading.Tasks;
using LCUSharp;
using LCUSharp.Websocket;

namespace Reveal
{
    class Program
    {
        static async Task Main(string[] args)
        {
            while (true)
            {
                var revealer = new Revealer();
                await revealer.Connect();
            }
        }
    }
}