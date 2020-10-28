using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using SixLabors.ImageSharp.Processing;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace ImageRecognitionLib
{
    public class LabeledImage
    {
        public LabeledImage(string fullName, string label) 
        {
            FullName = fullName;
            Label = label;
        }

        //name including absolute path
        public string FullName { get; }
        public string Name => Path.GetFileName(FullName);
        public string Label { get; }

        public override string ToString()
        {
            return Name + " " + Label;
        }
    }
    public delegate void Output(LabeledImage image);
    public class Model: INotifyPropertyChanged
    {
        private static readonly ManualResetEvent StopSignal = new ManualResetEvent(false);
        private readonly string _imagePath;
        private readonly InferenceSession _session;
        private ConcurrentQueue<string> _filenames;
        CancellationTokenSource _cts;

        readonly Output _log; 
        
        bool _finishedProcessing, _wasTerminated;
        public event PropertyChangedEventHandler PropertyChanged;
        public bool FinishedProcessing { get {return _finishedProcessing; } }

        bool _isProcessing;
        public bool IsProcessing 
        { 
            get { return _isProcessing; } 
            set 
            {
                _isProcessing = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsProcessing)));
            }
        }

        public Model(Output log, string imagePath = "./../../../../ImageRecognitionLib")
        {
            _imagePath = imagePath;
            _session = new InferenceSession("./ImageRecognitionLib/resnet152-v2-7.onnx");
            _log += log;
            _finishedProcessing = false;
            _cts = new CancellationTokenSource();
            IsProcessing = false;
        }

        private DenseTensor<float> ImageToTensor(string imagePath)
        {
            using var image = Image.Load<Rgb24>(imagePath);
            const int targetWidth = 224;
            const int targetHeight = 224;

            // change size of image to 224 x 224
            image.Mutate(x =>
            {
                x.Resize(new ResizeOptions
                {
                    Size = new Size(targetWidth, targetHeight),
                    Mode = ResizeMode.Crop // save proportions but crop leftovers
                });
            });

            // preprocess image
            var input = new DenseTensor<float>(new[] {1, 3, targetHeight, targetWidth}); //flatten to tensor
            var mean = new[] {0.485f, 0.456f, 0.406f};
            var stdDev = new[] {0.229f, 0.224f, 0.225f};
            for (var y = 0; y < targetHeight; y++)
            {
                Span<Rgb24> pixelSpan = image.GetPixelRowSpan(y);
                for (var x = 0; x < targetWidth; x++)
                {
                    input[0, 0, y, x] = ((pixelSpan[x].R / 255f) - mean[0]) / stdDev[0];
                    input[0, 1, y, x] = ((pixelSpan[x].G / 255f) - mean[1]) / stdDev[1]; // normalize image
                    input[0, 2, y, x] = ((pixelSpan[x].B / 255f) - mean[2]) / stdDev[2];
                }
            }

            return input;
        }

        private string Predict(DenseTensor<float> input)
        {
            //setup inputs and run session
            var inputs = new List<NamedOnnxValue> {NamedOnnxValue.CreateFromTensor("data", input)};
            using var results = _session.Run(inputs);

            // postprocess to get softmax vector
            var output = results.First().AsEnumerable<float>();
            var sum = output.Sum(x => (float)Math.Exp(x));
            var softmax = output.Select(x => (float)Math.Exp(x) / sum);
            
            //select labels with top 10 probabilities
            var top10 = softmax.Select((x, i) =>
                    new Tuple<string, float>(LabelMap.ClassLabels[i], x))
                    .OrderByDescending(x => x.Item2)
                    .Take(10);
            var prediction = "\n";
            foreach (var (label, confidence) in top10.ToList())
            {
                prediction += $"Label: {label}, confidence: {confidence}\n";
            }

            Console.WriteLine(prediction);
            return top10.First().Item1;
        }

        public void Stop() => StopSignal.Set();
        private void Worker(string[] filenames, CancellationToken ct)
        {
            foreach (var name in filenames)
            {
                if (ct.IsCancellationRequested)
                {
                    Console.WriteLine("Stopping thread by signal");
                    break;
                }

                var label = Predict(ImageToTensor(name));
                Console.WriteLine(new LabeledImage(name, label).ToString());

            }

            Console.WriteLine("Thread has finished working");
        }

        public async Task WorkAsync(string imgPath)
        {
            IsProcessing = true;
            var processingTask = new Task( (object path) => 
                {
                    _finishedProcessing = false;
                    _wasTerminated = false;
                    Work((string)path, _cts.Token);
                    _finishedProcessing = !_wasTerminated;
                },
                imgPath);
            processingTask.Start();
            await processingTask;
            IsProcessing = false;
        }
        public void Work(string imgPath, CancellationToken ct)
        {
            try
            {
                _filenames = new ConcurrentQueue<string>(Directory.GetFiles(imgPath, "*.jpg"));
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("These files don't exist!");
                return;
            }
            
            // stop routine manually by pressing Ctrl+C
            Console.CancelKeyPress += (sender, eArgs) =>
            {
                StopSignal.Set();
                eArgs.Cancel = true;
            };
            var maxProcCount = Environment.ProcessorCount;
            var threads = new Thread[maxProcCount];
            for (var i = 0; i < maxProcCount; ++i)
            {
                Console.WriteLine($"Starting thread {i}");
                threads[i] = new Thread( fileNames => Worker((string[])fileNames, ct));
                threads[i].Start();
            }

            for (var i = 0; i < maxProcCount; ++i)
            {
                if (!ct.IsCancellationRequested)
                    threads[i].Join();
                else
                    break;
            }

            Console.WriteLine("Done!");
        }
        public void TerminateProcessing()
        {
            _cts.Cancel();
            _wasTerminated = true;
        }
    }
}