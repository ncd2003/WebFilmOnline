using WebFilmOnline.ViewModels;
using WebFilmOnline.Models;
using System.Collections.Generic;

public interface IUpgradeService
{
    List<Package> GetAvailablePackages();
    UpgradeSummaryViewModel CalculateUpgrade(int newPackageId, int currentUserId);
    bool ProcessUpgrade(int newPackageId, int currentUserId);
}
