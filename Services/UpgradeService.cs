using WebFilmOnline.Models;
using WebFilmOnline.ViewModels;

public class UpgradeService : IUpgradeService
{
    private readonly FilmServiceDbContext _context;

    public UpgradeService(FilmServiceDbContext context)
    {
        _context = context;
    }

    public List<Package> GetAvailablePackages()
    {
        return _context.Packages.Where(p => p.Status == "Active").ToList();
    }

    public UpgradeSummaryViewModel CalculateUpgrade(int newPackageId, int currentUserId)
    {
        var currentSub = _context.UserSubscriptions.FirstOrDefault(u => u.UserId == currentUserId && u.IsActive);
        var newPackage = _context.Packages.Find(newPackageId);

        // Tính giá trị còn lại (giả lập)
        double remainingValue = 0;
        if (currentSub != null)
        {
            var remainingDays = (currentSub.EndDate - DateTime.Now).TotalDays;
            remainingValue = (currentSub.Price / currentSub.DurationDays) * remainingDays;
        }

        // Tính tổng giá sau khi trừ
        double finalCost = newPackage.Price - remainingValue;
        if (finalCost < 0) finalCost = 0;

        return new UpgradeSummaryViewModel
        {
            CurrentPackageName = currentSub?.Package?.Name,
            NewPackageName = newPackage.Name,
            RemainingValue = remainingValue,
            FinalCost = finalCost
        };
    }

    public bool ProcessUpgrade(int newPackageId, int currentUserId)
    {
        // Xử lý thanh toán (giả lập là luôn thành công)
        var sub = _context.UserSubscriptions.FirstOrDefault(u => u.UserId == currentUserId && u.IsActive);
        if (sub != null)
        {
