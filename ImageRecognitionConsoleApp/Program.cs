using System;
using ImageRecognitionLib;
using System.Linq;
using System.Runtime.Loader;


namespace ImageRecognition
{
    class Program
    {
        private static void PrintLogs(LabeledImage img)
        {
            System.Console.WriteLine(img.Name + " " + img.Label);
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
            model.WorkAsync(imgPath);
        }
    }
}

