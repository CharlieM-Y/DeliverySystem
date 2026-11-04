
using DeliverySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DeliverySystem.Pages
{
    public class CustomerViewModel : PageModel
    {
        private readonly DBcontext _db;

        public CustomerViewModel(DBcontext db)
        {
            _db = db;
        }

        // CURRENT CUSTOMER

        public Customer? CurrentCustomer { get; set; }

        // CUSTOMER DELIVERIES

        public List<Delivery> Deliveries { get; set; } = new();

        // PROFILE FORM

        [BindProperty]
        public EditProfileInput ProfileInput { get; set; } = new();

        public class EditProfileInput
        {
            [Required]
            public string Name { get; set; } = "";

            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            // Leave blank to keep existing password
            public string? Password { get; set; }

            public string? DeliveryAddress { get; set; }
        }

        // DELIVERY SUMMARY

        public int TotalDeliveries =>
            Deliveries.Count;

        public int ActiveDeliveries =>
            Deliveries.Count(d =>
                d.Status != DeliveryStatus.Delivered &&
                d.Status != DeliveryStatus.Failed);

        public int CompletedDeliveries =>
            Deliveries.Count(d =>
                d.Status == DeliveryStatus.Delivered);

        // GET CUSTOMER FROM SESSION

        private bool TryGetCurrentCustomer()
        {
            string? idString =
                HttpContext.Session.GetString("AccountId");

            if (!Guid.TryParse(idString, out Guid id))
                return false;

            CurrentCustomer = _db.Customers
                .FirstOrDefault(c => c.Id == id);

            return CurrentCustomer != null;
        }

        // LOAD CUSTOMER DELIVERIES

        private async Task LoadDataAsync()
        {
            if (CurrentCustomer == null)
                return;

            Deliveries = await _db.Deliveries
                .Where(d =>
                    d.CustomerId == CurrentCustomer.Id)
                .Include(d => d.Parcels)
                .Include(d => d.DeliveryStaff)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        // DISPLAY PAGE

        public async Task<IActionResult> OnGetAsync()
        {
            if (!TryGetCurrentCustomer())
                return RedirectToPage("/Index");

            await LoadDataAsync();

            // Populate profile form with existing information

            ProfileInput = new EditProfileInput
            {
                Name = CurrentCustomer!.Name,
                Email = CurrentCustomer.Email,
                DeliveryAddress = CurrentCustomer.DeliveryAddress
            };

            return Page();
        }

        // UPDATE CUSTOMER PROFILE

        public async Task<IActionResult> OnPostUpdateProfileAsync()
        {
            if (!TryGetCurrentCustomer())
                return RedirectToPage("/Index");

            // Validate only the profile form

            ModelState.Clear();

            if (!TryValidateModel(
                ProfileInput, nameof(ProfileInput)))
            {
                await LoadDataAsync();
                return Page();
            }

            // Prevent duplicate email addresses

            bool emailExists = await _db.Accounts
                .AnyAsync(a =>
                    a.Email == ProfileInput.Email &&
                    a.Id != CurrentCustomer!.Id);

            if (emailExists)
            {
                ModelState.AddModelError(
                    "ProfileInput.Email",
                    "This email address is already used by another account.");

                await LoadDataAsync();
                return Page();
            }

            // Update customer information

            CurrentCustomer!.Name = ProfileInput.Name;
            CurrentCustomer.Email = ProfileInput.Email;
            CurrentCustomer.DeliveryAddress =
                ProfileInput.DeliveryAddress;

            // Only update password if one was entered

            if (!string.IsNullOrWhiteSpace(ProfileInput.Password))
            {
                CurrentCustomer.Password = ProfileInput.Password;
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Your profile has been updated successfully.";

            return RedirectToPage();
        }
    }
}

