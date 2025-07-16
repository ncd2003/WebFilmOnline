using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebFilmOnline.Models;
using WebFilmOnline.Services.Interfaces;

namespace WebFilmOnline.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class AdminStreamingProviderController : Controller
    {
        private readonly IStreamingProviderService _streamingProviderService;

        public AdminStreamingProviderController(IStreamingProviderService streamingProviderService)
        {
            _streamingProviderService = streamingProviderService;
        }

        // GET: Admin/AdminStreamingProvider
        public async Task<IActionResult> Index()
        {
            var providers = await _streamingProviderService.GetAllProvidersAsync();
            return View(providers);
        }

        // GET: Admin/AdminStreamingProvider/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/AdminStreamingProvider/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,ApiKey,BaseUrl,IsActive")] StreamingProvider provider)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    int? createdByUserId = null;
                    if (int.TryParse(userId, out int id)) { createdByUserId = id; }

                    await _streamingProviderService.AddProviderAsync(provider, createdByUserId);
                    TempData["SuccessMessage"] = "Nhà cung cấp streaming đã được thêm thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (InvalidOperationException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm nhà cung cấp.");
                }
            }
            return View(provider);
        }

        // GET: Admin/AdminStreamingProvider/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var provider = await _streamingProviderService.GetProviderByIdAsync(id);
            if (provider == null)
            {
                return NotFound();
            }
            return View(provider);
        }

        // POST: Admin/AdminStreamingProvider/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProviderId,Name,ApiKey,BaseUrl,IsActive")] StreamingProvider provider)
        {
            if (id != provider.ProviderId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _streamingProviderService.UpdateProviderAsync(provider);
                    TempData["SuccessMessage"] = "Nhà cung cấp streaming đã được cập nhật thành công.";
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
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật nhà cung cấp.");
                }
            }
            return View(provider);
        }

        // GET: Admin/AdminStreamingProvider/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var provider = await _streamingProviderService.GetProviderByIdAsync(id);
            if (provider == null)
            {
                return NotFound();
            }
            return View(provider);
        }

        // POST: Admin/AdminStreamingProvider/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _streamingProviderService.DeleteProviderAsync(id);
                TempData["SuccessMessage"] = "Nhà cung cấp streaming đã được xóa thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa nhà cung cấp.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}