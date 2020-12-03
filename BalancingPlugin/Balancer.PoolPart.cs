using System;
using System.Collections.Generic;
using System.Linq;
using DrivePool.Integration.Extensions;
using DrivePool.Integration.Info.Balancing;

namespace AllInOnePlugin
{
    public partial class Balancer
    {
        public class PoolPart
        {
            public PoolPart(PoolTypes pType, PoolHelper pr)
            {
                this.type = pType;
                pools = pr;

                balanceStates = new List<BalanceStateInfo>();
                
                totalUnpooledSpace = 0;
                totalProtectedSpace = 0;
                totalUnprotectedSpace = 0;
                
                totalSize = 0;


                if (type == PoolTypes.Protected)
                {
                    totalProtectedSpace = (long)pools.balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.ProtectedPooledFiles);
                    if (pools.moveSpaceFrom != null)
                    {
                        totalProtectedSpace += (long)pools.moveSpaceFrom.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.ProtectedPooledFiles);
                    }
                }
                if (type == PoolTypes.Unprotected)
                {
                    totalUnprotectedSpace = (long)pools.balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.UnprotectedPooledFiles);
                    if (pools.moveSpaceFrom != null)
                    {
                        totalUnprotectedSpace += (long)pools.moveSpaceFrom.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.UnprotectedPooledFiles);
                    }
                }

                // Grab the right data for this Pool Type
                foreach (BalanceStateInfo balanceState in pools.balanceStates)
                {
                    bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                    bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];

                    long maxAvailableSpace = (long)balanceState.TotalSize - (long)balanceState.UnpooledOtherFiles;

                    if (allowDuplicated && !allowUnduplicated && type == PoolTypes.Protected)
                    {
                        balanceStates.Add(balanceState);
                    }
                    if (!allowDuplicated && allowUnduplicated && type == PoolTypes.Unprotected)
                    {
                        balanceStates.Add(balanceState);
                    }
                    if (allowDuplicated && allowUnduplicated && type == PoolTypes.Shared)
                    {
                        balanceStates.Add(balanceState);
                    }
                }
                
                totalUnpooledSpace = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.UnpooledOtherFiles);

                totalSize = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.TotalSize);
            }


            public long SetSpaceTo(long amount, PoolTypes? origin = null)
            {
                if (!origin.HasValue)
                {
                    if (type == PoolTypes.Shared)
                    {
                        throw new NotImplementedException();
                    }
                    origin = PoolTypes.Shared;
                }
                long diff = amount - totalPooledSpace;

                MoveSpaceFrom(origin.Value, diff);

                return totalPooledSpace;
            }

            public long MoveSpaceFrom(PoolTypes origin, long amount)
            {
                return MoveSpaceTo(origin, amount * -1) * -1;
            }
            public long MoveSpaceTo(PoolTypes destination, long amount)
            {
                bool isProtected = false;

                if (type == destination)
                {
                    // no point in moving data to ourself
                    return 0;
                }
                if (type != PoolTypes.Shared && destination != PoolTypes.Shared)
                {
                    if (amount > 0)
                    {
                        amount = MoveSpaceTo(PoolTypes.Shared, amount);
                        long newAmount = pools.pools[PoolTypes.Shared].MoveSpaceTo(destination, amount);

                        if (newAmount != amount)
                        {
                            MoveSpaceFrom(PoolTypes.Shared, amount - newAmount);
                        }
                        amount = newAmount;
                    }
                    else
                    {
                        amount *= -1;
                        amount = pools.pools[PoolTypes.Shared].MoveSpaceFrom(destination, amount); 
                        long newAmount = MoveSpaceFrom(PoolTypes.Shared, amount);

                        if (newAmount != amount)
                        {
                            pools.pools[PoolTypes.Shared].MoveSpaceTo(destination, amount - newAmount);
                        }

                        amount = newAmount * -1;
                    }

                    return amount;
                }

                if (type == PoolTypes.Protected || destination == PoolTypes.Protected)
                {
                    isProtected = true;
                }

                if (isProtected)
                {
                    if (amount > totalProtectedSpace)
                    {
                        amount = totalProtectedSpace;
                    }

                    if (amount < 0 && Math.Abs(amount) > pools.pools[destination].totalProtectedSpace)
                    {
                        amount = pools.pools[destination].totalProtectedSpace * -1;
                    }
                }
                else
                {
                    if (amount > totalUnprotectedSpace)
                    {
                        amount = totalUnprotectedSpace;
                    }

                    if (amount < 0 && Math.Abs(amount) > pools.pools[destination].totalUnprotectedSpace)
                    {
                        amount = pools.pools[destination].totalUnprotectedSpace * -1;
                    }
                }
                
                if (amount < 0)
                {
                    if (Math.Abs(amount) + totalPooledSpace > totalPooledSize)
                    {
                        amount = (totalPooledSize - totalPooledSpace) * -1;
                    }
                }
                else if (amount + pools.pools[destination].totalPooledSpace > pools.pools[destination].totalPooledSize)
                {
                    amount = pools.pools[destination].totalPooledSize - pools.pools[destination].totalPooledSpace;
                }
                
                if (isProtected)
                {
                    totalProtectedSpace -= amount;
                    pools.pools[destination].totalProtectedSpace += amount;
                }
                else
                {
                    totalUnprotectedSpace -= amount;
                    pools.pools[destination].totalUnprotectedSpace += amount;
                }

                return amount;
            }


            

            public void ApplyPercentageBalance(BalanceStateInfo.LimitOptions limitOptions, List<BalanceStateInfo> balanceStateInfosDup, List<BalanceStateInfo> balanceStateInfosUndup)
            {
                if (totalDrives <= 0 || totalSize <= 0)
                {
                    return;
                }

                double protectedPercent = (double)(totalProtectedSpace + totalUnpooledSpace) / (double)totalSize;
                double unprotectedPercent = (double)(totalUnprotectedSpace + totalUnpooledSpace) / (double)totalSize;
                
                foreach (BalanceStateInfo balanceState in balanceStates)
                {
                    if (balanceState.TotalSize <= 0)
                    {
                        continue;
                    }

                    bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                    bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];

                    double otherFileRatio = ((double)balanceState.UnpooledOtherFiles / (double)balanceState.TotalSize);

                    if (allowDuplicated && settings.EqualizeProtected)
                    {
                        balanceState.MoveFiles(BalanceStateInfo.FileTypes.ProtectedFiles, protectedPercent - otherFileRatio, new BalanceStateInfo.MoveOptions(BalanceStateInfo.MoveOptions.MoveDistributions.Even)
                        {
                            ExcludeStorageUnits = balanceStateInfosDup
                        });
                        balanceStateInfosDup.Add(balanceState);
                    }

                    if (allowUnduplicated && settings.EqualizeUnprotected)
                    {
                        balanceState.MoveFiles(BalanceStateInfo.FileTypes.UnprotectedFiles, unprotectedPercent - otherFileRatio, new BalanceStateInfo.MoveOptions(BalanceStateInfo.MoveOptions.MoveDistributions.Even)
                        {
                            ExcludeStorageUnits = balanceStateInfosUndup
                        });
                        balanceStateInfosUndup.Add(balanceState);
                    }
                }
            }

            public void ApplyFreeSpaceBalance(BalanceStateInfo.LimitOptions limitOptions, List<BalanceStateInfo> balanceStateInfosDup, List<BalanceStateInfo> balanceStateInfosUndup)
            {
                if (totalDrives <= 0 || totalPooledSpace <= 0)
                {
                    return;
                }

                long avgFreeSpace = totalFreeSpace / totalDrives;

                // Calculate the ratio of duplicated to unduplicated content in the pool
                double protectedPercent = (double)totalProtectedSpace / (double)totalPooledSpace;
                double unprotectedPercent = (double)totalUnprotectedSpace / (double)totalPooledSpace;
                
                foreach (BalanceStateInfo balanceState in balanceStates)
                {
                    if (balanceState.TotalSize <= 0)
                    {
                        continue;
                    }

                    double allowableSpace = (long)balanceState.TotalSize - avgFreeSpace - (long)balanceState.UnpooledOtherFiles;

                    if (allowableSpace <= 0)
                    {
                        balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoUnprotectedFiles, limitOptions);
                        balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoProtectedFiles, limitOptions);
                        continue;
                    }
                    // Find our local type ratio for this specific drive
                    double spaceRatio = (double)allowableSpace / (double)balanceState.TotalSize;


                    bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                    bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];

                    if (allowDuplicated && settings.EqualizeProtected)
                    {
                        balanceState.MoveFiles(BalanceStateInfo.FileTypes.ProtectedFiles, protectedPercent * spaceRatio, new BalanceStateInfo.MoveOptions(BalanceStateInfo.MoveOptions.MoveDistributions.Even)
                        {
                            ExcludeStorageUnits = balanceStateInfosDup
                        });
                        balanceStateInfosDup.Add(balanceState);
                    }

                    if (allowUnduplicated && settings.EqualizeUnprotected)
                    {
                        balanceState.MoveFiles(BalanceStateInfo.FileTypes.UnprotectedFiles, unprotectedPercent * spaceRatio, new BalanceStateInfo.MoveOptions(BalanceStateInfo.MoveOptions.MoveDistributions.Even)
                        {
                            ExcludeStorageUnits = balanceStateInfosUndup
                        });
                        balanceStateInfosUndup.Add(balanceState);
                    }
                }
            }


            public long totalPooledSpace
            {
                get
                {
                    return totalProtectedSpace + totalUnprotectedSpace;
                }
            }
            public long totalPooledSize
            {
                get
                {
                    return totalSize - totalUnpooledSpace;
                }
            }
            public long totalUsedSpace
            {
                get
                {
                    return totalProtectedSpace + totalUnprotectedSpace + totalUnpooledSpace;
                }
            }
            public long totalFreeSpace
            {
                get
                {
                    return totalSize - totalUsedSpace;
                }
            }
            public long totalFillableSpace
            {
                get
                {
                    return totalSize - totalUnpooledSpace;
                }
            }
            public long totalDrives
            {
                get
                {
                    return balanceStates.Count;
                }
            }

            public PoolTypes type { get; protected set; }

            public PoolHelper pools { get; protected set; }


            public List<BalanceStateInfo> balanceStates { get; set; }


            
            public long totalUnpooledSpace { get; protected set; }

            public long totalProtectedSpace { get; protected set; }
            public long totalUnprotectedSpace { get; protected set; }

            public long totalSize { get; protected set; }
        }
        
    }
}
