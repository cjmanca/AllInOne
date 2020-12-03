using DrivePool.Integration.Info.Balancing;
using System;
using System.Runtime.CompilerServices;

namespace AllInOnePlugin
{
    internal static class Integration
    {
        public static ulong GetAvailableFreeSpace(this BalanceStateInfo balanceState, BalancerSettingsState settings)
        {
            double totalSize = (double)((float)balanceState.TotalSize) * settings.ArchiveFillSettings.FillRatio - (double)((float)balanceState.UsedSpace);
            if (totalSize < 0)
            {
                totalSize = 0;
            }
            long num = (long)(balanceState.TotalSize - settings.ArchiveFillSettings.FillBytes - balanceState.UsedSpace);
            if (num < (long)0)
            {
                num = (long)0;
            }
            if (!settings.ArchiveFillSettings.IsFillBytesChecked)
            {
                num = (long)0;
            }
            return (ulong)Math.Max(num, (long)totalSize);
        }
    }
}
