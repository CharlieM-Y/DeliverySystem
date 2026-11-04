
using DeliverySystem.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DeliverySystem.Pages
{
    public class DeliveriesModel : PageModel
    {
        private readonly DBcontext _dbcontext;

        public List<Delivery> Deliveries { get; set; } = new();

        public DeliveriesModel(DBcontext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public void OnGet()
        {
            Deliveries = _dbcontext.Deliveries
                .Include(d => d.Customer)
                .Include(d => d.Parcels)
                .Include(d => d.DeliveryStaff)
                .OrderByDescending(d => d.CreatedAt)
                .ToList();
        }
    }
}
