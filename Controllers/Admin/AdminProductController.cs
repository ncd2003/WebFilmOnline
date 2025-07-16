using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using WebFilmOnline.Models;
using WebFilmOnline.Services.Interfaces;
using WebFilmOnline.Services.ViewModels; // Để sử dụng ViewModels

namespace WebFilmOnline.Controllers.Admin
{
    [Authorize(Roles = "Admin")] // Yêu cầu quyền Admin
    [Area("Admin")] // Nếu bạn đã cấu hình Area cho Admin
    public class AdminProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly IProductCategoryService _productCategoryService;
        private readonly IStreamingProviderService _streamingProviderService;
        private readonly IUserChannelService _userChannelService;

        public AdminProductController(
            IProductService productService,
            IProductCategoryService productCategoryService,
            IStreamingProviderService streamingProviderService,
            IUserChannelService userChannelService)
        {
            _productService = productService;
            _productCategoryService = productCategoryService;
            _streamingProviderService = streamingProviderService;
            _userChannelService = userChannelService;
        }

        // GET: Admin/AdminProduct
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        // GET: Admin/AdminProduct/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Categories = new SelectList(await _productCategoryService.GetAllCategoriesAsync(), "CategoryId", "Name");
            return View();
        }

        // POST: Admin/AdminProduct/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductCreateUpdateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    int? createdByUserId = null;
                    if (int.TryParse(userId, out int id)) { createdByUserId = id; }

                    var product = new Product
                    {
                        Title = model.Title,
                        Description = model.Description,
                        CategoryId = model.CategoryId,
                        ThumbnailUrl = model.ThumbnailUrl,
                        Price = model.Price,
                        IsActive = model.IsActive
                    };

                    await _productService.AddProductAsync(product, createdByUserId);
                    TempData["SuccessMessage"] = "Phim đã được thêm thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm phim: " + ex.Message);
                }
            }
            ViewBag.Categories = new SelectList(await _productCategoryService.GetAllCategoriesAsync(), "CategoryId", "Name", model.CategoryId);
            return View(model);
        }

        // GET: Admin/AdminProduct/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var model = new ProductCreateUpdateViewModel
            {
                ProductId = product.ProductId,
                Title = product.Title,
                Description = product.Description,
                CategoryId = product.CategoryId,
                ThumbnailUrl = product.ThumbnailUrl,
                Price = product.Price,
                IsActive = product.IsActive
            };

            ViewBag.Categories = new SelectList(await _productCategoryService.GetAllCategoriesAsync(), "CategoryId", "Name", model.CategoryId);
            return View(model);
        }

        // POST: Admin/AdminProduct/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductCreateUpdateViewModel model)
        {
            if (id != model.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var product = await _productService.GetProductByIdAsync(id);
                    if (product == null) { return NotFound(); }

                    product.Title = model.Title;
                    product.Description = model.Description;
                    product.CategoryId = model.CategoryId;
                    product.ThumbnailUrl = model.ThumbnailUrl;
                    product.Price = model.Price;
                    product.IsActive = model.IsActive;

                    await _productService.UpdateProductAsync(product);
                    TempData["SuccessMessage"] = "Phim đã được cập nhật thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (ArgumentException ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật phim: " + ex.Message);
                }
            }
            ViewBag.Categories = new SelectList(await _productCategoryService.GetAllCategoriesAsync(), "CategoryId", "Name", model.CategoryId);
            return View(model);
        }

        // GET: Admin/AdminProduct/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Admin/AdminProduct/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
                TempData["SuccessMessage"] = "Phim đã được xóa thành công.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message; // Hiển thị lỗi nếu có ràng buộc
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa phim.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/AdminProduct/ManageStreaming/5
        public async Task<IActionResult> ManageStreaming(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Product = product;
            ViewBag.Providers = await _streamingProviderService.GetAllProvidersAsync();
            ViewBag.UserChannels = await _userChannelService.GetAllUserChannelsAsync(); // Get all channels, including those created by users

            var streamingSources = await _productService.GetStreamingSourcesForProductAsync(productId);
            return View(streamingSources);
        }

        // POST: Admin/AdminProduct/AddProductStreaming
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductStreaming(ProductStreamingCreateUpdateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var streamingInfo = new ProductStreaming
                    {
                        ProductId = model.ProductId,
                        ProviderId = model.ProviderId > 0 ? model.ProviderId : null,
                        ChannelId = model.ChannelId > 0 ? model.ChannelId : null,
                        StreamingUrlOrId = model.StreamingUrlOrId,
                        Priority = model.Priority,
                        IsActive = model.IsActive
                    };

                    await _productService.AddProductStreamingAsync(streamingInfo);
                    TempData["SuccessMessage"] = "Nguồn streaming đã được thêm thành công.";
                }
                catch (ArgumentException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                catch (InvalidOperationException ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
                catch (Exception)
                {
                    TempData["ErrorMessage"] = "Đã xảy ra lỗi khi thêm nguồn streaming.";
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
            }
            return RedirectToAction(nameof(ManageStreaming), new { productId = model.ProductId });
        }

        // POST: Admin/AdminProduct/DeleteProductStreaming
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductStreaming(int productStreamingId, int productId)
        {
            try
            {
                await _productService.DeleteProductStreamingAsync(productStreamingId);
                TempData["SuccessMessage"] = "Nguồn streaming đã được xóa thành công.";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa nguồn streaming.";
            }
            return RedirectToAction(nameof(ManageStreaming), new { productId = productId });
        }
    }
}