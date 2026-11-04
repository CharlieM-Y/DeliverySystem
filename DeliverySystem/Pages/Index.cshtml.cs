using DeliverySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeliverySystem.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DBcontext _db;

        public IndexModel(DBcontext db)
        {
            _db = db;
        }

        public List<Account> Accounts { get; set; } = new();

        [BindProperty]
        public Guid SelectedAccountId { get; set; }

        public void OnGet()
        {
            Accounts = _db.Accounts
                .OrderBy(a => a.Name)
                .ToList();
        }

        public IActionResult OnPost()
        {
            var account = _db.Accounts
                .FirstOrDefault(a => a.Id == SelectedAccountId);

            if (account == null)
            {
                ModelState.AddModelError(
                    "", "Please select a valid account.");

                OnGet();
                return Page();
            }

            // Remember selected account
            HttpContext.Session.SetString(
                "AccountId", account.Id.ToString());

            // Redirect based on account type
            if (account is Customer)
            {
                return RedirectToPage("/CustomerView");
            }

            if (account is OfficeStaff)
            {
                return RedirectToPage("/OfficeStaffView");
            }

            if (account is DeliveryStaff)
            {
                return RedirectToPage("/DeliveryStaffView");
            }

            return RedirectToPage("/Index");
        }
    }
}
