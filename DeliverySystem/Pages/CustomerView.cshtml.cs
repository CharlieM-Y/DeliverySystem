
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

        // DISPLAY DATA
        public List<Delivery> Deliveries { get; set; } = new();

        public List<DeliveryRequest> DeliveryRequests { get; set; } = new();

   
        // FORM INPUTS
        [BindProperty]
        public EditProfileInput ProfileInput { get; set; } = new();

        [BindProperty]
        public CreateRequestInput RequestInput { get; set; } = new();

        [BindProperty]
        public Guid ResponseRequestId { get; set; }

    
        // PROFILE INPUT MODEL
        public class EditProfileInput
        {
            [Required]
            public string Name { get; set; } = "";

            [Required]
            [EmailAddress]
            public string Email { get; set; } = "";

            public string? Password { get; set; }

            public string? DeliveryAddress { get; set; }
        }


        // DELIVERY REQUEST INPUT MODEL
        public class CreateRequestInput
        {
            [Required]
            public string PickupAddress { get; set; } = "";

            [Required]
            public string DeliveryAddress { get; set; } = "";

            [Range(0.01, double.MaxValue)]
            public double Length { get; set; }

            [Range(0.01, double.MaxValue)]
            public double Width { get; set; }

            [Range(0.01, double.MaxValue)]
            public double Height { get; set; }

            [Range(0.01, double.MaxValue)]
            public double Weight { get; set; }
        }

    
        // DASHBOARD SUMMARY
        public int TotalDeliveries =>
            Deliveries.Count;

        public int ActiveDeliveries =>
            Deliveries.Count(d =>
                d.Status != DeliveryStatus.Delivered &&
                d.Status != DeliveryStatus.Failed);

        public int CompletedDeliveries =>
            Deliveries.Count(d =>
                d.Status == DeliveryStatus.Delivered);

        public int PendingRequests =>
            DeliveryRequests.Count(r =>
                r.Status == DeliveryRequestStatus.PendingReview);

        public int AwaitingResponseRequests =>
            DeliveryRequests.Count(r =>
                r.Status == DeliveryRequestStatus.AwaitingResponse);

        // GET CURRENT CUSTOMER FROM SESSION
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

        // LOAD CUSTOMER DATA
        private async Task LoadDataAsync()
        {
            if (CurrentCustomer == null)
                return;

            Deliveries = await _db.Deliveries
                .Where(d => d.CustomerId == CurrentCustomer.Id)
                .Include(d => d.Parcels)
                .Include(d => d.DeliveryStaff)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            DeliveryRequests = await _db.DeliveryRequests
                .Where(r => r.CustomerId == CurrentCustomer.Id)
                .Include(r => r.Parcels)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        // DISPLAY PAGE
        public async Task<IActionResult> OnGetAsync()
        {
            if (!TryGetCurrentCustomer())
                return RedirectToPage("/Index");

            await LoadDataAsync();

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

            ModelState.Clear();

            if (!TryValidateModel(
                ProfileInput, nameof(ProfileInput)))
            {
                await LoadDataAsync();
                return Page();
            }

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

            CurrentCustomer!.Name = ProfileInput.Name;
            CurrentCustomer.Email = ProfileInput.Email;
            CurrentCustomer.DeliveryAddress =
                ProfileInput.DeliveryAddress;

            if (!string.IsNullOrWhiteSpace(ProfileInput.Password))
            {
                CurrentCustomer.Password = ProfileInput.Password;
            }

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Your profile has been updated successfully.";

            return RedirectToPage();
        }

        // CREATE DELIVERY REQUEST
        public async Task<IActionResult> OnPostCreateRequestAsync()
        {
            if (!TryGetCurrentCustomer())
                return RedirectToPage("/Index");

            ModelState.Clear();

            if (!TryValidateModel(
                RequestInput, nameof(RequestInput)))
            {
                await LoadDataAsync();

                ProfileInput = new EditProfileInput
                {
                    Name = CurrentCustomer!.Name,
                    Email = CurrentCustomer.Email,
                    DeliveryAddress = CurrentCustomer.DeliveryAddress
                };

                return Page();
            }

            var request = new DeliveryRequest
            {
                CustomerId = CurrentCustomer!.Id,

                PickupAddress = RequestInput.PickupAddress,
                DeliveryAddress = RequestInput.DeliveryAddress,

                CreatedAt = DateTime.UtcNow,

                Status = DeliveryRequestStatus.PendingReview,

                Parcels = new List<Parcel>
                {
                    new Parcel
                    {
                        Length = RequestInput.Length,
                        Width = RequestInput.Width,
                        Height = RequestInput.Height,
                        Weight = RequestInput.Weight
                    }
                }
            };

            _db.DeliveryRequests.Add(request);

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Delivery request submitted successfully. " +
                "Please wait for Office Staff to provide a quote.";

            return RedirectToPage();
        }

        // ACCEPT DELIVERY QUOTE
        public async Task<IActionResult> OnPostAcceptQuoteAsync()
        {
            if (!TryGetCurrentCustomer())
                return RedirectToPage("/Index");

            var request = await _db.DeliveryRequests
                .Include(r => r.Parcels)
                .FirstOrDefaultAsync(r =>
                    r.Id == ResponseRequestId &&
                    r.CustomerId == CurrentCustomer!.Id);

            if (request == null)
            {
                TempData["ErrorMessage"] =
                    "Delivery request not found.";

                return RedirectToPage();
            }

            if (request.Status !=
                DeliveryRequestStatus.AwaitingResponse)
            {
                TempData["ErrorMessage"] =
                    "This request is not awaiting a response.";

                return RedirectToPage();
            }

            if (request.DeliveryId != null)
            {
                TempData["ErrorMessage"] =
                    "A delivery has already been created for this request.";

                return RedirectToPage();
            }

            if (!request.QuotedCost.HasValue ||
                !request.EstimatedPickupTime.HasValue ||
                !request.EstimatedDeliveryTime.HasValue)
            {
                TempData["ErrorMessage"] =
                    "The quote is incomplete.";

                return RedirectToPage();
            }

            // Create the actual delivery
            var delivery = new Delivery
            {
                CustomerId = request.CustomerId,
                Status = DeliveryStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // Associate the request's parcels with the delivery
            foreach (var parcel in request.Parcels)
            {
                delivery.Parcels.Add(parcel);
            }

            _db.Deliveries.Add(delivery);

            // Link the request to the new delivery
            request.Delivery = delivery;

            request.Status = DeliveryRequestStatus.Accepted;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Quote accepted successfully. " +
                "Your delivery has now been created.";

            return RedirectToPage();
        }

        // DECLINE DELIVERY QUOTE
        public async Task<IActionResult> OnPostDeclineQuoteAsync()
        {
            if (!TryGetCurrentCustomer())
                return RedirectToPage("/Index");

            var request = await _db.DeliveryRequests
                .FirstOrDefaultAsync(r =>
                    r.Id == ResponseRequestId &&
                    r.CustomerId == CurrentCustomer!.Id);

            if (request == null)
            {
                TempData["ErrorMessage"] =
                    "Delivery request not found.";

                return RedirectToPage();
            }

            if (request.Status !=
                DeliveryRequestStatus.AwaitingResponse)
            {
                TempData["ErrorMessage"] =
                    "This quote cannot be declined.";

                return RedirectToPage();
            }

            request.Status = DeliveryRequestStatus.Declined;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "You have declined the delivery quote.";

            return RedirectToPage();
        }
    }
}


