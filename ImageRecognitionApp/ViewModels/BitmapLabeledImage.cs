using System;
using System.IO;
using Avalonia.Media.Imaging;

namespace ImageRecognitionApp.ViewModels
{
    public class BitmapLabeledImage
    {
        public string FullName { get; set; }
        public string Name 
        {
            get { return Path.GetFileName(FullName); } 
        }
        public string Label { get; set; }
        public Bitmap AvaloniaBitmap { get; set; }

        public BitmapLabeledImage(string fullName, string label, Bitmap bitmap = null)
        {
            FullName = fullName;
            Label = label;
            AvaloniaBitmap = bitmap;
        }
        
        public override string ToString()
        {
            return Path.GetFileName(FullName) + " " + Label;
        }
    }
    
}