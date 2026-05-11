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

        // GET: Ekleme veya Düzenleme Formu
        [HttpGet("create/{hotelId}/{id?}")]
        public async Task<IActionResult> Create(int hotelId, int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("SuperAdmin") && user?.HotelId != hotelId) return Forbid();

            // ID yoksa (Yeni Ekleme)
            if (!id.HasValue || id == 0)
            {
                ViewBag.Title = "Yeni Oda Ekle";
                return View(new CreateRoomDto { HotelId = hotelId, IsActive = true });
            }

            // ID varsa (Düzenleme)
            var existingRoomResult = await _roomService.GetRoomByIdAsync(id.Value);
            if (!existingRoomResult.IsSuccess || existingRoomResult.Data == null)
            {
                return NotFound();
            }

            ViewBag.Title = "Oda Düzenle";
            var existingRoom = existingRoomResult.Data;

            // DTO dönüşümü yapıyoruz
            var dto = new CreateRoomDto
            {
                Id = existingRoom.Id,
                Name = existingRoom.Name ?? string.Empty,
                RoomNumber = existingRoom.RoomNumber ?? string.Empty,
                Description = existingRoom.Description,
                Price = existingRoom.Price,
                Capacity = existingRoom.Capacity,
                Size = existingRoom.Size,
                IsActive = existingRoom.IsActive
            };

            return View(dto);
        }

        // POST: Kaydetme veya Güncelleme İşlemi
        [HttpPost("create/{hotelId}/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int hotelId, int? id, CreateRoomDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Title = (id.HasValue && id.Value > 0) ? "Oda Düzenle" : "Yeni Oda Ekle";
                return View(dto);
            }

            // 1. GÜNCELLEME İŞLEMİ
            if (dto.Id > 0)
            {
                var updateDto = new UpdateRoomDto
                {
                    Id = dto.Id,
                    HotelId = dto.HotelId,
                    RoomNumber = dto.RoomNumber,
                    Name = string.IsNullOrWhiteSpace(dto.Name) ? dto.RoomNumber : dto.Name,
                    Description = dto.Description,
                    Capacity = dto.Capacity,
                    Size = dto.Size,
                    Price = dto.Price,
                    FloorNumber = dto.FloorNumber,
                    Type = dto.Type,
                    Status = dto.Status,
                    IsActive = dto.IsActive
                };

                await _roomService.UpdateRoomAsync(dto.Id, updateDto);
                TempData["SuccessMessage"] = "Oda bilgileri başarıyla güncellendi.";
                return RedirectToAction(nameof(Index), new { hotelId = dto.HotelId });
            }

            // 2. YENİ ODA EKLEME İŞLEMİ
            var existingRoomsResult = await _roomService.GetRoomsByHotelIdAsync(dto.HotelId);
            if (existingRoomsResult.IsSuccess && existingRoomsResult.Data != null)
            {
                bool roomExists = existingRoomsResult.Data.Any(r =>
                    !string.IsNullOrEmpty(r.Name) &&
                    (r.Name.Trim().Equals(dto.Name?.Trim(), StringComparison.OrdinalIgnoreCase) ||
                     r.Name.Trim().Equals(dto.RoomNumber.Trim(), StringComparison.OrdinalIgnoreCase)));

                if (roomExists)
                {
                    ViewBag.Title = "Yeni Oda Ekle";
                    TempData["ErrorMessage"] = $"Bu otelde '{dto.RoomNumber}' numaralı veya isimli bir oda zaten mevcut.";
                    return View(dto);
                }
            }

            var result = await _roomService.CreateRoomByIdAsync(dto.HotelId, dto);
            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Yeni oda başarıyla eklendi.";
                return RedirectToAction(nameof(Index), new { hotelId = dto.HotelId });
            }

            ViewBag.Title = "Yeni Oda Ekle";
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