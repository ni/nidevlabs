using System;

namespace NationalInstruments.WebServiceHost
{
    /// <summary>
    /// Main entry point to the GLL Web Service hosting process
    /// </summary>
    public static class ServerMain
    {
        [STAThread]
        private static void Main()
        {
            WebServiceRunner.WebServiceRunner.RunServer();
        }
    }
}