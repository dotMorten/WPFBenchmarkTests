using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.CsProj;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WPFUIBenchmark
{
    public class Program
    {
        public static Application app;
        static void Main(string[] args)
        {
#if DEBUG
            var test = new BenchmarkTests();
            test.Setup();
            test.LoadImageControl().Wait();
            Task.Delay(5000).Wait();
            test.Cleanup();
#else
            var summary = BenchmarkRunner.Run<BenchmarkTests>();
#endif
        }
        public class MultipleRuntimes : ManualConfig
        {
            public MultipleRuntimes()
            {
                Add(Job.Default.With(CsProjClassicNetToolchain.Net471).AsBaseline()); // NET 4.7.1
                Add(Job.Default.With(CsProjCoreToolchain.NetCoreApp30)); // .NET Core 3.0
            }
        }

        [Config(typeof(MultipleRuntimes))]
        [MemoryDiagnoser]
        public class BenchmarkTests
        {
            System.Windows.Application app;
            [GlobalSetup]
            public void Setup()
            {
                app = UIInitializer.CreateUIApplication(1024,768);
            }

            [GlobalCleanup]
            public void Cleanup()
            {
                app.Dispatcher.Invoke(() =>
                {
                    app.Shutdown();
                    app = null;
                });
            }

            [Benchmark]
            public Task LoadImageControl()
            {
                TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
                app.Dispatcher.Invoke(async () =>
                {
                    Image img = new Image() { Stretch = System.Windows.Media.Stretch.Fill, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };

                    img.LayoutUpdated += (s, e) =>
                    {
                        if (img.ActualWidth > 0)
                            tcs.TrySetResult(null);
                    };
                    TaskCompletionSource<object> tcs2 = new TaskCompletionSource<object>();
                    img.Loaded += (s, e) => tcs.SetResult(null);
                    app.MainWindow.Content = img;
                    await tcs.Task;
                    var imageSource = new System.Windows.Media.Imaging.BitmapImage();
                    imageSource.CreateOptions = System.Windows.Media.Imaging.BitmapCreateOptions.DelayCreation;
                    imageSource.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.None;
                    imageSource.BeginInit();
                    imageSource.UriSource = new Uri("image.jpg", UriKind.Relative);
                    imageSource.EndInit();
                    img.Source = imageSource;
                });
                return tcs.Task;
            }
        }
    }
}
