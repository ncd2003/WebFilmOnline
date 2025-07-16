using WebFilmOnline.Models;
using System.Collections.Generic;

public interface IPromotionService
{
    IEnumerable<Promotion> GetAll();
    void Create(Promotion promotion);
}
