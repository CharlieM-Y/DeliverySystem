
using DeliverySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DeliverySystem.Pages
{
    public class OfficeStaffViewModel : PageModel
    {
        private readonly DBcontext _db;

        public OfficeStaffViewModel(DBcontext db)
        {
            _db = db;
        }

        // DISPLAY DATA

        public OfficeStaff? CurrentStaff { get; set; }

        public List<Account> Accounts { get; set; } = new();
        public List<Delivery> Deliveries { get; set; } = new();
        public List<Customer> Customers { get; set; } = new();
        public List<DeliveryStaff> Drivers { get; set; } = new();

        // FORM INPUTS

        [BindProperty]
        public CreateAccountInput AccountInput { get; set; } = new();

        [BindProperty]
        public CreateDeliveryInput DeliveryInput { get; set; } = new();

        [BindProperty]
        public AssignDeliveryInput AssignmentInput { get; set; } = new();

        // INPUT MODELS

        public class CreateAccountInput
        {
            [Required]
            public string AccountType { get; set; } = "Customer";

            [Required]
            public string Name { get; set; } = "";

            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            [Required]
            public string Password { get; set; } = "";

            public string? DeliveryAddress { get; set; }

            public string? Department { get; set; }

            public bool IsAvailable { get; set; } = true;
        }

        public class CreateDeliveryInput
        {
            [Required]
            public Guid CustomerId { get; set; }

            [Range(0.01, double.MaxValue)]
            public double Length { get; set; }

            [Range(0.01, double.MaxValue)]
            public double Width { get; set; }

            [Range(0.01, double.MaxValue)]
            public double Height { get; set; }

            [Range(0.01, double.MaxValue)]
            public double Weight { get; set; }
        }

        public class AssignDeliveryInput
        {
            [Required]
            public Guid DeliveryId { get; set; }

            [Required]
            public Guid DeliveryStaffId { get; set; }
        }

        // SESSION CHECK

        private bool TryGetCurrentStaff()
        {
            string? idString =
                HttpContext.Session.GetString("AccountId");

            if (!Guid.TryParse(idString, out Guid id))
                return false;

            CurrentStaff = _db.OfficeStaff
                .FirstOrDefault(s => s.Id == id);

            return CurrentStaff != null;
        }

        // LOAD PAGE DATA

        private async Task LoadDataAsync()
        {
            Accounts = await _db.Accounts
                .OrderBy(a => a.Name)
                .ToListAsync();

            Customers = await _db.Customers
                .OrderBy(c => c.Name)
                .ToListAsync();

            Drivers = await _db.DeliveryStaff
                .OrderBy(d => d.Name)
                .ToListAsync();

            Deliveries = await _db.Deliveries
                .Include(d => d.Customer)
                .Include(d => d.DeliveryStaff)
                .Include(d => d.Parcels)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!TryGetCurrentStaff())
                return RedirectToPage("/Index");

            await LoadDataAsync();
            return Page();
        }

        // CREATE ACCOUNT

        public async Task<IActionResult> OnPostCreateAccountAsync()
        {
            if (!TryGetCurrentStaff())
                return RedirectToPage("/Index");

            ModelState.Clear();

            if (!TryValidateModel(AccountInput, nameof(AccountInput)))
            {
                await LoadDataAsync();
                return Page();
            }

            if (await _db.Accounts.AnyAsync(
                a => a.Email == AccountInput.Email))
            {
                ModelState.AddModelError(
                    "AccountInput.Email",
                    "An account with this email already exists.");

                await LoadDataAsync();
                return Page();
            }

            Account account;

            switch (AccountInput.AccountType)
            {
                case "Customer":
                    account = new Customer
                    {
                        Name = AccountInput.Name,
                        Email = AccountInput.Email,
                        Password = AccountInput.Password,
                        DeliveryAddress = AccountInput.DeliveryAddress
                    };
                    break;

                case "OfficeStaff":
                    account = new OfficeStaff
                    {
                        Name = AccountInput.Name,
                        Email = AccountInput.Email,
                        Password = AccountInput.Password,
                        Department = AccountInput.Department
                    };
                    break;

                case "DeliveryStaff":
                    account = new DeliveryStaff
                    {
                        Name = AccountInput.Name,
                        Email = AccountInput.Email,
                        Password = AccountInput.Password,
                        IsAvailable = AccountInput.IsAvailable
                    };
                    break;

                default:
                    ModelState.AddModelError(
                        "AccountInput.AccountType",
                        "Invalid account type.");

                    await LoadDataAsync();
                    return Page();
            }

            _db.Accounts.Add(account);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Account created successfully.";

            return RedirectToPage();
        }

        // CREATE DELIVERY

        public async Task<IActionResult> OnPostCreateDeliveryAsync()
        {
            if (!TryGetCurrentStaff())
                return RedirectToPage("/Index");

            ModelState.Clear();

            if (!TryValidateModel(
                DeliveryInput, nameof(DeliveryInput)))
            {
                await LoadDataAsync();
                return Page();
            }

            var customer = await _db.Customers
                .FirstOrDefaultAsync(
                    c => c.Id == DeliveryInput.CustomerId);

            if (customer == null)
            {
                ModelState.AddModelError(
                    "DeliveryInput.CustomerId",
                    "Please select a valid customer.");

                await LoadDataAsync();
                return Page();
            }

            var delivery = new Delivery
            {
                CustomerId = customer.Id,
                Status = DeliveryStatus.Pending,
                CreatedAt = DateTime.UtcNow,

                Parcels = new List<Parcel>
                {
                    new Parcel
                    {
                        Length = DeliveryInput.Length,
                        Width = DeliveryInput.Width,
                        Height = DeliveryInput.Height,
                        Weight = DeliveryInput.Weight
                    }
                }
            };

            _db.Deliveries.Add(delivery);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Delivery created successfully.";

            return RedirectToPage();
        }

        // ASSIGN DELIVERY STAFF

        public async Task<IActionResult> OnPostAssignDeliveryAsync()
        {
            if (!TryGetCurrentStaff())
                return RedirectToPage("/Index");

            ModelState.Clear();

            if (!TryValidateModel(
                AssignmentInput, nameof(AssignmentInput)))
            {
                await LoadDataAsync();
                return Page();
            }

            var delivery = await _db.Deliveries
                .FirstOrDefaultAsync(
                    d => d.Id == AssignmentInput.DeliveryId);

            var driver = await _db.DeliveryStaff
                .FirstOrDefaultAsync(
                    d => d.Id == AssignmentInput.DeliveryStaffId);

            if (delivery == null || driver == null)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid delivery or delivery staff.");

                await LoadDataAsync();
                return Page();
            }

            // Only check IsAvailable
            if (!driver.IsAvailable)
            {
                ModelState.AddModelError(
                    "",
                    "The selected delivery staff member is unavailable.");

                await LoadDataAsync();
                return Page();
            }

            if (delivery.Status == DeliveryStatus.Delivered ||
                delivery.Status == DeliveryStatus.Failed)
            {
                ModelState.AddModelError(
                    "",
                    "Completed or failed deliveries cannot be assigned.");

                await LoadDataAsync();
                return Page();
            }

            delivery.DeliveryStaffId = driver.Id;
            delivery.Status = DeliveryStatus.Allocated;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Delivery staff assigned successfully.";

            return RedirectToPage();
        }
    }
}
