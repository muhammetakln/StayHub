using Core.Abstracts.Interfaces;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace UI.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    [Route("admin/room")]
    public class AdminRoomController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IHotelService _hotelService;
        private readonly UserManager<Guest> _userManager;

        public AdminRoomController(IRoomService roomService, IHotelService hotelService, UserManager<Guest> userManager)
        {
            _roomService = roomService;
            _hotelService = hotelService;
            _userManager = userManager;
        }

        // Otelin odalarını listeler
        [HttpGet("index/{hotelId}")]
        public async Task<IActionResult> Index(int hotelId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("SuperAdmin") && user?.HotelId != hotelId) return Forbid();

            // Servis 'IResult<List<RoomDto>>' döndüğü için '.Data' üzerinden listeye ulaşıyoruz
            var result = await _roomService.GetRoomsByHotelIdAsync(hotelId);

            var hotel = await _hotelService.GetHotelByIdAsync(hotelId);
            ViewBag.HotelName = hotel?.Name;
            ViewBag.HotelId = hotelId;

            return View(result.Data ?? new List<RoomDto>());
        }

        [HttpGet("create/{hotelId}")]
        public IActionResult Create(int hotelId)
        {
            // Formda HotelId'yi gizli inputta tutmak için DTO'yu başlatıyoruz
            return View(new CreateRoomDto { HotelId = hotelId });
        }

        [HttpPost("create/{hotelId}")] // Route'dan hotelId alıyoruz
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int hotelId, CreateRoomDto dto)
        {
            // Model kontrolü
            if (!ModelState.IsValid) return View(dto);

            // 🔥 SENİN SERVİSİNE GÖRE GÜNCELLENDİ:
            // Servis 'CreateRoomByIdAsync' bekliyor ve 'hotelId' parametresi istiyor.
            var result = await _roomService.CreateRoomByIdAsync(hotelId, dto);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Yeni oda başarıyla eklendi.";
                return RedirectToAction(nameof(Index), new { hotelId = hotelId });
            }

            // Servis başarısızsa hata mesajını ekrana bas
            ModelState.AddModelError("", result.Message);
            return View(dto);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int hotelId)
        {
            var result = await _roomService.DeleteRoomAsync(id);
            if (result.IsSuccess)
                TempData["InfoMessage"] = "Oda silindi.";
            else
                TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index), new { hotelId = hotelId });
        }
    }
}