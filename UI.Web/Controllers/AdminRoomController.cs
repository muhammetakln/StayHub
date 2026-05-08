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

        [HttpPost("create/{hotelId}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int hotelId, CreateRoomDto dto)
        {
            // 1. Standart model doğrulaması
            if (!ModelState.IsValid) return View(dto);

            // 🛡️ 2. KRİTİK KONTROL: Çift Oda Kaydını Engelleme
            // Oteldeki mevcut odaları çekiyoruz
            var existingRoomsResult = await _roomService.GetRoomsByHotelIdAsync(hotelId);

            if (existingRoomsResult.IsSuccess && existingRoomsResult.Data != null)
            {
                // Boşlukları silip büyük/küçük harf duyarsız kontrol yapıyoruz
                bool roomExists = existingRoomsResult.Data.Any(r =>
                    !string.IsNullOrEmpty(r.Name) &&
                    r.Name.Trim().Equals(dto.Name.Trim(), StringComparison.OrdinalIgnoreCase));

                if (roomExists)
                {
                    // Eğer oda varsa Toastr/SweetAlert ile hata fırlat ve işlemi durdur
                    TempData["ErrorMessage"] = $"Bu otelde '{dto.Name}' adında bir oda zaten mevcut. Lütfen farklı bir numara/isim girin.";
                    return View(dto);
                }
            }

            // 3. Her şey kolundaysa kaydı gerçekleştir
            var result = await _roomService.CreateRoomByIdAsync(hotelId, dto);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Yeni oda başarıyla eklendi.";
                return RedirectToAction(nameof(Index), new { hotelId = hotelId });
            }

            // Servis başarısızsa hata mesajını ekrana bas
            TempData["ErrorMessage"] = result.Message;
            return View(dto);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int hotelId)
        {
            var result = await _roomService.DeleteRoomAsync(id);
            if (result.IsSuccess)
                TempData["SuccessMessage"] = "Oda başarıyla silindi.";
            else
                TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index), new { hotelId = hotelId });
        }
    }
}