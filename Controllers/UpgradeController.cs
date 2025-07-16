using Microsoft.AspNetCore.Mvc;
using WebFilmOnline.Models;
using WebFilmOnline.Services;

public class UpgradeController : Controller
{
    private readonly IUpgradeService _upgradeService;

    public UpgradeController(IUpgradeService upgradeService)
    {
        _upgradeService = upgradeService;
    }

    // Bước 1: Chọn gói muốn nâng cấp
    public IActionResult Select()
    {
        var packages = _upgradeService.GetAvailablePackages();
        return View(packages);
    }

    // Bước 2: Xác nhận nâng cấp
    public IActionResult Confirm(int packageId)
    {
        var summary = _upgradeService.CalculateUpgrade(packageId, currentUserId: 1); // giả định UserId = 1
        return View(summary);
    }

    // Bước 3: Gửi thanh toán
    [HttpPost]
    public IActionResult ConfirmUpgrade(int packageId)
    {
        var result = _upgradeService.ProcessUpgrade(packageId, currentUserId: 1);
        return RedirectToAction("Result", new { success = result });
    }

    // Bước 4: Hiển thị kết quả
    public IActionResult Result(bool success)
    {
        ViewBag.Success = success;
        return View();
    }
}
