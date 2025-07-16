using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebFilmOnline.Models;
using WebFilmOnline.Services.Interfaces;
using WebFilmOnline.Data; // To get Users for dropdown

namespace WebFilmOnline.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class AdminUserChannelController : Controller
    {
        private readonly IUserChannelService _userChannelService;
        private readonly IStreamingProviderService _streamingProviderService;
        private readonly FilmServiceDBContext _context; // For getting users

        public AdminUserChannelController(
            IUserChannelService userChannelService,
            IStreamingProviderService streamingProviderService,
            FilmServiceDBContext context)
        {
            _userChannelService = userChannelService;
            _streamingProviderService = streamingProviderService;
            _context = context; // Inject DbContext to get Users (or use a UserService if available)
        }

        // GET: Admin/AdminUserChannel
        public async Task<IActionResult> Index()
        {
            var channels = await _userChannelService.GetAllUserChannelsAsync();
            return View(channels);
        }

        // GET: Admin/AdminUserChannel/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdowns();
            return View();
        }

        // POST: Admin/AdminUserChannel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Name,Description,ProviderId,ApiKey,IsActive")] UserChannel channel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _userChannelService.AddUserChannelAsync(channel);
                    TempData["SuccessMessage"] = "Kênh người dùng đã được thêm thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm kênh người dùng.");
                }
            }
            await PopulateDropdowns(channel.UserId, channel.ProviderId);
            return View(channel);
        }

        // GET: Admin/AdminUserChannel/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var channel = await _userChannelService.GetUserChannelByIdAsync(id);
            if (channel == null)
            {
                return NotFound();
            }
            await PopulateDropdowns(channel.UserId, channel.ProviderId);
            return View(channel);
        }

        // POST: Admin/AdminUserChannel/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ChannelId,UserId,Name,Description,ProviderId,ApiKey,IsActive")] UserChannel channel)
        {
            if (id != channel.ChannelId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _userChannelService.UpdateUserChannelAsync(channel);
                    TempData["SuccessMessage"] = "Kênh người dùng đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật kênh người dùng.");
                }
            }
            await PopulateDropdowns(channel.UserId, channel.ProviderId);
            return View(channel);
        }

        // GET: Admin/AdminUserChannel/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var channel = await _userChannelService.GetUserChannelByIdAsync(id);
            if (channel == null)
            {
                return NotFound();
            }
            return View(channel);
        }

        // POST: Admin/AdminUserChannel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _userChannelService.DeleteUserChannelAsync(id);
                TempData["SuccessMessage"] = "Kênh người dùng đã được xóa thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa kênh người dùng.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateDropdowns(int? selectedUserId = null, int? selectedProviderId = null)
        {
            ViewBag.Users = new SelectList(await _context.User.ToListAsync(), "UserId", "UserName", selectedUserId);
            ViewBag.Providers = new SelectList(await _streamingProviderService.GetAllProvidersAsync(), "ProviderId", "Name", selectedProviderId);
        }
    }
}
