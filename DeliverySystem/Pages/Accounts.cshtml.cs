using DeliverySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DeliverySystem.Pages
{
    public class AccountsModel : PageModel
    {
        private readonly DBcontext _dbcontext;

        public List<Account> Accounts { get; set; } = new List<Account>();

        [BindProperty] public Account NewAccount { get; set; }
        public AccountsModel(DBcontext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public void OnGet()
        {
            Accounts = _dbcontext.Accounts.ToList();
        }

        public IActionResult OnPost()
        {
            
            _dbcontext.Accounts.Add(NewAccount);
            _dbcontext.SaveChanges();
            return RedirectToPage();
        }
    }
}
