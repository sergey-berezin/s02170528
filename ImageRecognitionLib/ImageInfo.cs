using System.Collections.Generic;

namespace ImageRecognitionLib
{
    public class ImageInfo
    {
        public string path { get; set; } = "";
        public string name { get; set; } = "";
        public List<string> className { get; set; }
        public List<float> confidence { get; set; }
        public ImageInfo()
        {
            className = new List<string>();
            confidence = new List<float>();
        }
        public void AddInfo(string p, string cl, float conf)
        {
            path = p;
            className.Add(cl);
            confidence.Add(conf);
        }
        public override string ToString()
        {
            string res = path + "\n";
            for (int i = 0; i < className.Count; i++)
            {
                res += className[i] + " with confidence " + confidence[i] + "\n";
            }
            return res;
        }
    }
}