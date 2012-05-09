using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GPUExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int secondsPassedCounter = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void MarshallActionToUIThread(Action action)
        {
            RunButton.Dispatcher.BeginInvoke(action);
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            // Disable the button and enable it 
            this.StatusLabel.Content = "Beginning processing";
            this.RunButton.IsEnabled = false;

            // Load static data

            // Fire the CPU thread
            BackgroundWorker cpuWorker = new BackgroundWorker();
            cpuWorker.WorkerSupportsCancellation = true;
            cpuWorker.WorkerReportsProgress = true;

            cpuWorker.DoWork += new DoWorkEventHandler(cpuWork);
            cpuWorker.ProgressChanged += new ProgressChangedEventHandler(cpuProgressChanged);
            cpuWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(cpuRunWorkerCompleted);

            if (cpuWorker.IsBusy != true)
            {
                cpuWorker.RunWorkerAsync();
            }

            // Fire the GPU thread

            // Start the cancelProcessingTimer
            System.Timers.Timer cancelProcessingTimer = new System.Timers.Timer();
            cancelProcessingTimer.Interval = 1000;
            cancelProcessingTimer.Elapsed +=
                (s, ev) =>
                {
                    MarshallActionToUIThread(() =>
                    {
                        secondsPassedCounter++;
                        if (secondsPassedCounter >= 10)
                        {
                            cpuWorker.CancelAsync();
                            this.StatusLabel.Content = String.Format("Processing stopped after {0} seconds", secondsPassedCounter.ToString());
                            this.RunButton.IsEnabled = true;
                            cancelProcessingTimer.Enabled = false;
                            secondsPassedCounter = 0;  // reset
                        }
                        else
                        {
                            this.StatusLabel.Content = String.Format("Running {0} seconds", secondsPassedCounter.ToString());
                        }
                    });
                };
            cancelProcessingTimer.Enabled = true;
        }

        private void cpuWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            for (int i = 1; (i <= 30); i++)
            {
                if ((worker.CancellationPending == true))
                {
                    e.Cancel = true;
                    worker.ReportProgress(0, i.ToString());
                    break;
                }
                else
                {
                    // Perform a time consuming operation and report progress.
                    System.Threading.Thread.Sleep(500);
                    worker.ReportProgress(0, i.ToString());
                }
            }
        }

        private void cpuProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.CpuLabel.Content = e.UserState.ToString();
        }

        private void cpuRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

    }
}
