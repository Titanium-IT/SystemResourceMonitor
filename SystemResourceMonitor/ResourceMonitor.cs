using System.Diagnostics;

namespace SystemResourceMonitor
{
    public class ResourceMonitor
    {
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _memoryCounter;

        public ResourceMonitor()
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
        }

        public (float CpuUsage, float AvailableMemory) GetResourceUsage()
        {
            float cpuUsage = _cpuCounter.NextValue();
            float availableMemory = _memoryCounter.NextValue();
            return (cpuUsage, availableMemory);
        }
    }
}
