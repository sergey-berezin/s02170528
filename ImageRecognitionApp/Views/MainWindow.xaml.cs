using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ImageRecognitionApp.ViewModels;
using System.Threading.Tasks;

namespace ImageRecognitionApp.Views
{
    class MyAppUIServices : UIServices
    {
        Window _window;
        ComboBox _classSelectorComboBox;
        ListBox _processedImagedListBox;
        ListBox _chosenClassListBox;
        TextBlock _processedImageTextBlock;
        TextBlock _chosenClassTextBlock;
        TextBlock _progressBarTextBlock;
        TextBlock _filterComboBoxTextBlock;
        ProgressBar _completionProgressBar;

        public MyAppUIServices(Window window, ComboBox classSelectorComboBox,
                               ListBox processedImagedListBox, ListBox chosenClassListBox,
                               TextBlock processedImageTextBlock, TextBlock chosenClassTextBlock,
                               TextBlock progressBarTextBlock, TextBlock filterComboBoxTextBlock,
                               ProgressBar completionProgressBar)
        {
            _window = window;
            _classSelectorComboBox = classSelectorComboBox;
            _processedImagedListBox = processedImagedListBox;
            _chosenClassListBox = chosenClassListBox;
            _filterComboBoxTextBlock = filterComboBoxTextBlock;
            _processedImageTextBlock = processedImageTextBlock;
            _chosenClassTextBlock = chosenClassTextBlock;
            _progressBarTextBlock = progressBarTextBlock;
            _completionProgressBar = completionProgressBar;
        }

        public async Task<string> ShowOpenDialogAsync() 
        {
            var OFD = new OpenFolderDialog();
            OFD.Directory = @"../..";
            OFD.Title = "Choose directory";
            
            var imgPath = await OFD.ShowAsync(_window);

            return imgPath;
        }
        
        public void IsVisibleProgressBar(bool value)
        {
            _progressBarTextBlock.IsVisible = value;
            _completionProgressBar.IsVisible = value;
        }

        public void IsVisibleProcessedImageViewer(bool value) 
        {
            _processedImagedListBox.IsVisible = value;
            _processedImageTextBlock.IsVisible = value;
        }

        public void IsVisibleFilteredImageViewer(bool value) 
        {
            _chosenClassListBox.IsVisible = value;
            _chosenClassTextBlock.IsVisible = value;
        }

        public void IsVisibleClassFilter(bool value) 
        {
            _classSelectorComboBox.IsVisible = value;
            _filterComboBoxTextBlock.IsVisible = value;
        }
    }
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            var appUIservices = new MyAppUIServices(this,
                this.FindControl<ComboBox>("ClassSelectorComboBox"),
                this.FindControl<ListBox>("ProcessedImageListBox"),
                this.FindControl<ListBox>("ChosenClassListBox"),
                this.FindControl<TextBlock>("ProcessedImageTextBlock"),
                this.FindControl<TextBlock>("ChosenClassTextBlock"),
                this.FindControl<TextBlock>("ProgressBarTextBlock"),
                this.FindControl<TextBlock>("FilterComboBoxTextBlock"),
                this.FindControl<ProgressBar>("CompletionProgressBar"));
            var viewModel = new MainWindowViewModel(appUIservices);

            DataContext = viewModel;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}