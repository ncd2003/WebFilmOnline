using Microsoft.AspNetCore.Mvc;
using WebFilmOnline.Models;
using WebFilmOnline.Services;

public class PromotionController : Controller
{
    private readonly IPromotionService _promotionService;

    public PromotionController(IPromotionService promotionService)
    {
        _promotionService = promotionService;
    }

    public IActionResult Index()
    {
        var promotions = _promotionService.GetAll();
        return View(promotions);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Promotion promotion)
    {
        if (ModelState.IsValid)
        {
            _promotionService.Create(promotion);
            return RedirectToAction("Index");
        }
        return View(promotion);
    }


}
