using System;
using System.Collections.Generic;
using System.Management;
using System.Threading;
using System.Threading.Tasks;
using PCL.Core.Logging;

namespace PCL.Core.Utils.OS;

public static class HardwareInfo
{
    private static readonly object _Lock = new();
    
    /// <summary>
    /// 系统 CPU 信息
    /// </summary>
    public static string CPUName = "Unknown";

    /// <summary>
    /// 系统 GPU 信息
    /// </summary>
    public static IReadOnlyList<GPUInfo> GPUs { get; private set; } = [];

    /// <summary>
    /// 已安装物理内存大小，单位 MiB
    /// </summary>
    public static long SystemMemorySize = (long)KernelInterop.GetPhysicalMemoryBytes().Total / 1024 / 1024;

    public readonly record struct GPUInfo(string Name, string DriverVersion, long Memory);

    /// <summary>
    ///     获取系统信息，例如 CPU 与 GPU，并存储到 CPUName 和 GPUs。
    ///     <para>
    ///     <b>不要在主线程上调用。</b>WMI 首次查询需要拉起 <c>Winmgmt</c> 服务，
    ///     实测在普通机器上要 1 秒以上，放在启动关键路径上会让欢迎页白白多转一秒。
    ///     </para>
    ///     <para>
    ///     查询语句只列真正用到的字段，不写 <c>SELECT *</c>：<c>Win32_VideoController</c>
    ///     有五六十个属性，全取一遍的序列化开销比取三个字段大得多。
    ///     </para>
    /// </summary>
    public static void GetHardwareInfo()
    {
        // CPU
        var cpuName = (string?)null;
        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\CIMV2", "SELECT Name FROM Win32_Processor");
            foreach (ManagementObject queryObj in searcher.Get())
            {
                cpuName = queryObj["Name"]?.ToString()?.Trim();
                break;
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "获取 CPU 信息时出错");
        }

        // GPU
        var gpuList = new List<GPUInfo>();
        try
        {
            using var searcher = new ManagementObjectSearcher(@"root\CIMV2",
                "SELECT Name, DriverVersion, AdapterRAM FROM Win32_VideoController");
            foreach (ManagementObject queryObj in searcher.Get())
            {
                var gpuInfo = new GPUInfo
                {
                    Name = queryObj["Name"]?.ToString() ?? "",
                    DriverVersion = queryObj["DriverVersion"]?.ToString() ?? "",
                    Memory = queryObj["AdapterRAM"] is not null and not DBNull
                        ? Convert.ToInt64(queryObj["AdapterRAM"]) / (1024 * 1024)
                        : 0
                };
                gpuList.Add(gpuInfo);
            }
        }
        catch (Exception ex)
        {
            LogWrapper.Warn(ex, "获取 GPU 信息时出错");
        }

        lock (_Lock)
        {
            if (cpuName is not null)
                CPUName = cpuName;
            if (gpuList.Count > 0)
                GPUs = gpuList;
        }
        LogWrapper.Info("已获取系统硬件信息");
    }

    /// <summary>
    ///     后台取一次硬件信息，不阻塞调用方。
    ///     <para>
    ///     用「已启动」标记保证整个进程只跑一次：这个结果只是拿来展示，
    ///     重复查询除了再等一秒没有任何收益。
    ///     </para>
    /// </summary>
    public static void GetHardwareInfoInBackground()
    {
        if (Interlocked.Exchange(ref _bgStarted, 1) != 0) return;
        Task.Run(() =>
        {
            try
            {
                GetHardwareInfo();
            }
            catch (Exception ex)
            {
                LogWrapper.Warn(ex, "后台获取硬件信息时出错");
            }
        });
    }

    private static int _bgStarted;
}