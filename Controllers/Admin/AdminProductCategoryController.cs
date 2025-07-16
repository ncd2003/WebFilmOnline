using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebFilmOnline.Models;
using WebFilmOnline.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace WebFilmOnline.Controllers.Admin
{
    [Authorize(Roles = "Admin")] // Yêu cầu quyền Admin
    [Area("Admin")] // Nếu bạn đã cấu hình Area cho Admin
    public class AdminProductCategoryController : Controller
    {
        private readonly IProductCategoryService _productCategoryService;

        public AdminProductCategoryController(IProductCategoryService productCategoryService)
        {
            _productCategoryService = productCategoryService;
        }

        // GET: Admin/AdminProductCategory
        public async Task<IActionResult> Index()
        {
            var categories = await _productCategoryService.GetAllCategoriesAsync();
            return View(categories);
        }

        // GET: Admin/AdminProductCategory/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminProductCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description")] ProductCategory category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _productCategoryService.AddCategoryAsync(category);
                    TempData["SuccessMessage"] = "Danh mục đã được thêm thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm danh mục.");
                }
            }
            return View(category);
        }

        // GET: Admin/AdminProductCategory/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _productCategoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/AdminProductCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Name,Description")] ProductCategory category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _productCategoryService.UpdateCategoryAsync(category);
                    TempData["SuccessMessage"] = "Danh mục đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật danh mục.");
                }
            }
            return View(category);
        }

        // GET: Admin/AdminProductCategory/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _productCategoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Admin/AdminProductCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var deleted = await _productCategoryService.DeleteCategoryAsync(id);
                if (deleted)
                {
                    TempData["SuccessMessage"] = "Danh mục đã được xóa thành công.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không tìm thấy danh mục để xóa.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message; // Hiển thị lỗi nếu có phim đang sử dụng
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa danh mục.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}