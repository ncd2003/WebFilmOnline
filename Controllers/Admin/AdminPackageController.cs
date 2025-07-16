using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering; // For SelectList
using System.Security.Claims;
using WebFilmOnline.Models;
using WebFilmOnline.Services.Interfaces;
using WebFilmOnline.Services.ViewModels;

namespace WebFilmOnline.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class AdminPackageController : Controller
    {
        private readonly IPackageService _packageService;
        private readonly IProductService _productService; // Để quản lý Product trong Package

        public AdminPackageController(IPackageService packageService, IProductService productService)
        {
            _packageService = packageService;
            _productService = productService;
        }

        // GET: Admin/AdminPackage
        public async Task<IActionResult> Index()
        {
            var packages = await _packageService.GetAllPackagesAsync();
            return View(packages);
        }

        // GET: Admin/AdminPackage/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminPackage/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PackageCreateUpdateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    int? createdByUserId = null;
                    if (int.TryParse(userId, out int id)) { createdByUserId = id; }

                    var package = new Package
                    {
                        Name = model.Name,
                        Description = model.Description,
                        Price = model.Price,
                        DurationDays = model.DurationDays,
                        IsActive = model.IsActive
                    };

                    await _packageService.AddPackageAsync(package, createdByUserId);
                    TempData["SuccessMessage"] = "Gói đã được thêm thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm gói.");
                }
            }
            return View(model);
        }

        // GET: Admin/AdminPackage/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var package = await _packageService.GetPackageByIdAsync(id);
            if (package == null) { return NotFound(); }

            var model = new PackageCreateUpdateViewModel
            {
                PackageId = package.PackageId,
                Name = package.Name,
                Description = package.Description,
                Price = package.Price,
                DurationDays = package.DurationDays,
                IsActive = package.IsActive
            };
            return View(model);
        }

        // POST: Admin/AdminPackage/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PackageCreateUpdateViewModel model)
        {
            if (id != model.PackageId) { return BadRequest(); }

            if (ModelState.IsValid)
            {
                try
                {
                    var package = await _packageService.GetPackageByIdAsync(id);
                    if (package == null) { return NotFound(); }

                    package.Name = model.Name;
                    package.Description = model.Description;
                    package.Price = model.Price;
                    package.DurationDays = model.DurationDays;
                    package.IsActive = model.IsActive;

                    await _packageService.UpdatePackageAsync(package);
                    TempData["SuccessMessage"] = "Gói đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex) { ModelState.AddModelError("", ex.Message); }
                catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); }
                catch (Exception) { ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật gói."); }
            }
            return View(model);
        }

        // GET: Admin/AdminPackage/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var package = await _packageService.GetPackageByIdAsync(id);
            if (package == null) { return NotFound(); }
            return View(package);
        }

        // POST: Admin/AdminPackage/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _packageService.DeletePackageAsync(id);
                TempData["SuccessMessage"] = "Gói đã được xóa thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa gói.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/AdminPackage/ManageProducts/5
        public async Task<IActionResult> ManageProducts(int packageId)
        {
            var package = await _packageService.GetPackageByIdAsync(packageId);
            if (package == null) { return NotFound(); }

            ViewBag.Package = package;
            ViewBag.ProductsInPackage = await _packageService.GetProductsInPackageAsync(packageId);

            // Lấy tất cả các phim để thêm vào gói (trừ những phim đã có trong gói)
            var allProducts = await _productService.GetAllProductsAsync();
            var currentProductIdsInPackage = (ViewBag.ProductsInPackage as IEnumerable<Product>)?.Select(p => p.ProductId).ToList() ?? new List<int>();
            ViewBag.AvailableProducts = new SelectList(
                allProducts.Where(p => !currentProductIdsInPackage.Contains(p.ProductId)),
                "ProductId", "Title"
            );

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductToPackage(int packageId, int productId)
        {
            try
            {
                await _packageService.AddProductToPackageAsync(packageId, productId);
                TempData["SuccessMessage"] = "Phim đã được thêm vào gói.";
            }
            catch (ArgumentException ex) { TempData["ErrorMessage"] = ex.Message; }
            catch (InvalidOperationException ex) { TempData["ErrorMessage"] = ex.Message; }
            catch (Exception) { TempData["ErrorMessage"] = "Đã xảy ra lỗi khi thêm phim vào gói."; }

            return RedirectToAction(nameof(ManageProducts), new { packageId = packageId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveProductFromPackage(int packageId, int productId)
        {
            try
            {
                await _packageService.RemoveProductFromPackageAsync(packageId, productId);
                TempData["SuccessMessage"] = "Phim đã được xóa khỏi gói.";
            }
            catch (Exception) { TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa phim khỏi gói."; }
            return RedirectToAction(nameof(ManageProducts), new { packageId = packageId });
        }
    }
}