using Core.Abstracts.Interfaces;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Data.Contexts; // ✅ Eklendi
using Microsoft.EntityFrameworkCore; // ✅ Eklendi

namespace UI.Web.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    [Route("admin/room")]
    public class AdminRoomController : Controller
    {
        private readonly IRoomService _roomService;
        private readonly IHotelService _hotelService;
        private readonly UserManager<Guest> _userManager;
        private readonly StayHubContext _context; // ✅ Eklendi

        public AdminRoomController(IRoomService roomService, IHotelService hotelService, UserManager<Guest> userManager, StayHubContext context)
        {
            _roomService = roomService;
            _hotelService = hotelService;
            _userManager = userManager;
            _context = context; // ✅ Eklendi
        }

        [HttpGet("index/{hotelId}")]
        public async Task<IActionResult> Index(int hotelId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("SuperAdmin") && user?.HotelId != hotelId) return Forbid();

            var result = await _roomService.GetRoomsByHotelIdAsync(hotelId);
            var hotel = await _hotelService.GetHotelByIdAsync(hotelId);
            ViewBag.HotelName = hotel?.Name;
            ViewBag.HotelId = hotelId;

            return View(result.Data ?? new List<RoomDto>());
        }

        [HttpGet("create/{hotelId}/{id?}")]
        public async Task<IActionResult> Create(int hotelId, int? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (!User.IsInRole("SuperAdmin") && user?.HotelId != hotelId) return Forbid();

            if (!id.HasValue || id == 0)
            {
                ViewBag.Title = "Yeni Oda Ekle";
                return View(new CreateRoomDto { HotelId = hotelId, IsActive = true });
            }

            var existingRoomResult = await _roomService.GetRoomByIdAsync(id.Value);
            if (!existingRoomResult.IsSuccess || existingRoomResult.Data == null) return NotFound();

            ViewBag.Title = "Oda Düzenle";
            var existingRoom = existingRoomResult.Data;

            var dto = new CreateRoomDto
            {
                Id = existingRoom.Id,
                HotelId = hotelId, // ✅ HotelId eklendi
                Name = existingRoom.Name ?? string.Empty,
                RoomNumber = existingRoom.RoomNumber ?? string.Empty,
                Description = existingRoom.Description,
                Price = existingRoom.Price,
                Capacity = existingRoom.Capacity,
                Size = existingRoom.Size,
                IsActive = existingRoom.IsActive,
               
            };

            return View(dto);
        }

        [HttpPost("create/{hotelId}/{id?}")]
        [ValidateAntiForgeryToken]
        // ✅ 'List<IFormFile> Images' parametresi eklendi (HTML'deki name="Images" ile aynı olmalı)
        public async Task<IActionResult> Create(int hotelId, int? id, CreateRoomDto dto, List<IFormFile> Images)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Title = (id.HasValue && id.Value > 0) ? "Oda Düzenle" : "Yeni Oda Ekle";
                return View(dto);
            }

            int currentRoomId = 0;

            // 1. GÜNCELLEME VEYA EKLEME
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
                currentRoomId = dto.Id;
                TempData["SuccessMessage"] = "Oda bilgileri güncellendi.";
            }
            else
            {
                // Var olan generic dönüş tipini (Data barındıranı) kullandığınızdan emin olun
                var createResult = await _roomService.CreateRoomByIdAsync(dto.HotelId, dto);

                if (!createResult.IsSuccess)
                {
                    ViewBag.Title = "Yeni Oda Ekle";
                    TempData["ErrorMessage"] = createResult.Message;
                    return View(dto);
                }

                // ✅ ÇÖZÜM: createResult.Data üzerinden Id'ye erişiyoruz
                // Eğer hala hata veriyorsa, IRoomService içindeki metodun dönüş tipinin 
                // IDataResult<RoomDto> olduğundan emin olun.
                currentRoomId = createResult.Data.Id;
                TempData["SuccessMessage"] = "Yeni oda eklendi.";
            }

            // ✅ 2. GÖRSEL YÜKLEME VE ENTITY KAYIT MANTIĞI
            if (Images != null && Images.Count > 0)
            {
                string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "rooms");
                if (!Directory.Exists(uploadFolder)) Directory.CreateDirectory(uploadFolder);

                int order = 1;
                foreach (var file in Images)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // ✅ Senin RoomImage entity yapına tam uyumlu kayıt
                    var roomImage = new RoomImage
                    {
                        RoomId = currentRoomId,
                        ImageUrl = "/uploads/rooms/" + fileName,
                        ImageName = file.FileName,
                        DisplayOrder = order,
                        IsPrimary = (order == 1),
                        UploadedAt = true, // Entity'deki bool alan
                        IsDeleted = false
                    };

                    _context.RoomImages.Add(roomImage);
                    order++;
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { hotelId = hotelId });
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int hotelId)
        {
            var result = await _roomService.DeleteRoomAsync(id);
            if (result.IsSuccess)
                TempData["SuccessMessage"] = "Oda silindi.";
            else
                TempData["ErrorMessage"] = result.Message;

            return RedirectToAction(nameof(Index), new { hotelId = hotelId });
        }
    }
}