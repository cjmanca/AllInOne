namespace AllInOnePlugin
{
    public partial class Balancer
    {
        public enum PoolTypes
        {
            Shared,
            Protected,
            Unprotected
        };
















        /*
        private void EqualizeByFreeSpace(IEnumerable<BalanceStateInfo> balanceStates, IEnumerable<BalanceStateInfo> feederStates, BalanceStateInfo.LimitOptions limitOptions)
        {
            List<BalanceStateInfo> balanceStateInfosDup = new List<BalanceStateInfo>(feederStates);
            List<BalanceStateInfo> balanceStateInfosUndup = new List<BalanceStateInfo>(feederStates);

            List<BalanceStateInfo> emptyCapped = new List<BalanceStateInfo>();


            long totalFreeSpace = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.FreeSpace);
            int totalDrives = balanceStates.Count<BalanceStateInfo>();
            long totalUsedSpaceUndup = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.UnprotectedPooledFiles) + (long)feederStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.UnprotectedPooledFiles);
            long totalUsedSpaceDup = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.ProtectedPooledFiles) + (long)feederStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.ProtectedPooledFiles);
            long totalUsedSpaceCombined = totalUsedSpaceUndup + totalUsedSpaceDup;

            long totalExclusiveFreeSpaceUndup = 0;
            long totalExclusiveFreeSpaceDup = 0;
            int totalExclusiveDrivesDup = 0;
            int totalExclusiveDrivesUndup = 0;

            // Find how much space is set aside exclusively for duplicated or unduplicated content
            foreach (BalanceStateInfo balanceState in balanceStates)
            {
                bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];

                long maxFreeSpace = (long)balanceState.TotalSize - (long)balanceState.UnpooledOtherFiles;
                
                if (allowDuplicated && !allowUnduplicated)
                {
                    totalExclusiveFreeSpaceDup += maxFreeSpace;
                    totalExclusiveDrivesDup++;
                }
                if (!allowDuplicated && allowUnduplicated)
                {
                    totalExclusiveFreeSpaceUndup += maxFreeSpace;
                    totalExclusiveDrivesUndup++;
                }
            }

            bool undupSpaceOnlyInGeneralPool = false;
            bool dupSpaceOnlyInGeneralPool = false;

            // If there's more exclusive space for a content type than exists of that type, we can't use that type for balancing the rest of the pool
            if (totalExclusiveFreeSpaceDup >= totalUsedSpaceDup)
            {
                undupSpaceOnlyInGeneralPool = true;
            }
            if (totalExclusiveFreeSpaceUndup >= totalUsedSpaceUndup)
            {
                dupSpaceOnlyInGeneralPool = true;
            }

            // Find drives which won't count towards pool free space averaging
            foreach (BalanceStateInfo balanceState in balanceStates)
            {
                bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];

                long maxFreeSpace = (long)balanceState.TotalSize - (long)balanceState.UnpooledOtherFiles;

                if ((!allowDuplicated && !allowUnduplicated) || (!allowDuplicated && dupSpaceOnlyInGeneralPool) || (!allowUnduplicated && undupSpaceOnlyInGeneralPool))
                {
                    totalFreeSpace -= maxFreeSpace;
                    totalDrives--;
                    emptyCapped.Add(balanceState);
                }
            }


            // Calculate average free space
            long avgFreeSpace = 0;

            if (totalDrives > 0)
                avgFreeSpace = totalFreeSpace / totalDrives;



            // Find drives which would be completely empty with this new average free space setting (their maximum available space is smaller than the average free space)
            // Exclude these drives from the averaging calculations, so the rest of the drives will be consistent
            bool foundCappedDrive = true;
            while (foundCappedDrive)
            {
                foundCappedDrive = false;

                foreach (BalanceStateInfo balanceState in balanceStates)
                {
                    bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                    bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];

                    long maxFreeSpace = (long)balanceState.TotalSize - (long)balanceState.UnpooledOtherFiles;


                    // TODO: Should probably do these calculations for exclusive space too
                    if (((allowDuplicated && !undupSpaceOnlyInGeneralPool) || (allowUnduplicated && !dupSpaceOnlyInGeneralPool)) && avgFreeSpace > maxFreeSpace && !emptyCapped.Contains(balanceState))
                    {
                        totalFreeSpace -= maxFreeSpace;
                        totalDrives--;

                        emptyCapped.Add(balanceState);
                        foundCappedDrive = true;
                    }

                    avgFreeSpace = 0;

                    if (totalDrives > 0)
                        avgFreeSpace = totalFreeSpace / totalDrives;


                }
            }



            // Calculate average free space for exclusive only situations
            long avgExclusiveFreeSpaceDup = avgFreeSpace;
            long avgExclusiveFreeSpaceUnup = avgFreeSpace;

            if (undupSpaceOnlyInGeneralPool && totalExclusiveDrivesDup > 0)
                avgExclusiveFreeSpaceDup = totalExclusiveFreeSpaceDup / totalExclusiveDrivesDup;

            if (dupSpaceOnlyInGeneralPool && totalExclusiveDrivesUndup > 0)
                avgExclusiveFreeSpaceUnup = totalExclusiveFreeSpaceUndup / totalExclusiveDrivesUndup;



            // Calculate the ratio of duplicated to unduplicated content in the pool
            double undupPercent = 0;
            double dupPercent = 0;

            if (totalUsedSpaceCombined > 0)
            {
                undupPercent = (double)totalUsedSpaceUndup / (double)totalUsedSpaceCombined;
                dupPercent = (double)totalUsedSpaceDup / (double)totalUsedSpaceCombined;
            }

            // In exclusive only situations, we only use the ratio for the exclusive only data type on the drives exclusive to that type.
            // The general pool will be the other (non-exclusive only) data type
            if (undupSpaceOnlyInGeneralPool && totalExclusiveFreeSpaceDup > 0)
            {
                dupPercent = (double)totalUsedSpaceDup / (double)totalExclusiveFreeSpaceDup;
            }
            if (dupSpaceOnlyInGeneralPool && totalExclusiveFreeSpaceUndup > 0)
            {
                undupPercent = (double)totalUsedSpaceUndup / (double)totalExclusiveFreeSpaceUndup;
            }



            foreach (BalanceStateInfo balanceState in balanceStates)
            {
                // Allowable space (should take up 100% of the used pooled data if added up on all drives)
                long allowableSpaceDup = (long)balanceState.TotalSize - (long)avgExclusiveFreeSpaceDup - (long)balanceState.UnpooledOtherFiles;
                long allowableSpaceUndup = (long)balanceState.TotalSize - (long)avgExclusiveFreeSpaceUnup - (long)balanceState.UnpooledOtherFiles;

                if (allowableSpaceDup <= 0 && allowableSpaceUndup <= 0)
                {
                    balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoUnprotectedFiles, limitOptions);
                    balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoProtectedFiles, limitOptions);
                    continue;
                }

                bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];



                // Find our local type ratios for this specific drive
                double spaceRatioDup = (double)allowableSpaceDup / (double)balanceState.TotalSize;
                double spaceRatioUndup = (double)allowableSpaceUndup / (double)balanceState.TotalSize;

                double undupSharePercent = spaceRatioUndup;
                double dupSharePercent = spaceRatioDup;

                if (allowUnduplicated && allowDuplicated)
                {
                    undupSharePercent = undupPercent * spaceRatioUndup;
                    dupSharePercent = dupPercent * spaceRatioDup;
                }



                // If unduplicated data is allowed on this drive, set up a balancer move
                if (allowUnduplicated && settings.EqualizeUnprotected && (!dupSpaceOnlyInGeneralPool || !allowDuplicated))
                {
                    balanceState.MoveFiles(BalanceStateInfo.FileTypes.UnprotectedFiles, undupSharePercent, new BalanceStateInfo.MoveOptions(BalanceStateInfo.MoveOptions.MoveDistributions.Even)
                    {
                        ExcludeStorageUnits = balanceStateInfosUndup
                    });
                    balanceStateInfosUndup.Add(balanceState);
                }
                if (!allowUnduplicated)
                {
                    balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoUnprotectedFiles, limitOptions);
                }

                // If duplicated data is allowed on this drive, set up a balancer move
                if (allowDuplicated && settings.EqualizeProtected && (!undupSpaceOnlyInGeneralPool || !allowUnduplicated))
                {
                    balanceState.MoveFiles(BalanceStateInfo.FileTypes.ProtectedFiles, dupSharePercent, new BalanceStateInfo.MoveOptions(BalanceStateInfo.MoveOptions.MoveDistributions.Even)
                    {
                        ExcludeStorageUnits = balanceStateInfosDup
                    });
                    balanceStateInfosDup.Add(balanceState);
                }
                if (!allowDuplicated)
                {
                    balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoProtectedFiles, limitOptions);
                }
            }
        }























        private void EqualizeByPercent(IEnumerable<BalanceStateInfo> balanceStates, IEnumerable<BalanceStateInfo> feederStates, BalanceStateInfo.LimitOptions limitOptions)
        {
            List<BalanceStateInfo> balanceStateInfosDup = new List<BalanceStateInfo>(feederStates);
            List<BalanceStateInfo> balanceStateInfosUndup = new List<BalanceStateInfo>(feederStates);

            List<BalanceStateInfo> emptyCapped = new List<BalanceStateInfo>();
            List<BalanceStateInfo> emptyCappedDup = new List<BalanceStateInfo>();
            List<BalanceStateInfo> emptyCappedUndup = new List<BalanceStateInfo>();


            long totalUsedSpaceOther = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.UnpooledOtherFiles);
            long totalSizeUndup = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.TotalSize) - (long)totalUsedSpaceOther;
            long totalSizeDup = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.TotalSize) - (long)totalUsedSpaceOther;
            long totalSize = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.TotalSize) - (long)totalUsedSpaceOther;
            long totalFreeSpace = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.FreeSpace);
            int totalDrives = balanceStates.Count<BalanceStateInfo>();
            long totalUsedSpaceUndup = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.UnprotectedPooledFiles) + (long)feederStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.UnprotectedPooledFiles);
            long totalUsedSpaceDup = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.ProtectedPooledFiles) + (long)feederStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.ProtectedPooledFiles);
            long totalUsedSpaceCombined = totalUsedSpaceUndup + totalUsedSpaceDup;

            long totalExclusiveFreeSpaceUndup = 0;
            long totalExclusiveFreeSpaceDup = 0;
            int totalExclusiveDrivesDup = 0;
            int totalExclusiveDrivesUndup = 0;

            // Find how much space is set aside exclusively for duplicated or unduplicated content
            foreach (BalanceStateInfo balanceState in balanceStates)
            {
                bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];

                long maxFreeSpace = (long)balanceState.TotalSize - (long)balanceState.UnpooledOtherFiles;

                if (allowDuplicated && !allowUnduplicated)
                {
                    totalExclusiveFreeSpaceDup += maxFreeSpace;
                    totalExclusiveDrivesDup++;
                }
                if (!allowDuplicated && allowUnduplicated)
                {
                    totalExclusiveFreeSpaceUndup += maxFreeSpace;
                    totalExclusiveDrivesUndup++;
                }
            }

            bool undupSpaceOnlyInGeneralPool = false;
            bool dupSpaceOnlyInGeneralPool = false;

            // If there's more exclusive space for a content type than exists of that type, we can't use that type for balancing the rest of the pool
            if (totalExclusiveFreeSpaceDup >= totalUsedSpaceDup)
            {
                undupSpaceOnlyInGeneralPool = true;
            }
            if (totalExclusiveFreeSpaceUndup >= totalUsedSpaceUndup)
            {
                dupSpaceOnlyInGeneralPool = true;
            }


            // Find drives which won't count towards pool free space averaging
            foreach (BalanceStateInfo balanceState in balanceStates)
            {
                bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];

                long maxFreeSpace = (long)balanceState.TotalSize - (long)balanceState.UnpooledOtherFiles;

                if ((!allowDuplicated && !allowUnduplicated) || (!allowDuplicated && dupSpaceOnlyInGeneralPool) || (!allowUnduplicated && undupSpaceOnlyInGeneralPool))
                {
                    totalFreeSpace -= maxFreeSpace;
                    totalDrives--;
                    emptyCapped.Add(balanceState);
                }


                if (!allowDuplicated || (allowUnduplicated && undupSpaceOnlyInGeneralPool))
                {
                    totalSizeDup -= maxFreeSpace;
                    emptyCappedDup.Add(balanceState);
                }
                if (!allowUnduplicated || (allowDuplicated && dupSpaceOnlyInGeneralPool))
                {
                    totalSizeUndup -= maxFreeSpace;
                    emptyCappedUndup.Add(balanceState);
                }


            }


            // Calculate average free space
            long avgFreeSpace = 0;

            if (totalDrives > 0)
                avgFreeSpace = totalFreeSpace / totalDrives;

            double percentTotalUsedSpace = (double)totalUsedSpaceCombined / (double)(totalSize);
            double percentTotalUsedSpaceDup = (double)totalUsedSpaceDup / (double)(totalSizeDup - totalUsedSpaceUndup);
            double percentTotalUsedSpaceUndup = (double)totalUsedSpaceUndup / (double)(totalSizeUndup - totalUsedSpaceDup);


            // Find drives which would be completely empty with this new percent setting (the non-pooled files take up more space alone than this percentage)
            // Exclude these drives from the averaging calculations, so the rest of the drives will be consistent
            bool foundCappedDrive = true;
            while (foundCappedDrive)
            {
                foundCappedDrive = false;

                foreach (BalanceStateInfo balanceState in balanceStates)
                {
                    bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                    bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];

                    long maxFreeSpace = (long)balanceState.TotalSize - (long)balanceState.UnpooledOtherFiles;


                    // TODO: Should probably do these calculations for exclusive space too
                    if (((allowDuplicated && !undupSpaceOnlyInGeneralPool) || (allowUnduplicated && !dupSpaceOnlyInGeneralPool)) 
                        && avgFreeSpace > maxFreeSpace && !emptyCapped.Contains(balanceState))
                    {
                        totalFreeSpace -= maxFreeSpace;
                        totalDrives--;

                        emptyCapped.Add(balanceState);
                        foundCappedDrive = true;
                    }

                    if (!allowDuplicated)
                    {
                        totalSizeDup -= maxFreeSpace;
                        emptyCappedDup.Add(balanceState);
                    }
                    if (!allowUnduplicated)
                    {
                        totalSizeDup -= maxFreeSpace;
                        emptyCappedUndup.Add(balanceState);
                    }


                    double unpooledSpacePercentage = (double)balanceState.UnpooledOtherFiles / (double)balanceState.TotalSize;
                    
                    if ((allowDuplicated && (!allowUnduplicated || !undupSpaceOnlyInGeneralPool)) && unpooledSpacePercentage > percentTotalUsedSpaceDup && !emptyCappedDup.Contains(balanceState))
                    {
                        totalSizeDup -= maxFreeSpace;
                        emptyCappedDup.Add(balanceState);
                        foundCappedDrive = true;
                    }

                    if (allowUnduplicated && unpooledSpacePercentage > percentTotalUsedSpaceUndup && !emptyCappedUndup.Contains(balanceState))
                    {
                        totalSizeUndup -= maxFreeSpace;
                        emptyCappedUndup.Add(balanceState);
                        foundCappedDrive = true;
                    }





                    avgFreeSpace = 0;

                    if (totalDrives > 0)
                        avgFreeSpace = totalFreeSpace / totalDrives;


                }
            }



            // Calculate average free space for exclusive only situations
            long avgExclusiveFreeSpaceDup = avgFreeSpace;
            long avgExclusiveFreeSpaceUnup = avgFreeSpace;

            if (undupSpaceOnlyInGeneralPool && totalExclusiveDrivesDup > 0)
                avgExclusiveFreeSpaceDup = totalExclusiveFreeSpaceDup / totalExclusiveDrivesDup;

            if (dupSpaceOnlyInGeneralPool && totalExclusiveDrivesUndup > 0)
                avgExclusiveFreeSpaceUnup = totalExclusiveFreeSpaceUndup / totalExclusiveDrivesUndup;



            // Calculate the ratio of duplicated to unduplicated content in the pool
            double undupPercent = 0;
            double dupPercent = 0;

            if (totalUsedSpaceCombined > 0)
            {
                undupPercent = (double)totalUsedSpaceUndup / (double)totalUsedSpaceCombined;
                dupPercent = (double)totalUsedSpaceDup / (double)totalUsedSpaceCombined;
            }

            // In exclusive only situations, we only use the ratio for the exclusive only data type on the drives exclusive to that type.
            // The general pool will be the other (non-exclusive only) data type
            if (undupSpaceOnlyInGeneralPool && totalExclusiveFreeSpaceDup > 0)
            {
                dupPercent = (double)totalUsedSpaceDup / (double)totalExclusiveFreeSpaceDup;
            }
            if (dupSpaceOnlyInGeneralPool && totalExclusiveFreeSpaceUndup > 0)
            {
                undupPercent = (double)totalUsedSpaceUndup / (double)totalExclusiveFreeSpaceUndup;
            }



            foreach (BalanceStateInfo balanceState in balanceStates)
            {
                // Allowable space (should take up 100% of the used pooled data if added up on all drives)
                long allowableSpaceDup = (long)balanceState.TotalSize - (long)avgExclusiveFreeSpaceDup - (long)balanceState.UnpooledOtherFiles;
                long allowableSpaceUndup = (long)balanceState.TotalSize - (long)avgExclusiveFreeSpaceUnup - (long)balanceState.UnpooledOtherFiles;

                if (allowableSpaceDup <= 0 && allowableSpaceUndup <= 0)
                {
                    balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoUnprotectedFiles, limitOptions);
                    balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoProtectedFiles, limitOptions);
                    continue;
                }

                bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];



                // Find our local type ratios for this specific drive
                double spaceRatioDup = (double)allowableSpaceDup / (double)balanceState.TotalSize;
                double spaceRatioUndup = (double)allowableSpaceUndup / (double)balanceState.TotalSize;

                double undupSharePercent = spaceRatioUndup;
                double dupSharePercent = spaceRatioDup;

                if (allowUnduplicated && allowDuplicated)
                {
                    undupSharePercent = undupPercent * spaceRatioUndup;
                    dupSharePercent = dupPercent * spaceRatioDup;
                }



                // If unduplicated data is allowed on this drive, set up a balancer move
                if (allowUnduplicated && settings.EqualizeUnprotected && (!dupSpaceOnlyInGeneralPool || !allowDuplicated))
                {
                    balanceState.MoveFiles(BalanceStateInfo.FileTypes.UnprotectedFiles, undupSharePercent, new BalanceStateInfo.MoveOptions(BalanceStateInfo.MoveOptions.MoveDistributions.Even)
                    {
                        ExcludeStorageUnits = balanceStateInfosUndup
                    });
                    balanceStateInfosUndup.Add(balanceState);
                }
                if (!allowUnduplicated)
                {
                    balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoUnprotectedFiles, limitOptions);
                }

                // If duplicated data is allowed on this drive, set up a balancer move
                if (allowDuplicated && settings.EqualizeProtected && (!undupSpaceOnlyInGeneralPool || !allowUnduplicated))
                {
                    balanceState.MoveFiles(BalanceStateInfo.FileTypes.ProtectedFiles, dupSharePercent, new BalanceStateInfo.MoveOptions(BalanceStateInfo.MoveOptions.MoveDistributions.Even)
                    {
                        ExcludeStorageUnits = balanceStateInfosDup
                    });
                    balanceStateInfosDup.Add(balanceState);
                }
                if (!allowDuplicated)
                {
                    balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoProtectedFiles, limitOptions);
                }
            }
        }






























        private void EqualizeByPercentOld(IEnumerable<BalanceStateInfo> balanceStates, IEnumerable<BalanceStateInfo> feederStates, BalanceStateInfo.LimitOptions limitOptions)
        {
            List<BalanceStateInfo> balanceStateInfosDup = new List<BalanceStateInfo>(feederStates);
            List<BalanceStateInfo> balanceStateInfosUndup = new List<BalanceStateInfo>(feederStates);

            long totalUsedSpaceOther = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.UnpooledOtherFiles);
            long totalUsedSpaceUndup = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.UnprotectedPooledFiles) + (long)feederStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.UnprotectedPooledFiles);
            long totalSizeUndup = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.TotalSize) - (long)totalUsedSpaceOther;
            long totalUsedSpaceDup = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.ProtectedPooledFiles) + (long)feederStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.ProtectedPooledFiles);
            long totalSizeDup = (long)balanceStates.Sum<BalanceStateInfo>((BalanceStateInfo bs) => bs.TotalSize) - (long)totalUsedSpaceOther;

            foreach (BalanceStateInfo balanceState in balanceStates)
            {
                bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];

                long maxFreeSpace = (long)balanceState.TotalSize - (long)balanceState.UnpooledOtherFiles;

                if (!allowUnduplicated)
                {
                    totalSizeUndup -= maxFreeSpace;
                }

                if (!allowDuplicated)
                {
                    totalSizeDup -= maxFreeSpace;
                }
            }
            

            double percentTotalUsedSpaceDup = (double)totalUsedSpaceDup / (double)(totalSizeDup - totalUsedSpaceUndup);
            double percentTotalUsedSpaceUndup = (double)totalUsedSpaceUndup / (double)(totalSizeUndup - totalUsedSpaceDup);


            

            foreach (BalanceStateInfo balanceState in balanceStates)
            {
                bool allowUnduplicated = settings.UnduplicatedDrives[balanceState.Uid];
                bool allowDuplicated = settings.DuplicatedDrives[balanceState.Uid];

                double otherFileRatio = ((double)balanceState.UnpooledOtherFiles / (double)balanceState.TotalSize);
                
                if (allowUnduplicated && settings.EqualizeUnprotected)
                {
                    balanceState.MoveFiles(BalanceStateInfo.FileTypes.UnprotectedFiles, Math.Max(0, percentTotalUsedSpaceUndup - otherFileRatio), new BalanceStateInfo.MoveOptions(BalanceStateInfo.MoveOptions.MoveDistributions.Even)
                    {
                        ExcludeStorageUnits = balanceStateInfosUndup
                    });
                    balanceStateInfosUndup.Add(balanceState);
                }
                if (!allowUnduplicated)
                {
                    balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoUnprotectedFiles, limitOptions);
                }
                
                if (allowDuplicated && settings.EqualizeProtected)
                {
                    balanceState.MoveFiles(BalanceStateInfo.FileTypes.ProtectedFiles, Math.Max(0, percentTotalUsedSpaceDup - otherFileRatio), new BalanceStateInfo.MoveOptions(BalanceStateInfo.MoveOptions.MoveDistributions.Even)
                    {
                        ExcludeStorageUnits = balanceStateInfosDup
                    });
                    balanceStateInfosDup.Add(balanceState);
                }
                if (!allowDuplicated)
                {
                    balanceState.SetLimit(BalanceStateInfo.LimitTypes.NoProtectedFiles, limitOptions);
                }
            }
        }
        */
    }
}
