using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFUIBenchmark
{
    class UIInitializer
    {
        public static System.Windows.Application CreateUIApplication(int width = 500, int height = 250)
        {
            var waitForApplicationRun = new TaskCompletionSource<System.Windows.Application>();
            System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                var application = new System.Windows.Application();

                application.Startup += (s, e) =>
                {
                    application.Dispatcher.Invoke(() =>
                    {
                        var window = application.MainWindow = new System.Windows.Window() { Width = width, Height = height };
                        window.Content = new System.Windows.Controls.ContentControl();
                        window.Show();
                        if (window.IsLoaded)
                        {
                            waitForApplicationRun.TrySetResult(application);
                        }
                        else
                        {
                            window.Loaded += (s2, e2) => waitForApplicationRun.TrySetResult(application);
                        }
                    });
                };
                application.Run();
            }));
            t.SetApartmentState(System.Threading.ApartmentState.STA);
            t.Start();
            waitForApplicationRun.Task.Wait();
            return waitForApplicationRun.Task.Result;
        }
    }
}
