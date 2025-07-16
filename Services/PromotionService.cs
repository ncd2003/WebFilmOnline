using WebFilmOnline.Models;

public class PromotionService : IPromotionService
{
    private readonly FilmServiceDbContext _context;

    public PromotionService(FilmServiceDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Promotion> GetAll()
    {
        return _context.Promotions.ToList();
    }

    public void Create(Promotion promotion)
    {
        _context.Promotions.Add(promotion);
        _context.SaveChanges();
    }
}
