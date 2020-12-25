using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Linq;
using ImageRecognitionLib;
using System.IO;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Threading;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Drawing.Imaging;


namespace ImageRecognitionApp.ViewModels
{
    public interface UIServices
    {
        Task<string> ShowOpenDialogAsync();
        void IsVisibleProgressBar(bool value);
        void IsVisibleProcessedImageViewer(bool value);
        void IsVisibleFilteredImageViewer(bool value);
        void IsVisibleClassFilter(bool value);
    }
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        UIServices _services;
        Model _model;
        string _imagePath;
        Output _output;

        int _processedImagesAmount;
        int ProcessedImagesAmount 
        {
            get => _processedImagesAmount;
            set 
            {
                _processedImagesAmount = value;
                PercentProcessed = (int)((double)_processedImagesAmount / _totalAmountOfImagesInDirectory * 100);

            }
        }

        public ApplicationContext db;
        
        public event PropertyChangedEventHandler PropertyChanged;
        public List<string> LabelsListComboBox { get; set; }

        int _totalAmountOfImagesInDirectory;
        int _percentProcessed;
        public int PercentProcessed
        {
            get { return _percentProcessed; }
            set 
            {
                _percentProcessed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PercentProcessed)));
            }
        }
        public ICommand ChooseDirCommand { get; set; }
        public ICommand InterruptProcessingCommand { get; set; }
        
        private readonly ICommand clearCommand;
        public ICommand Clear { get { return clearCommand; } }

        private readonly ICommand showCommand;
        public ICommand Show { get { return showCommand; } }

        bool clearFlag = false;
        
        string _selectedIndexComboBox;
        public string SelectedIndexComboBoxProperty
        {
             get => _selectedIndexComboBox;
             set 
             {
                 _selectedIndexComboBox = value;
                 PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedIndexComboBoxProperty)));
                 if (value != string.Empty)
                    _services.IsVisibleFilteredImageViewer(true);
             }
        }
   
        ObservableCollection<ImageClassViewModel> classVMs = new ObservableCollection<ImageClassViewModel>();
        public ObservableCollection<ImageClassViewModel> ClassVMs
        {
            get
            {
                return classVMs;
            }
            set
            {
                classVMs = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ClassVMs)));

            }
        }
        
        public ImageClassViewModel selectedImgType { get; set; }
        ObservableCollection<ImageViewModel> selectedImages = new ObservableCollection<ImageViewModel>();

        public ObservableCollection<ImageViewModel> SelectedImages
        {
            get
            {
                return selectedImages;
            }
            set
            {
                selectedImages = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedImages)));

            }

        }
        
        ObservableCollection<ImageViewModel> _processedImageCollection;

        private ObservableCollection<ImageViewModel> ProcessedImageCollection 
        {
            get => _processedImageCollection;
            set
            {
                _processedImageCollection = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProcessedImageCollection)));
            } 
        }

        List<ImageViewModel> _filteredImageCollection;
        public List<ImageViewModel> FilteredImageCollection 
        { 
            get { return _filteredImageCollection; } 
            set 
            {
                _filteredImageCollection = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilteredImageCollection)));
            } 
        }

        public MainWindowViewModel(UIServices services)
        {
            Init();

            _services = services;
            PropertyChanged += ReactToSelectedIndexComboBox;
            ProcessedImageCollection = null;
            _processedImagesAmount = 0;
            _output = ProcessLabeledImage;
            _model = new Model(_output);
            _model.PropertyChanged += CheckExecuteCondition;
        }

        void CheckExecuteCondition(object obj, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_model.IsProcessing))
                ((RelayCommand)InterruptProcessingCommand).RaiseCanExecuteChanged(this, e);
        }

        void ReactToSelectedIndexComboBox(object obj, PropertyChangedEventArgs e)
        {
            if ((e.PropertyName == nameof(SelectedIndexComboBoxProperty)) && (ProcessedImageCollection != null))
            {
                var query = ProcessedImageCollection.Where(x => x.ClassName == SelectedIndexComboBoxProperty);
                FilteredImageCollection = query.ToList<ImageViewModel>();
            }
        }

        void RefreshCollection(object obj, NotifyCollectionChangedEventArgs e)
        {
            var query = ProcessedImageCollection.Where(x => x.ClassName == SelectedIndexComboBoxProperty);
            FilteredImageCollection = query.ToList<ImageViewModel>();
        }
        
        void ProcessLabeledImage(ImageInfo labeledImage)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProcessedImagesAmount++;
                //ProcessedImageCollection.Add(labeledImage);
            });
        }
        string _stats;
        public string Statistics
        {
            get
            {
                return _stats;
            }
            set
            {
                _stats = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Statistics)));
            }
        }
        public void GetStatistics()
        {
            string res = "";
            var q = from img in db.Images select img;
            foreach(var img in q)
            {
                res += img.Path + "; Number of requests: " + img.count + "\n";
            }
            Statistics = res;
        }
        private byte[] ConvertImageToByteArray(string fileName)
        {
            Bitmap bitMap = new Bitmap(fileName);
            ImageFormat bmpFormat = bitMap.RawFormat;
            var imageToConvert = System.Drawing.Image.FromFile(fileName);
            using (MemoryStream ms = new MemoryStream())
            {
                imageToConvert.Save(ms, bmpFormat);
                return ms.ToArray();
            }
        }

        void Init() 
        {
            InitLabels();
            InitCommands();
        }

        void InitCommands()
        {
            ChooseDirCommand = new RelayCommand(async (object o) => await TryChooseDirectory());
            InterruptProcessingCommand = new RelayCommand((object o) => TryToInterrupt(), 
                                                          (object o) => CanExecuteInterruptCommand());
        }

        private bool CanExecuteInterruptCommand()
        {
            return _model.IsProcessing;
        }

        private void TryToInterrupt()
        {
            _model.TerminateProcessing();
        }

        int CountAmountOfImagesInDirectory() 
        {
            var counter = 0;

            foreach (var item in Directory.GetFiles(_imagePath))
                counter++;

            return counter;
        }

        async Task TryChooseDirectory()
        {
            _services.IsVisibleProcessedImageViewer(true);
            _services.IsVisibleClassFilter(true);
            _processedImagesAmount = 0;
            PercentProcessed = 0;
            ProcessedImageCollection = new ObservableCollection<ImageViewModel>();
            ProcessedImageCollection.CollectionChanged += RefreshCollection;
            _imagePath = null;
            _imagePath = await _services.ShowOpenDialogAsync();
            _services.IsVisibleProgressBar(true);
            db = new ApplicationContext();
            classVMs = new ObservableCollection<ImageViewModel>();

            if (_imagePath != null) 
            {
                _totalAmountOfImagesInDirectory = CountAmountOfImagesInDirectory();
                await _model.WorkAsync(_imagePath);
                foreach (var file in Directory.GetFiles(_imagePath))
                { 
                    bool flag = false;
                    var fileInfo = new FileInfo(file);
                    _processedImageCollection.Add(new ImageViewModel(fileInfo.FullName, fileInfo.Name));
                    foreach (var img in db.Images)
                    {
                        if (fileInfo.FullName == img.Path)
                        {
                            var code1 = ConvertImageToByteArray(fileInfo.FullName);
                            IStructuralEquatable equ = code1;
                            var code2 = img.Details.Image;
                            if (equ.Equals(code2, EqualityComparer<object>.Default))
                            {
                                img.count++;

                                db.SaveChanges();
                                _processedImageCollection.Add(new ImageViewModel(fileInfo.FullName, fileInfo.Name, img.Confidence, img.ClassName));

                                flag = true;
                                _processedImagesAmount++;

                                Dispatcher.BeginInvoke(new Action(() =>
                                {
                                    if (ClassVMs.Any())
                                    {
                                        bool flag1 = false;
                                        foreach (var imgClass in ClassVMs)
                                        {
                                            if (imgClass.Type == img.ClassName)
                                            {
                                                imgClass.Count++;
                                                flag1 = true;
                                                break;
                                            }
                                        }
                                        if (!flag1)
                                        {
                                            ClassVMs.Add(new ImageClassViewModel(img.ClassName, 1));

                                        }

                                    }
                                    else
                                    {
                                        ClassVMs.Add(new ImageClassViewModel(img.ClassName, 1));

                                    }
                                }));
                                break;
                            }

                        }
                    }

                    if (!flag)
                    {
                        _processedImageCollection.Add(new ImageViewModel(fileInfo.FullName, fileInfo.Name));
                    }
                    
                }
            }
            _services.IsVisibleProgressBar(false);
        }

        void InitLabels()
        {
            LabelsListComboBox = new List<string>();
            foreach (var t in LabelMap.ClassLabels)
                LabelsListComboBox.Add(t);
        }
        
        public void ClearDB()
        {
            //db.Database.ExecuteSqlRaw("DELETE from Images");
            db.Images.RemoveRange(db.Images);

            db.SaveChanges();
            clearFlag = true;

        }
    }
}
