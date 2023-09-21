using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.ExtendedExecution;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TimerBackgroundApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public int timerCount = 0;
        public DispatcherTimer timer = null;
        public MainPage()
        {
            this.InitializeComponent();
            try
            {
                ExtendedExecutionCall();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("error at constructor call :" + ex);
            }
        }

        public async void ExtendedExecutionCall()
        {
            // Create an extended execution session
            var newSession = new ExtendedExecutionSession();
            newSession.Reason = ExtendedExecutionReason.Unspecified;
            newSession.Description = "Long Running Processing";
            newSession.Revoked += (sender, args) =>
            {
                Trace.WriteLine("Application went to suspention mode : ExtendedExecutionSession SessionRevoked");
            };
            ExtendedExecutionResult result = await newSession.RequestExtensionAsync();
            Trace.WriteLine("ExtendedExecutionResult : " + result);
            if (result == ExtendedExecutionResult.Denied)
            {
                // Extended execution was denied by the user
                // Handle this case accordingly
            }
            else
            {
                ConstructorCall();
                // Extended execution was granted; you can perform your timer-related background tasks here
            }

        }

        public async void ConstructorCall()
        {
            timerCount = 0;
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1 * 1000);
            timer.Tick += (sender, e) =>
            {
                timerCount = timerCount + 1;
                timerCountValue.Text = "Timer Count : " + timerCount;
                LogTheMessage("timerCount : " + timerCount);
            };
            timer.Start();
        }

        public async void LogTheMessage(string message)
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                // Check if the custom folder exists; if not, create it
                StorageFolder customFolder = await localFolder.CreateFolderAsync("TimerBackgroundAppLogs", CreationCollisionOption.OpenIfExists);

                // Create the file with the specified name inside the custom folder
                StorageFile logFile = await customFolder.CreateFileAsync("TimerBackgroundApp.txt", CreationCollisionOption.OpenIfExists);

                await Windows.Storage.FileIO.AppendTextAsync(logFile, $"File is located at {logFile.Path.ToString()}\n" + $" - {DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}" + Environment.NewLine);

                Trace.WriteLine(String.Format("File is located at {0}", logFile.Path.ToString()));
            }
            catch (TaskCanceledException tcex)
            {
                Trace.WriteLine("Task canceled: " + tcex.Message);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Error in log creation function : " + ex.Message);
                // Handle any other exceptions that may occur while writing to the log file
            }
        }
    }
}
