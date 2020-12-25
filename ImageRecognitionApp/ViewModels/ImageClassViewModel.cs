using System.ComponentModel;

namespace ImageRecognitionApp.ViewModels
{
    public class ImageClassViewModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        string type;
        int count = 0;
        public string Type
        {
            get
            {
                return type;
            }
            set
            {
                type = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Type)));
            }
        }
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            }
        }
        public ImageClassViewModel(string name, int c)
        {
            Type = name;
            Count = c;
        }
    }
}