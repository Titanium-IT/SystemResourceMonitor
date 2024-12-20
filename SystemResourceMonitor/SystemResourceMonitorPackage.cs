using Microsoft;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace SystemResourceMonitor
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(SystemResourceMonitorPackage.PackageGuidString)]
    [ProvideAutoLoad(Microsoft.VisualStudio.Shell.Interop.UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class SystemResourceMonitorPackage : AsyncPackage
    {
        /// <summary>
        /// SystemResourceMonitorPackage GUID string.
        /// </summary>
        public const string PackageGuidString = "f7bfa0a0-3ba5-4c19-9d9f-6dde2424d5a1";

        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            _cancellationTokenSource = new CancellationTokenSource();

            // Start monitoring resources
            MonitorResourcesAsync(_cancellationTokenSource.Token);
        }

        /// <summary>
        /// Monitors system resources and outputs the data to the Output window.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation.</param>
        private async void MonitorResourcesAsync(CancellationToken cancellationToken)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            // Get the Output window service
            IVsOutputWindow outputWindow = await GetServiceAsync(typeof(SVsOutputWindow)) as IVsOutputWindow;
            Assumes.Present(outputWindow);

            // Create a custom pane in the Output window
            Guid paneGuid = Guid.NewGuid();
            outputWindow.CreatePane(ref paneGuid, "System Resource Monitor", 1, 1);
            outputWindow.GetPane(ref paneGuid, out IVsOutputWindowPane pane);
            Assumes.Present(pane);

            // Use the ResourceMonitor class
            var resourceMonitor = new ResourceMonitor();

            // Monitoring loop
            while (!cancellationToken.IsCancellationRequested)
            {
                var (cpuUsage, availableMemory) = resourceMonitor.GetResourceUsage();

                string outputMessage = $"CPU Usage: {cpuUsage:F2}% | Available Memory: {availableMemory} MB{Environment.NewLine}";
                pane.OutputString(outputMessage);

                // Delay before the next update
                await Task.Delay(1000, cancellationToken);
            }
        }

        /// <summary>
        /// Dispose method to clean up resources.
        /// </summary>
        /// <param name="disposing">Indicates whether the method was called from Dispose.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
