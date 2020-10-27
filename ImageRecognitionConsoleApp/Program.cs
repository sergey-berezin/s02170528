using System;
using ImageRecognitionLib;
using System.Linq;
using System.Runtime.Loader;


namespace ImageRecognition
{
    class Program
    {
        private static void PrintLogs(string message)
        {
            System.Console.WriteLine(message);
        }
        static void Main(string[] args)
        {
            var imgPath = args.FirstOrDefault() ?? "./../../../../ImageRecognitionLib";

            var model = new Model(PrintLogs, imgPath);

            Console.CancelKeyPress += (sender, eArgs) =>
            {
                model.Stop();
                eArgs.Cancel = true;
            };
            model.Work();
        }
    }
}

