using Core.Abstracts.Interfaces;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Data.Contexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace UI.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    [Route("admin/reservation")]
    public class AdminReservationController : Controller
    {
        private readonly IReservationService _reservationService;
        private readonly UserManager<Guest> _userManager;
        private readonly StayHubContext _context;
        private readonly IMapper _mapper;

        public AdminReservationController(
            IReservationService reservationService,
            UserManager<Guest> userManager,
            StayHubContext context,
            IMapper mapper)
        {
            _reservationService = reservationService;
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(int hotelId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            if (!User.IsInRole("SuperAdmin") && user.HotelId != hotelId) return Forbid();

            // 🛡️ ÇÖZÜM: SQLite hatasını önlemek için önce Oda ID'lerini listeliyoruz
            var hotelRoomIds = await _context.Rooms
                .Where(rm => rm.HotelId == hotelId)
                .Select(rm => rm.Id)
                .ToListAsync();

            // Rezervasyonları bu Oda ID listesi üzerinden çekiyoruz
            // Bu sayede SQL tarafında r.HotelId kolonu asla aranmaz
            var reservations = await _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.Guest)
                .Where(r => hotelRoomIds.Contains(r.RoomId) && !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var dtoList = _mapper.Map<List<ReservationDto>>(reservations);

            foreach (var dto in dtoList)
            {
                var original = reservations.FirstOrDefault(x => x.Id == dto.Id);
                if (original != null)
                {
                    dto.HotelId = hotelId;
                    dto.RoomNumber = original.Room?.RoomNumber ?? "N/A";
                    dto.RoomName = original.Room?.Name ?? "Standart Oda";
                    dto.HotelName = original.Room?.Hotel?.Name ?? "StayHub";
                }
            }

            var hotel = await _context.Hotels.AsNoTracking().FirstOrDefaultAsync(h => h.Id == hotelId);
            ViewBag.HotelName = hotel?.Name;
            ViewBag.HotelId = hotelId;

            return View(dtoList);
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var reservationDto = await _reservationService.GetReservationByIdAsync(id);
            if (reservationDto == null) return NotFound();

            return View(reservationDto);
        }

        [HttpPost("cancel/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id, int hotelId)
        {
            var result = await _reservationService.CancelReservationAsync(id);

            if (result.IsSuccess)
            {
                await _reservationService.SendCancellationEmail(id);
                TempData["SuccessMessage"] = "Rezervasyon başarıyla iptal edildi.";
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToAction(nameof(Index), new { hotelId = hotelId });
        }
    }
}