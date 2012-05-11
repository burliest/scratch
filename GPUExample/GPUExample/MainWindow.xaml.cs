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
using System.Xml;
using System.Xml.Linq;

namespace GPUExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        struct Geocode
        {
            public int id;
            public float latitude;
            public float longitude;
        }

        private int secondsPassedCounter = 0;
        private IList<Geocode> geocodes = new List<Geocode>();

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
            this.StatusLabel.Content = "Beginning processing";
            this.RunButton.IsEnabled = false;

            #region Fire the GPU thread

            BackgroundWorker gpuWorker = new BackgroundWorker();
            gpuWorker.WorkerSupportsCancellation = true;
            gpuWorker.WorkerReportsProgress = true;

            gpuWorker.DoWork += new DoWorkEventHandler(gpuWork);
            gpuWorker.ProgressChanged += new ProgressChangedEventHandler(gpuProgressChanged);
            gpuWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(gpuRunWorkerCompleted);

            if (gpuWorker.IsBusy != true)
            {
                gpuWorker.RunWorkerAsync();
            }

            #endregion

            #region Fire the CPU thread

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

            #endregion

            #region Start the cancelProcessingTimer

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
                            gpuWorker.CancelAsync();
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

            #endregion
        }

        private void cpuWork(object sender, DoWorkEventArgs e)
        {
            int BLOCKSIZE = 5000;
            int recordcount = 0;

            BackgroundWorker worker = sender as BackgroundWorker;
            BenchmarkCPU cpu = new BenchmarkCPU();

            float[] latitudes = geocodes.Select(x => x.latitude).ToArray();
            float[] longitudes = geocodes.Select(x => x.longitude).ToArray();

            for (int i = 0; i < latitudes.Length; i += BLOCKSIZE)
            {
                for (int j = 0; j < longitudes.Length; j += BLOCKSIZE)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        worker.ReportProgress(0, recordcount.ToString());
                        break;
                    }
                    else
                    {
                        // Perform a time consuming operation and report progress.
                        float[] lat1 = new float[BLOCKSIZE];
                        float[] long1 = new float[BLOCKSIZE];
                        float[] lat2 = new float[BLOCKSIZE];
                        float[] long2 = new float[BLOCKSIZE];

                        Array.Copy(latitudes, i, lat1, 0, BLOCKSIZE);
                        Array.Copy(longitudes, i, long1, 0, BLOCKSIZE);
                        Array.Copy(latitudes, j, lat2, 0, BLOCKSIZE);
                        Array.Copy(longitudes, j, long2, 0, BLOCKSIZE);

                        float[] distances = cpu.CalculateGreaterCircleDistance(lat1, long1, lat2, long2);

                        recordcount += BLOCKSIZE;
                        worker.ReportProgress(0, recordcount.ToString());
                    }
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

        private void gpuWork(object sender, DoWorkEventArgs e)
        {
            int BLOCKSIZE = 5000;
            int recordcount = 0;

            BackgroundWorker worker = sender as BackgroundWorker;
            BenchmarkGPU gpu = new BenchmarkGPU();

            float[] latitudes = geocodes.Select(x => x.latitude).ToArray();
            float[] longitudes = geocodes.Select(x => x.longitude).ToArray();

            for (int i = 0; i < latitudes.Length; i += BLOCKSIZE)
            {
                for (int j = 0; j < longitudes.Length; j += BLOCKSIZE)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        worker.ReportProgress(0, recordcount.ToString());
                        break;
                    }
                    else
                    {
                        // Perform a time consuming operation and report progress.
                        float[] lat1 = new float[BLOCKSIZE];
                        float[] long1 = new float[BLOCKSIZE];
                        float[] lat2 = new float[BLOCKSIZE];
                        float[] long2 = new float[BLOCKSIZE];

                        Array.Copy(latitudes, i, lat1, 0, BLOCKSIZE);
                        Array.Copy(longitudes, i, long1, 0, BLOCKSIZE);
                        Array.Copy(latitudes, j, lat2, 0, BLOCKSIZE);
                        Array.Copy(longitudes, j, long2, 0, BLOCKSIZE);

                        float[] distances = gpu.CalculateGreaterCircleDistance(lat1, long1, lat2, long2);

                        recordcount += BLOCKSIZE;
                        worker.ReportProgress(0, recordcount.ToString());
                    }
                }
            }
        }

        private void gpuProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.GpuLabel.Content = e.UserState.ToString();
        }

        private void gpuRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var filedata =
                from p in XElement.Load("geocodes.xml").Elements("point")
                select new Geocode()
                    {
                        id = Convert.ToInt32(p.Attribute("id").Value),
                        latitude = Convert.ToSingle(p.Attribute("lat").Value),
                        longitude = Convert.ToSingle(p.Attribute("long").Value)
                    };

            geocodes = filedata.ToList<Geocode>();

            this.StatusLabel.Content = "Waiting to run test";
        }

    }
}
