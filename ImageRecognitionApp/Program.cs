using ImageRecognitionLib;
using System.Linq;


namespace ImageRecognition
{
    class Program
    {
        static void Main(string[] args)
        {
            var imgPath = args.FirstOrDefault() ?? "./../../../../ImageRecognitionLib";

            var model = new Model(imagePath: imgPath);
            model.Work();
        }
    }
}

