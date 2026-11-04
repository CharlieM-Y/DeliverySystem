using DeliverySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeliverySystem.Pages
{
    public class AccountsModel : PageModel
    {
        private readonly DBcontext _dbcontext;

        public List<Account> Accounts { get; set; } = new();

        [BindProperty]
        public Customer NewCustomer { get; set; } = new();

        [BindProperty]
        public OfficeStaff NewOfficeStaff { get; set; } = new();

        [BindProperty]
        public DeliveryStaff NewDeliveryStaff { get; set; } = new();

        public AccountsModel(DBcontext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public void OnGet()
        {
            Accounts = _dbcontext.Accounts.ToList();
        }

        public IActionResult OnPostCustomer()
        {
            _dbcontext.Customers.Add(NewCustomer);
            _dbcontext.SaveChanges();

            return RedirectToPage();
        }

        public IActionResult OnPostOfficeStaff()
        {
            _dbcontext.OfficeStaff.Add(NewOfficeStaff);
            _dbcontext.SaveChanges();

            return RedirectToPage();
        }

        public IActionResult OnPostDeliveryStaff()
        {
            _dbcontext.DeliveryStaff.Add(NewDeliveryStaff);
            _dbcontext.SaveChanges();

            return RedirectToPage();
        }
    }
}
