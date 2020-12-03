using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using DrivePool.Integration.Info.Balancing;

namespace AllInOnePlugin
{
    /// <summary>
    /// This class is the "Controller" and it builds our balancing model.
    /// </summary>
    public partial class Balancer
        : DrivePool.Integration.Balancing.BalancerBase
    {
        /// <summary>
        /// Each balancer must have a unique Guid.
        /// </summary>
        public override Guid Id
        {
            get 
            {
                return new Guid("a2070e10-80f0-4eaf-b97b-56f4eab6a8f1");
            }
        }

        /// <summary>
        /// The name of the balancer as shown to the user.
        /// </summary>
        public override string Name
        {
            get 
            {
                return "All In One";
            }
        }

        /// <summary>
        /// A short description that will be shown to the user.
        /// </summary>
        public override string Description
        {
            get 
            {
                return "Combines several of the official plugins so they actually work together (as they should have in the first place).";
            }
        }

        /// <summary>
        /// Determine if any files need to be moved, and schedule one or more moves.
        /// 
        /// This is our balancing algorithm.
        /// </summary>
        /// 

        public static BalancerSettingsState settings;

        public override void Balance(IEnumerable<DrivePool.Integration.Info.Balancing.BalanceStateInfo> balanceStates)
        {
            settings = base.Setting as BalancerSettingsState ?? BalancerSettingsState.Default;
            
            // Make sure all drives are in dictionaries
            foreach (BalanceStateInfo balanceState in balanceStates)
            {
                if (!settings.UnduplicatedDrives.ContainsKey(balanceState.Uid))
                {
                    settings.UnduplicatedDrives[balanceState.Uid] = true;
                }
                if (!settings.DuplicatedDrives.ContainsKey(balanceState.Uid))
                {
                    settings.DuplicatedDrives[balanceState.Uid] = true;
                }
            }


            BalanceStateInfo.MoveOptions.MoveDistributions distribution = BalanceStateInfo.MoveOptions.MoveDistributions.Even;

            if (settings.FreePlacement)
            {
                distribution = BalanceStateInfo.MoveOptions.MoveDistributions.First;
            }

            IEnumerable<string> feederDrives =
                from kv in settings.FeederDrives
                where kv.Value
                select kv.Key;
            IEnumerable<BalanceStateInfo> feederDriveBalanceStateInfos  =
                from bs in balanceStates
                where feederDrives.Contains<string>(bs.Uid)
                select bs;
            List<BalanceStateInfo> archiveDriveBalanceStateInfos = balanceStates.Except<BalanceStateInfo>(feederDriveBalanceStateInfos).ToList();
            
            archiveDriveBalanceStateInfos.Sort((a, b) =>
            {
                return ((double)a.UsedSpace / (double)a.TotalSize).CompareTo(((double)b.UsedSpace / (double)b.TotalSize));
            });

            BalanceStateInfo.MoveOptions moveOption = new BalanceStateInfo.MoveOptions(distribution)
            {
                MaximumFillRatio = settings.ArchiveFillRatio,
                ExcludeStorageUnits = feederDriveBalanceStateInfos
            };

            BalanceStateInfo.LimitOptions limitOptions = new BalanceStateInfo.LimitOptions()
            {
                LimitType = BalanceStateInfo.LimitOptions.LimitTypes.SoftLimit,
                SoftLimitRatio = settings.FeederFillRatio
            };

            foreach (BalanceStateInfo balanceState in feederDriveBalanceStateInfos)
            {
                balanceState.MoveFiles(BalanceStateInfo.FileTypes.UnprotectedFiles, 0, moveOption);
                balanceState.MoveFiles(BalanceStateInfo.FileTypes.ProtectedFiles, 0, moveOption);
            }

            foreach (BalanceStateInfo balanceState in archiveDriveBalanceStateInfos)
            {
                balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoPooledFiles, limitOptions);
            }

            
            if (settings.EqualizeByFreeSpace)
            {
                this.EqualizeByFreeSpace(archiveDriveBalanceStateInfos, feederDriveBalanceStateInfos, limitOptions);
            }
            else if (settings.EqualizeByPercent)
            {
                this.EqualizeByPercent(archiveDriveBalanceStateInfos, feederDriveBalanceStateInfos, limitOptions);
            }
            else if (settings.FreePlacement)
            {

                foreach (BalanceStateInfo balanceState in archiveDriveBalanceStateInfos)
                {
                    bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                    bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];
                    
                    if (!allowUnduplicated)
                    {
                        balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoUnprotectedFiles, limitOptions);
                        balanceState.MoveFiles(BalanceStateInfo.FileTypes.UnprotectedFiles, 0, moveOption);
                    }
                    else
                    {
                        balanceState.MoveFiles(BalanceStateInfo.FileTypes.UnprotectedFiles, settings.ArchiveFillRatio, moveOption);
                    }

                    if (!allowDuplicated)
                    {
                        balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoProtectedFiles, limitOptions);
                        balanceState.MoveFiles(BalanceStateInfo.FileTypes.ProtectedFiles, 0, moveOption);
                    }
                    else
                    {
                        balanceState.MoveFiles(BalanceStateInfo.FileTypes.ProtectedFiles, settings.ArchiveFillRatio, moveOption);
                    }
                }

            }
        }


        private void EqualizeByPercent(IEnumerable<BalanceStateInfo> archiveBalanceStates, IEnumerable<BalanceStateInfo> feederBalanceStates, BalanceStateInfo.LimitOptions limitOptions)
        {
            List<BalanceStateInfo> usableDrives = new List<BalanceStateInfo>(archiveBalanceStates);
            List<BalanceStateInfo> feedDrives = new List<BalanceStateInfo>(feederBalanceStates);

            PoolHelper p = null;

            long amount = 0;
            long diff = 0;

            bool shouldContinue = true;
            
            while (shouldContinue)
            {
                shouldContinue = false;

                p = new PoolHelper(usableDrives, feedDrives);


                if (p.totalSize <= 0)
                {
                    return;
                }

                double totalSize = (double)p.totalSize;
                long totalUsedSpace = p.totalUsedSpace;

                double protectedRatio = (double)p.pools[PoolTypes.Protected].totalSize / totalSize;
                double unprotectedRatio = (double)p.pools[PoolTypes.Unprotected].totalSize / totalSize;
                double sharedRatio = (double)p.pools[PoolTypes.Shared].totalSize / totalSize;


                long protectedGoal1 = (long)(protectedRatio * totalUsedSpace) - p.pools[PoolTypes.Protected].totalUnpooledSpace;
                long unprotectedGoal1 = (long)(unprotectedRatio * totalUsedSpace) - p.pools[PoolTypes.Unprotected].totalUnpooledSpace;
                long sharedGoal1 = (long)(sharedRatio * totalUsedSpace) - p.pools[PoolTypes.Shared].totalUnpooledSpace;

                amount = p.pools[PoolTypes.Protected].SetSpaceTo(protectedGoal1);
                diff = protectedGoal1 - amount;

                if (diff != 0)
                {
                    protectedGoal1 = amount;

                    totalSize = (double)(p.pools[PoolTypes.Unprotected].totalSize + p.pools[PoolTypes.Shared].totalSize);
                    totalUsedSpace = p.pools[PoolTypes.Unprotected].totalUsedSpace + p.pools[PoolTypes.Shared].totalUsedSpace;

                    if (totalSize > 0)
                    {
                        unprotectedRatio = (double)p.pools[PoolTypes.Unprotected].totalSize / totalSize;
                        sharedRatio = (double)p.pools[PoolTypes.Shared].totalSize / totalSize;

                        //unprotectedGoal1 += (long)(diff * unprotectedRatio);

                        unprotectedGoal1 = (long)(unprotectedRatio * totalUsedSpace) - p.pools[PoolTypes.Unprotected].totalUnpooledSpace;
                        sharedGoal1 = (long)(sharedRatio * totalUsedSpace) - p.pools[PoolTypes.Shared].totalUnpooledSpace;
                    }
                    else
                    {
                        unprotectedGoal1 = 0;
                        sharedGoal1 = 0;
                    }
                }

                amount = p.pools[PoolTypes.Unprotected].SetSpaceTo(unprotectedGoal1);
                diff = unprotectedGoal1 - amount;


                if (diff != 0)
                {
                    totalSize = (double)(p.pools[PoolTypes.Protected].totalSize + p.pools[PoolTypes.Shared].totalSize);
                    totalUsedSpace = p.pools[PoolTypes.Protected].totalUsedSpace + p.pools[PoolTypes.Shared].totalUsedSpace;

                    if (totalSize > 0)
                    {
                        protectedRatio = protectedRatio = (double)p.pools[PoolTypes.Protected].totalSize / totalSize;
                        sharedRatio = (double)p.pools[PoolTypes.Shared].totalSize / totalSize;

                        //protectedGoal1 += (long)(diff * protectedRatio);

                        protectedGoal1 = protectedGoal1 = (long)(protectedRatio * totalUsedSpace) - p.pools[PoolTypes.Protected].totalUnpooledSpace;
                        sharedGoal1 = (long)(sharedRatio * totalUsedSpace) - p.pools[PoolTypes.Shared].totalUnpooledSpace;

                        amount = p.pools[PoolTypes.Protected].SetSpaceTo(protectedGoal1);
                    }
                    else
                    {
                        protectedGoal1 = 0;
                        sharedGoal1 = 0;
                    }
                }


                foreach (var pool in p.pools)
                {
                    if (pool.Value.totalSize <= 0)
                    {
                        continue;
                    }

                    double percent = (double)pool.Value.totalUsedSpace / (double)pool.Value.totalSize;
                    double protectedPercent = (double)(pool.Value.totalProtectedSpace + pool.Value.totalUnpooledSpace) / (double)pool.Value.totalSize;
                    double unprotectedPercent = (double)(pool.Value.totalUnprotectedSpace + pool.Value.totalUnpooledSpace) / (double)pool.Value.totalSize;

                    foreach (var balanceState in pool.Value.balanceStates)
                    {
                        if (balanceState.TotalSize <= 0)
                        {
                            continue;
                        }

                        bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                        bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];

                        double otherFileRatio = ((double)balanceState.UnpooledOtherFiles / (double)balanceState.TotalSize);

                        if (otherFileRatio > percent)
                        {
                            usableDrives.Remove(balanceState);
                            feedDrives.Add(balanceState);
                            shouldContinue = true;
                            break;
                        }
                    }

                    if (shouldContinue)
                    {
                        break;
                    }
                }
                
            }

            p.ApplyPercentageBalance(limitOptions);
        }



        private void EqualizeByFreeSpace(IEnumerable<BalanceStateInfo> archiveBalanceStates, IEnumerable<BalanceStateInfo> feederBalanceStates, BalanceStateInfo.LimitOptions limitOptions)
        {
            List<BalanceStateInfo> usableDrives = new List<BalanceStateInfo>(archiveBalanceStates);
            List<BalanceStateInfo> feedDrives = new List<BalanceStateInfo>(feederBalanceStates);

            PoolHelper p = null;

            long amount = 0;
            long diff = 0;

            bool shouldContinue = true;

            while (shouldContinue)
            {
                shouldContinue = false;

                p = new PoolHelper(usableDrives, feedDrives);


                if (p.totalDrives <= 0)
                {
                    return;
                }

                long totalFreeSpace = p.totalFreeSpace;
                long totalDrives = p.totalDrives;

                long avgFreeSpace = totalFreeSpace / totalDrives;

                long protectedGoal1 = p.pools[PoolTypes.Protected].totalSize - (long)(avgFreeSpace * p.pools[PoolTypes.Protected].totalDrives);
                long unprotectedGoal1 = p.pools[PoolTypes.Unprotected].totalSize - (long)(avgFreeSpace * p.pools[PoolTypes.Unprotected].totalDrives);
                long sharedGoal1 = p.pools[PoolTypes.Shared].totalSize - (long)(avgFreeSpace * p.pools[PoolTypes.Shared].totalDrives);


                amount = p.pools[PoolTypes.Protected].SetSpaceTo(protectedGoal1);
                diff = protectedGoal1 - amount;

                if (diff != 0)
                {
                    protectedGoal1 = amount;

                    totalFreeSpace = p.pools[PoolTypes.Unprotected].totalFreeSpace + p.pools[PoolTypes.Shared].totalFreeSpace;
                    totalDrives = p.pools[PoolTypes.Unprotected].totalDrives + p.pools[PoolTypes.Shared].totalDrives;

                    if (totalDrives > 0)
                    {
                        avgFreeSpace = totalFreeSpace / totalDrives;

                        unprotectedGoal1 = p.pools[PoolTypes.Unprotected].totalSize - (long)(avgFreeSpace * p.pools[PoolTypes.Unprotected].totalDrives);
                        sharedGoal1 = p.pools[PoolTypes.Shared].totalSize - (long)(avgFreeSpace * p.pools[PoolTypes.Shared].totalDrives);
                    }
                    else
                    {
                        unprotectedGoal1 = 0;
                        sharedGoal1 = 0;
                    }
                }

                amount = p.pools[PoolTypes.Unprotected].SetSpaceTo(unprotectedGoal1);
                diff = unprotectedGoal1 - amount;

                if (diff != 0)
                {
                    unprotectedGoal1 = amount;

                    totalFreeSpace = p.pools[PoolTypes.Protected].totalFreeSpace + p.pools[PoolTypes.Shared].totalFreeSpace;
                    totalDrives = p.pools[PoolTypes.Protected].totalDrives + p.pools[PoolTypes.Shared].totalDrives;

                    if (totalDrives > 0)
                    {
                        avgFreeSpace = totalFreeSpace / totalDrives;

                        protectedGoal1 = p.pools[PoolTypes.Protected].totalSize - (long)(avgFreeSpace * p.pools[PoolTypes.Protected].totalDrives);
                        sharedGoal1 = p.pools[PoolTypes.Shared].totalSize - (long)(avgFreeSpace * p.pools[PoolTypes.Shared].totalDrives);

                        amount = p.pools[PoolTypes.Protected].SetSpaceTo(protectedGoal1);
                    }
                    else
                    {
                        protectedGoal1 = 0;
                        sharedGoal1 = 0;
                    }
                }

                foreach (var pool in p.pools)
                {
                    if (pool.Value.totalDrives <= 0)
                    {
                        continue;
                    }
                    avgFreeSpace = pool.Value.totalFreeSpace / pool.Value.totalDrives;

                    foreach (var balanceState in pool.Value.balanceStates)
                    {
                        double allowableSpace = (long)balanceState.TotalSize - avgFreeSpace - (long)balanceState.UnpooledOtherFiles;

                        if (allowableSpace <= 0)
                        {
                            usableDrives.Remove(balanceState);
                            feedDrives.Add(balanceState);
                            shouldContinue = true;
                            break;
                        }
                    }

                    if (shouldContinue)
                    {
                        break;
                    }
                }

            }

            p.ApplyFreeSpaceBalance(limitOptions);
        }
        
    }
}
