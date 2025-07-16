using WebFilmOnline.Models;
using System;

public class PromotionValidationService
{
    public bool IsValid(Promotion promotion)
    {
        if (string.IsNullOrWhiteSpace(promotion.Name))
            return false;

        if (promotion.DiscountPercent <= 0 || promotion.DiscountPercent > 100)
            return false;

        if (promotion.StartDate >= promotion.EndDate)
            return false;

        if (promotion.EndDate < DateTime.Now)
            return false;

        return true;
    }
}
