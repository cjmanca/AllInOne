using System.Collections.Generic;
using DrivePool.Integration.Info.Balancing;

namespace AllInOnePlugin
{
    public partial class Balancer
    {
        public class PoolHelper
        {
            public PoolHelper(IEnumerable<BalanceStateInfo> pBalanceStates, IEnumerable<BalanceStateInfo> pMoveSpaceFrom = null)
            {
                balanceStates = pBalanceStates;
                moveSpaceFrom = pMoveSpaceFrom;
                
                pools = new Dictionary<PoolTypes, PoolPart>();
                pools[PoolTypes.Protected] = new PoolPart(PoolTypes.Protected, this);
                pools[PoolTypes.Unprotected] = new PoolPart(PoolTypes.Unprotected, this);
                pools[PoolTypes.Shared] = new PoolPart(PoolTypes.Shared, this);
                
                ResetBalance();
                
            }

            public void ResetBalance()
            {
                pools[PoolTypes.Shared].MoveSpaceFrom(PoolTypes.Protected, totalProtectedSpace);
                pools[PoolTypes.Shared].MoveSpaceFrom(PoolTypes.Unprotected, totalUnprotectedSpace);
                pools[PoolTypes.Shared].MoveSpaceTo(PoolTypes.Protected, totalProtectedSpace);
                pools[PoolTypes.Shared].MoveSpaceTo(PoolTypes.Unprotected, totalUnprotectedSpace);
            }
            

            public void ApplyPercentageBalance(BalanceStateInfo.LimitOptions limitOptions)
            {
                List<BalanceStateInfo> balanceStateInfosDup = new List<BalanceStateInfo>();
                List<BalanceStateInfo> balanceStateInfosUndup = new List<BalanceStateInfo>();

                foreach (var pool in pools)
                {
                    pool.Value.ApplyPercentageBalance(limitOptions, balanceStateInfosDup, balanceStateInfosUndup);
                }

                foreach (BalanceStateInfo balanceState in balanceStates)
                {
                    bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                    bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];
                    
                    if (!allowDuplicated)
                    {
                        balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoProtectedFiles, limitOptions);
                    }
                    if (!allowUnduplicated)
                    {
                        balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoUnprotectedFiles, limitOptions);
                    }
                }
            }

            public void ApplyFreeSpaceBalance(BalanceStateInfo.LimitOptions limitOptions)
            {
                List<BalanceStateInfo> balanceStateInfosDup = new List<BalanceStateInfo>();
                List<BalanceStateInfo> balanceStateInfosUndup = new List<BalanceStateInfo>();

                foreach (var pool in pools)
                {
                    pool.Value.ApplyFreeSpaceBalance(limitOptions, balanceStateInfosDup, balanceStateInfosUndup);
                }

                foreach (BalanceStateInfo balanceState in balanceStates)
                {
                    bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                    bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];

                    if (!allowDuplicated)
                    {
                        balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoProtectedFiles, limitOptions);
                    }
                    if (!allowUnduplicated)
                    {
                        balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoUnprotectedFiles, limitOptions);
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
                    return pools[PoolTypes.Protected].totalFillableSpace + pools[PoolTypes.Unprotected].totalFillableSpace + pools[PoolTypes.Shared].totalFillableSpace;
                }
            }
            public long totalDrives
            {
                get
                {
                    return pools[PoolTypes.Protected].totalDrives + pools[PoolTypes.Unprotected].totalDrives + pools[PoolTypes.Shared].totalDrives;
                }
            }
            public long totalUnpooledSpace
            {
                get
                {
                    return pools[PoolTypes.Protected].totalUnpooledSpace + pools[PoolTypes.Unprotected].totalUnpooledSpace + pools[PoolTypes.Shared].totalUnpooledSpace;
                }
            }
            public long totalProtectedSpace
            {
                get
                {
                    return pools[PoolTypes.Protected].totalProtectedSpace + pools[PoolTypes.Unprotected].totalProtectedSpace + pools[PoolTypes.Shared].totalProtectedSpace;
                }
            }
            public long totalUnprotectedSpace
            {
                get
                {
                    return pools[PoolTypes.Unprotected].totalUnprotectedSpace + pools[PoolTypes.Unprotected].totalUnprotectedSpace + pools[PoolTypes.Shared].totalUnprotectedSpace;
                }
            }
            public long totalSize
            {
                get
                {
                    return pools[PoolTypes.Protected].totalSize + pools[PoolTypes.Unprotected].totalSize + pools[PoolTypes.Shared].totalSize;
                }
            }

            public Dictionary<PoolTypes, PoolPart> pools { get; protected set; }

            public IEnumerable<BalanceStateInfo> balanceStates { get; protected set; }
            public IEnumerable<BalanceStateInfo> moveSpaceFrom { get; protected set; }
        }
        
    }
}
