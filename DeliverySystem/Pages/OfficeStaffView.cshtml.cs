
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

        // ========================================
        // CURRENT OFFICE STAFF
        // ========================================

        public OfficeStaff? CurrentStaff { get; set; }

        // ========================================
        // DISPLAY DATA
        // ========================================

        public List<Account> Accounts { get; set; } = new();

        public List<Customer> Customers { get; set; } = new();

        public List<DeliveryStaff> Drivers { get; set; } = new();

        public List<Delivery> Deliveries { get; set; } = new();

        public List<DeliveryRequest> DeliveryRequests { get; set; } = new();

        // ========================================
        // FORM INPUTS
        // ========================================

        [BindProperty]
        public CreateAccountInput AccountInput { get; set; } = new();

        [BindProperty]
        public QuoteRequestInput QuoteInput { get; set; } = new();

        [BindProperty]
        public AssignDeliveryInput AssignmentInput { get; set; } = new();

        // ========================================
        // CREATE ACCOUNT INPUT
        // ========================================

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

            public bool IsAvailable { get; set; } = false;
        }

        // ========================================
        // QUOTE REQUEST INPUT
        // ========================================

        public class QuoteRequestInput
        {
            [Required]
            public Guid RequestId { get; set; }

            [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
            public decimal QuotedCost { get; set; }

            [Required]
            public DateTime? EstimatedPickupTime { get; set; }

            [Required]
            public DateTime? EstimatedDeliveryTime { get; set; }
        }

        // ========================================
        // ASSIGN DELIVERY INPUT
        // ========================================

        public class AssignDeliveryInput
        {
            [Required]
            public Guid DeliveryId { get; set; }

            [Required]
            public Guid DeliveryStaffId { get; set; }
        }

        // ========================================
        // DASHBOARD SUMMARY
        // ========================================

        public int TotalAccounts =>
            Accounts.Count;

        public int TotalDeliveries =>
            Deliveries.Count;

        public int PendingRequests =>
            DeliveryRequests.Count(r =>
                r.Status == DeliveryRequestStatus.PendingReview);

        public int AwaitingResponseRequests =>
            DeliveryRequests.Count(r =>
                r.Status == DeliveryRequestStatus.AwaitingResponse);

        public int UnassignedDeliveries =>
            Deliveries.Count(d =>
                d.DeliveryStaffId == null &&
                d.Status != DeliveryStatus.Delivered &&
                d.Status != DeliveryStatus.Failed);

        // ========================================
        // GET CURRENT OFFICE STAFF FROM SESSION
        // ========================================

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

        // ========================================
        // LOAD ALL DASHBOARD DATA
        // ========================================

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

            DeliveryRequests = await _db.DeliveryRequests
                .Include(r => r.Customer)
                .Include(r => r.Parcels)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        // ========================================
        // DISPLAY PAGE
        // ========================================

        public async Task<IActionResult> OnGetAsync()
        {
            if (!TryGetCurrentStaff())
                return RedirectToPage("/Index");

            await LoadDataAsync();

            return Page();
        }

        // ========================================
        // CREATE ACCOUNT
        // ========================================

        public async Task<IActionResult> OnPostCreateAccountAsync()
        {
            if (!TryGetCurrentStaff())
                return RedirectToPage("/Index");

            ModelState.Clear();

            if (!TryValidateModel(
                AccountInput, nameof(AccountInput)))
            {
                await LoadDataAsync();
                return Page();
            }

            bool emailExists = await _db.Accounts
                .AnyAsync(a => a.Email == AccountInput.Email);

            if (emailExists)
            {
                ModelState.AddModelError(
                    "AccountInput.Email",
                    "This email address is already in use.");

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
                $"{AccountInput.AccountType} account created successfully.";

            return RedirectToPage();
        }

        // ========================================
        // QUOTE DELIVERY REQUEST
        // ========================================

        public async Task<IActionResult> OnPostQuoteRequestAsync()
        {
            if (!TryGetCurrentStaff())
                return RedirectToPage("/Index");

            ModelState.Clear();

            if (!TryValidateModel(
                QuoteInput, nameof(QuoteInput)))
            {
                await LoadDataAsync();
                return Page();
            }

            if (QuoteInput.RequestId == Guid.Empty)
            {
                TempData["ErrorMessage"] =
                    "Invalid delivery request.";

                return RedirectToPage();
            }

            var request = await _db.DeliveryRequests
                .FirstOrDefaultAsync(r =>
                    r.Id == QuoteInput.RequestId);

            if (request == null)
            {
                TempData["ErrorMessage"] =
                    "Delivery request not found.";

                return RedirectToPage();
            }

            if (request.Status !=
                DeliveryRequestStatus.PendingReview)
            {
                TempData["ErrorMessage"] =
                    "This request is no longer awaiting a quote.";

                return RedirectToPage();
            }

            if (!QuoteInput.EstimatedPickupTime.HasValue ||
                !QuoteInput.EstimatedDeliveryTime.HasValue ||
                QuoteInput.EstimatedDeliveryTime.Value <=
                    QuoteInput.EstimatedPickupTime.Value)
            {
                TempData["ErrorMessage"] =
                    "Estimated delivery time must be after pickup time.";

                return RedirectToPage();
            }

            // Save the quote

            request.QuotedCost = QuoteInput.QuotedCost;

            request.EstimatedPickupTime =
                QuoteInput.EstimatedPickupTime;

            request.EstimatedDeliveryTime =
                QuoteInput.EstimatedDeliveryTime;

            // The customer can now accept or decline

            request.Status =
                DeliveryRequestStatus.AwaitingResponse;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Quote sent successfully. " +
                "The customer can now accept or decline it.";

            return RedirectToPage();
        }

        // ========================================
        // ASSIGN DELIVERY STAFF
        // ========================================

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
                .FirstOrDefaultAsync(d =>
                    d.Id == AssignmentInput.DeliveryId);

            if (delivery == null)
            {
                TempData["ErrorMessage"] =
                    "Delivery not found.";

                return RedirectToPage();
            }

            var driver = await _db.DeliveryStaff
                .FirstOrDefaultAsync(d =>
                    d.Id == AssignmentInput.DeliveryStaffId);

            if (driver == null)
            {
                TempData["ErrorMessage"] =
                    "Delivery staff member not found.";

                return RedirectToPage();
            }

            // Driver must be clocked in

            if (!driver.IsAvailable)
            {
                TempData["ErrorMessage"] =
                    "This delivery staff member is clocked out.";

                return RedirectToPage();
            }

            // Completed deliveries cannot be assigned

            if (delivery.Status == DeliveryStatus.Delivered ||
                delivery.Status == DeliveryStatus.Failed)
            {
                TempData["ErrorMessage"] =
                    "Completed deliveries cannot be assigned.";

                return RedirectToPage();
            }

            // Assign delivery to driver

            delivery.DeliveryStaffId = driver.Id;

            delivery.Status = DeliveryStatus.Allocated;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                $"Delivery assigned to {driver.Name} successfully.";

            return RedirectToPage();
        }
    }
}
