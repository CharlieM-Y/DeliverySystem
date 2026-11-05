
using DeliverySystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DeliverySystem.Pages
{
    public class DeliveryStaffViewModel : PageModel
    {
        private readonly DBcontext _db;

        public DeliveryStaffViewModel(DBcontext db)
        {
            _db = db;
        }

        // CURRENT LOGGED-IN DELIVERY STAFF

        public DeliveryStaff? CurrentStaff { get; set; }

        // ASSIGNED DELIVERIES

        public List<Delivery> Deliveries { get; set; } = new();

        // FORM INPUT

        [BindProperty]
        public UpdateStatusInput StatusInput { get; set; } = new();

        public class UpdateStatusInput
        {
            [Required]
            public Guid DeliveryId { get; set; }

            [Required]
            public DeliveryStatus Status { get; set; }
        }

        // SUMMARY COUNTS

        public int TotalAssigned =>
            Deliveries.Count;

        public int InProgressCount =>
            Deliveries.Count(d =>
                d.Status != DeliveryStatus.Delivered &&
                d.Status != DeliveryStatus.Failed);

        public int CompletedCount =>
            Deliveries.Count(d =>
                d.Status == DeliveryStatus.Delivered);

        // GET CURRENT STAFF FROM SESSION

        private bool TryGetCurrentStaff()
        {
            string? idString =
                HttpContext.Session.GetString("AccountId");

            if (!Guid.TryParse(idString, out Guid id))
                return false;

            CurrentStaff = _db.DeliveryStaff
                .FirstOrDefault(d => d.Id == id);

            return CurrentStaff != null;
        }

        // LOAD ASSIGNED DELIVERIES

        private async Task LoadDataAsync()
        {
            if (CurrentStaff == null)
                return;

            Deliveries = await _db.Deliveries
                .Where(d =>
                    d.DeliveryStaffId == CurrentStaff.Id)
                .Include(d => d.Customer)
                .Include(d => d.Parcels)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();
        }

        // DISPLAY PAGE

        public async Task<IActionResult> OnGetAsync()
        {
            if (!TryGetCurrentStaff())
                return RedirectToPage("/Index");

            await LoadDataAsync();

            return Page();
        }

        // CLOCK IN / CLOCK OUT

        public async Task<IActionResult> OnPostToggleAvailabilityAsync()
        {
            if (!TryGetCurrentStaff())
                return RedirectToPage("/Index");

            // Toggle the current staff member's availability
            CurrentStaff!.IsAvailable = !CurrentStaff.IsAvailable;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = CurrentStaff.IsAvailable
                ? "You have successfully clocked in."
                : "You have successfully clocked out.";

            return RedirectToPage();
        }

        // UPDATE DELIVERY STATUS

        public async Task<IActionResult> OnPostUpdateStatusAsync()
        {
            if (!TryGetCurrentStaff())
                return RedirectToPage("/Index");

            ModelState.Clear();

            if (!TryValidateModel(
                StatusInput, nameof(StatusInput)))
            {
                await LoadDataAsync();
                return Page();
            }

            // Find the delivery, ensuring it belongs
            // to the currently logged-in staff member.

            var delivery = await _db.Deliveries
                .FirstOrDefaultAsync(d =>
                    d.Id == StatusInput.DeliveryId &&
                    d.DeliveryStaffId == CurrentStaff!.Id);

            if (delivery == null)
            {
                ModelState.AddModelError(
                    "",
                    "Delivery not found or not assigned to you.");

                await LoadDataAsync();
                return Page();
            }

            // Prevent updates to finished deliveries.

            if (delivery.Status == DeliveryStatus.Delivered ||
                delivery.Status == DeliveryStatus.Failed)
            {
                ModelState.AddModelError(
                    "",
                    "This delivery has already been completed.");

                await LoadDataAsync();
                return Page();
            }

            // Allow delivery staff to select
            // an appropriate status.

            if (StatusInput.Status != DeliveryStatus.ReadyForDelivery &&
                StatusInput.Status != DeliveryStatus.PickedUp &&
                StatusInput.Status != DeliveryStatus.Delivered &&
                StatusInput.Status != DeliveryStatus.Failed)
            {
                ModelState.AddModelError(
                    "",
                    "Invalid delivery status.");

                await LoadDataAsync();
                return Page();
            }

            // Update the delivery.

            delivery.Status = StatusInput.Status;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Delivery status updated successfully.";

            return RedirectToPage();
        }
    }
}
