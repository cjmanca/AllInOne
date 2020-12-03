using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DrivePool.Integration.Info.Balancing;
using DrivePool.Integration.Balancing;
using DrivePool.Integration.Info;
using System.Runtime.CompilerServices;

namespace AllInOnePlugin
{
    public partial class Balancer
    {



        private void OrderedMove(IEnumerable<BalanceStateInfo> ssds, IEnumerable<BalanceStateInfo> archives)
        {
            if (!settings.OrderedPlacementSettings.UnduplicatedOrderEnabled)
            {
                this.MoveOutOfSsdsUnordered(settings, ssds, archives, BalanceStateInfo.FileTypes.UnprotectedFiles);
            }
            else
            {
                this.MoveOutOfSsdsOrdered(settings, ssds, archives, BalanceStateInfo.FileTypes.UnprotectedFiles);
            }
            if (settings.OrderedPlacementSettings.DuplicatedOrderEnabled)
            {
                this.MoveOutOfSsdsOrdered(settings, ssds, archives, BalanceStateInfo.FileTypes.ProtectedFiles);
            }
            else
            {
                this.MoveOutOfSsdsUnordered(settings, ssds, archives, BalanceStateInfo.FileTypes.ProtectedFiles);
            }
        }





        private void GetSortedBalanceStates(OrderedPlacementSettingsState settings, IEnumerable<BalanceStateInfo> archives, out IEnumerable<BalanceStateInfo> unduplicatedSorted, out IEnumerable<BalanceStateInfo> duplicatedSorted)
        {
            PoolPartSettingsState[] unduplicatedOrder;
            int num;
            List<BalanceStateInfo> unduplicatedSortedBalancedStates = new List<BalanceStateInfo>();
            List<BalanceStateInfo> duplicatedSortedBalancedStates = new List<BalanceStateInfo>();
            if (settings.UnduplicatedOrder != null)
            {
                unduplicatedOrder = settings.UnduplicatedOrder;
                for (num = 0; num < (int)unduplicatedOrder.Length; num++)
                {
                    PoolPartSettingsState poolPartSettingsState1 = unduplicatedOrder[num];
                    BalanceStateInfo balanceStateInfo3 = (
                        from bs2 in archives
                        where bs2.Uid == poolPartSettingsState1.Uid
                        select bs2).FirstOrDefault<BalanceStateInfo>();
                    if (balanceStateInfo3 != null)
                    {
                        unduplicatedSortedBalancedStates.Add(balanceStateInfo3);
                    }
                }
            }
            if (!archives.Any<BalanceStateInfo>() || !(archives.First<BalanceStateInfo>().PoolParts.First<PoolPartInfo>().GetType().GetProperty("FirstSeen") != null))
            {
                foreach (BalanceStateInfo balanceStateInfo4 in
                    from bs in archives
                    orderby (double)((float)bs.UsedSpace) / (double)((float)bs.TotalSize)
                    select bs)
                {
                    if (unduplicatedSortedBalancedStates.Any<BalanceStateInfo>((BalanceStateInfo bs2) => bs2.Uid == balanceStateInfo4.Uid))
                    {
                        continue;
                    }
                    unduplicatedSortedBalancedStates.Add(balanceStateInfo4);
                }
                if (!settings.DuplicatedOrderSameAsUnduplicated)
                {
                    if (settings.DuplicatedOrder != null)
                    {
                        unduplicatedOrder = settings.DuplicatedOrder;
                        for (num = 0; num < (int)unduplicatedOrder.Length; num++)
                        {
                            PoolPartSettingsState poolPartSettingsState2 = unduplicatedOrder[num];
                            BalanceStateInfo balanceStateInfo5 = (
                                from bs2 in archives
                                where bs2.Uid == poolPartSettingsState2.Uid
                                select bs2).FirstOrDefault<BalanceStateInfo>();
                            if (balanceStateInfo5 != null)
                            {
                                duplicatedSortedBalancedStates.Add(balanceStateInfo5);
                            }
                        }
                    }
                    foreach (BalanceStateInfo balanceStateInfo6 in
                        from bs in archives
                        orderby (double)((float)bs.UsedSpace) / (double)((float)bs.TotalSize)
                        select bs)
                    {
                        if (duplicatedSortedBalancedStates.Any<BalanceStateInfo>((BalanceStateInfo bs2) => bs2.Uid == balanceStateInfo6.Uid))
                        {
                            continue;
                        }
                        duplicatedSortedBalancedStates.Add(balanceStateInfo6);
                    }
                }
                else
                {
                    duplicatedSortedBalancedStates = unduplicatedSortedBalancedStates;
                }
            }
            else
            {
                foreach (BalanceStateInfo bs in (from bs in archives
                                                 orderby bs.UsedSpace / bs.TotalSize
                                                 select bs))
                {
                    if (!unduplicatedSortedBalancedStates.Any((BalanceStateInfo bs2) => bs2.Uid == bs.Uid))
                    {
                        unduplicatedSortedBalancedStates.Add(bs);
                    }
                }
                if (settings.DuplicatedOrderSameAsUnduplicated)
                {
                    duplicatedSortedBalancedStates = unduplicatedSortedBalancedStates;
                }
                else
                {
                    if (settings.DuplicatedOrder != null)
                    {
                        PoolPartSettingsState[] array = settings.DuplicatedOrder;
                        for (int i = 0; i < array.Length; i++)
                        {
                            PoolPartSettingsState pp = array[i];
                            BalanceStateInfo balanceStateInfo2 = (from bs2 in archives
                                                                  where bs2.Uid == pp.Uid
                                                                  select bs2).FirstOrDefault<BalanceStateInfo>();
                            if (balanceStateInfo2 != null)
                            {
                                duplicatedSortedBalancedStates.Add(balanceStateInfo2);
                            }
                        }
                    }
                    foreach (BalanceStateInfo bs in (from bs in archives
                                                     orderby bs.UsedSpace / bs.TotalSize
                                                     select bs))
                    {
                        if (!duplicatedSortedBalancedStates.Any((BalanceStateInfo bs2) => bs2.Uid == bs.Uid))
                        {
                            duplicatedSortedBalancedStates.Add(bs);
                        }
                    }
                }
            }
            unduplicatedSorted = unduplicatedSortedBalancedStates;
            duplicatedSorted = duplicatedSortedBalancedStates;
        }

        private double GetSsdLimitRatio(BalancerSettingsState settings, BalanceStateInfo ssd)
        {
            if (!settings.SsdFillSettings.IsFillBytesChecked || settings.SsdFillSettings.FillBytes <= (long)0)
            {
                return settings.SsdFillSettings.FillRatio;
            }
            double fillBytes = (double)((float)settings.SsdFillSettings.FillBytes) / (double)((float)ssd.TotalSize);
            fillBytes = 1 - fillBytes;
            return Math.Max(settings.SsdFillSettings.FillRatio, fillBytes);
        }


        private void MoveOutExisting(BalancerSettingsState settings, IEnumerable<BalanceStateInfo> sortedBalanceStates, BalanceStateInfo.FileTypes fileTypes)
        {
            IEnumerable<BalanceStateInfo> balanceStateInfos;
            BalanceStateInfo.MoveOptions moveOption;
            balanceStateInfos = (fileTypes != BalanceStateInfo.FileTypes.UnprotectedFiles ? sortedBalanceStates : sortedBalanceStates.Reverse<BalanceStateInfo>());
            if (fileTypes == BalanceStateInfo.FileTypes.ProtectedFiles && sortedBalanceStates.Count<BalanceStateInfo>() <= 2)
            {
                return;
            }
            int num1 = 0;
            foreach (BalanceStateInfo archive in balanceStateInfos)
            {

                bool allowDuplicated = settings.DuplicatedDrives[archive.Uid];
                bool allowUnduplicated = settings.UnduplicatedDrives[archive.Uid];

                if (fileTypes == BalanceStateInfo.FileTypes.ProtectedFiles && !allowDuplicated)
                {
                    archive.SetLimit(BalanceStateInfo.LimitTypes.NoProtectedFiles, limitOptions);
                    archive.MoveFiles(BalanceStateInfo.FileTypes.ProtectedFiles, 0);
                    continue;
                }
                if (fileTypes == BalanceStateInfo.FileTypes.UnprotectedFiles && !allowUnduplicated)
                {
                    archive.SetLimit(BalanceStateInfo.LimitTypes.NoUnprotectedFiles, limitOptions);
                    archive.MoveFiles(BalanceStateInfo.FileTypes.UnprotectedFiles, 0);
                    continue;
                }

                int num2 = balanceStateInfos.ToList<BalanceStateInfo>().IndexOf(archive);
                List<BalanceStateInfo> list = sortedBalanceStates.Take<BalanceStateInfo>(num2).ToList<BalanceStateInfo>();
                if (!list.Any<BalanceStateInfo>())
                {
                    continue;
                }
                Func<int, BalanceStateInfo> func = (int shiftBy) => {
                    BalanceStateInfo balanceStateInfo;
                    int num = this._rand.Next(0, shiftBy + 1);
                    BalanceStateInfo balanceStateInfo1 = null;
                    List<BalanceStateInfo>.Enumerator enumerator = list.GetEnumerator();
                    try
                    {
                        while (enumerator.MoveNext())
                        {
                            BalanceStateInfo current = enumerator.Current;
                            if (current.GetAvailableFreeSpace(settings) <= (long)0)
                            {
                                continue;
                            }
                            if (num <= 0)
                            {
                                balanceStateInfo = current;
                                return balanceStateInfo;
                            }
                            else
                            {
                                balanceStateInfo1 = current;
                                num--;
                            }
                        }
                        return balanceStateInfo1;
                    }
                    finally
                    {
                        ((IDisposable)enumerator).Dispose();
                    }
                    return balanceStateInfo;
                };
                BalanceStateInfo balanceStateInfo3 = null;
                do
                {
                    balanceStateInfo3 = (fileTypes != BalanceStateInfo.FileTypes.ProtectedFiles ? func(0) : func(list.Count<BalanceStateInfo>()));
                    if (balanceStateInfo3 == null)
                    {
                        break;
                    }
                    list.Remove(balanceStateInfo3);
                    moveOption = new BalanceStateInfo.MoveOptions(balanceStateInfo3)
                    {
                        MaximumFillRatio = this.GetArchiveLimitRatio(settings, balanceStateInfo3)
                    };
                }
                while (archive.MoveFiles(fileTypes, 0, moveOption).MoveRatio != 1);
                if (balanceStateInfo3 != null)
                {
                    num1++;
                }
                else
                {
                    return;
                }
            }
        }

        private double GetArchiveLimitRatio(BalancerSettingsState settings, BalanceStateInfo archive)
        {
            if (!settings.ArchiveFillSettings.IsFillBytesChecked || settings.ArchiveFillSettings.FillBytes <= (long)0)
            {
                return settings.ArchiveFillSettings.FillRatio;
            }
            double fillBytes = (double)((float)settings.ArchiveFillSettings.FillBytes) / (double)((float)archive.TotalSize);
            fillBytes = 1 - fillBytes;
            return Math.Max(settings.ArchiveFillSettings.FillRatio, fillBytes);
        }

        private void MoveOutOfSsdsOrdered(BalancerSettingsState settings, IEnumerable<BalanceStateInfo> ssds, IEnumerable<BalanceStateInfo> archives, BalanceStateInfo.FileTypes fileTypes)
        {
            IEnumerable<BalanceStateInfo> balanceStateInfos;
            IEnumerable<BalanceStateInfo> balanceStateInfos1;
            this.GetSortedBalanceStates(settings.OrderedPlacementSettings, archives, out balanceStateInfos, out balanceStateInfos1);
            IEnumerable<BalanceStateInfo> balanceStateInfos2 = (fileTypes == BalanceStateInfo.FileTypes.ProtectedFiles ? balanceStateInfos1 : balanceStateInfos);
            BalanceStateInfo.MoveOptions moveOption = new BalanceStateInfo.MoveOptions(BalanceStateInfo.MoveOptions.MoveDistributions.Even);
            int processed = 0;
            int ssdCount = ssds.Count<BalanceStateInfo>();
            if (fileTypes == BalanceStateInfo.FileTypes.UnprotectedFiles)
            {
                ssdCount = 1;
            }

            foreach (BalanceStateInfo ssd in ssds)
            {
                processed = 0;
                while (true)
                {
                    int num2 = ssdCount * processed;
                    int num3 = ssdCount * (processed + 1);
                    moveOption.ExcludeStorageUnits = ssds.Union<BalanceStateInfo>(balanceStateInfos2.Take<BalanceStateInfo>(num2)).Union<BalanceStateInfo>(balanceStateInfos2.Skip<BalanceStateInfo>(num3));
                    moveOption.MaximumFillRatio = settings.ArchiveFillSettings.FillRatio;
                    IEnumerable<BalanceStateInfo> balanceStateInfos3 = ssds.Union<BalanceStateInfo>(archives).Except<BalanceStateInfo>(moveOption.ExcludeStorageUnits);
                    if (!balanceStateInfos3.Any<BalanceStateInfo>())
                    {
                        break;
                    }
                    if (balanceStateInfos3.Count<BalanceStateInfo>() == 1)
                    {
                        moveOption.MaximumFillRatio = Math.Max(moveOption.MaximumFillRatio, this.GetArchiveLimitRatio(settings, balanceStateInfos3.Single<BalanceStateInfo>()));
                    }
                    if (ssd.MoveFiles(fileTypes, 0, moveOption).MoveRatio == 1)
                    {
                        break;
                    }
                    processed++;
                }
            }
            if (settings.OrderedPlacementSettings.IsMoveExistingChecked)
            {
                if (fileTypes != BalanceStateInfo.FileTypes.UnprotectedFiles)
                {
                    if (settings.OrderedPlacementSettings.UnduplicatedOrderEnabled && settings.OrderedPlacementSettings.DuplicatedOrderEnabled && settings.OrderedPlacementSettings.DuplicatedOrderSameAsUnduplicated)
                    {
                        this.MoveOutExisting(settings, balanceStateInfos, BalanceStateInfo.FileTypes.ProtectedFiles);
                        return;
                    }
                    if (settings.OrderedPlacementSettings.DuplicatedOrderEnabled)
                    {
                        this.MoveOutExisting(settings, balanceStateInfos1, BalanceStateInfo.FileTypes.ProtectedFiles);
                    }
                }
                else if (settings.OrderedPlacementSettings.UnduplicatedOrderEnabled)
                {
                    this.MoveOutExisting(settings, balanceStateInfos, BalanceStateInfo.FileTypes.UnprotectedFiles);
                    return;
                }
            }
        }
        private void MoveOutOfSsdsUnordered(BalancerSettingsState settings, IEnumerable<BalanceStateInfo> ssds, IEnumerable<BalanceStateInfo> archives, BalanceStateInfo.FileTypes fileTypes)
        {
            BalanceStateInfo.MoveOptions moveOption = new BalanceStateInfo.MoveOptions(BalanceStateInfo.MoveOptions.MoveDistributions.Even)
            {
                MaximumFillRatio = settings.ArchiveFillSettings.FillRatio,
                ExcludeStorageUnits = ssds
            };
            

            foreach (BalanceStateInfo ssd in ssds)
            {
                if (ssd.MoveFiles(fileTypes, 0, moveOption).MoveRatio >= 1 || !settings.ArchiveFillSettings.IsFillBytesChecked || settings.ArchiveFillSettings.FillBytes <= (long)0)
                {
                    continue;
                }
                foreach (BalanceStateInfo archive in archives)
                {
                    bool allowDuplicated = settings.DuplicatedDrives[archive.Uid];
                    bool allowUnduplicated = settings.UnduplicatedDrives[archive.Uid];

                    if (fileTypes == BalanceStateInfo.FileTypes.ProtectedFiles && !allowDuplicated)
                    {
                        archive.SetLimit(BalanceStateInfo.LimitTypes.NoProtectedFiles, limitOptions);
                        archive.MoveFiles(BalanceStateInfo.FileTypes.ProtectedFiles, 0);
                        continue;
                    }
                    if (fileTypes == BalanceStateInfo.FileTypes.UnprotectedFiles && !allowUnduplicated)
                    {
                        archive.SetLimit(BalanceStateInfo.LimitTypes.NoUnprotectedFiles, limitOptions);
                        archive.MoveFiles(BalanceStateInfo.FileTypes.UnprotectedFiles, 0);
                        continue;
                    }

                    moveOption.MoveDistribution = BalanceStateInfo.MoveOptions.MoveDistributions.Specific;
                    moveOption.MaximumFillRatio = this.GetArchiveLimitRatio(settings, archive);
                    moveOption.SpecificStorageUnit = archive;
                    if (ssd.MoveFiles(fileTypes, 0, moveOption).MoveRatio == 1)
                    {
                        break;
                    }
                }
            }
        }
    }
}
