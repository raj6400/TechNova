namespace TechNova.Controllers
{
    public interface ICartService
    {
        Task<dynamic> GetCartItemCountAsync();
    }
}