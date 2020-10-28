using System;
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
    public class MainWindowViewModel : ViewModelBase
    {
        UIServices _services;
        Model _model;
        string _imagePath;
        Output _log;

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
        
        public new event PropertyChangedEventHandler PropertyChanged;
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
   
        ObservableCollection<LabeledImage> _processedImageCollection;

        private ObservableCollection<LabeledImage> ProcessedImageCollection 
        {
            get => _processedImageCollection;
            set
            {
                _processedImageCollection = value;

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProcessedImageCollection)));
            } 
        }

        List<LabeledImage> _filteredImageCollection;
        public List<LabeledImage> FilteredImageCollection 
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
            _log = ProcessLabeledImage;
            _model = new Model(_log);
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
                var query = ProcessedImageCollection.Where(x => x.Label == SelectedIndexComboBoxProperty);
                FilteredImageCollection = query.ToList<LabeledImage>();
            }
        }

        void RefreshCollection(object obj, NotifyCollectionChangedEventArgs e)
        {
            var query = ProcessedImageCollection.Where(x => x.Label == SelectedIndexComboBoxProperty);
            FilteredImageCollection = query.ToList<LabeledImage>();
        }
        
        void ProcessLabeledImage(LabeledImage labeledImage)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                ProcessedImagesAmount++;
                ProcessedImageCollection.Add(labeledImage);
            });
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
            ProcessedImageCollection = new ObservableCollection<LabeledImage>();
            ProcessedImageCollection.CollectionChanged += RefreshCollection;
            _imagePath = null;
            _imagePath = await _services.ShowOpenDialogAsync();
            _services.IsVisibleProgressBar(true);

            if (_imagePath != null) 
            {
                _totalAmountOfImagesInDirectory = CountAmountOfImagesInDirectory();
                await _model.WorkAsync(_imagePath);
            }
            _services.IsVisibleProgressBar(false);
        }

        void InitLabels()
        {
            LabelsListComboBox = new List<string>();
            foreach (var t in LabelMap.ClassLabels)
                LabelsListComboBox.Add(t);
        }
    }
}
