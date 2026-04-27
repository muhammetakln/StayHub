using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace UI.Web.Controllers
{
    [Authorize(Roles = "Admin")]  // ✅ SADECE ADMIN
    [Route("admin/hotel")]
    public class AdminHotelController : Controller
    {
        private readonly IHotelService _hotelService;
        private readonly ILogger<AdminHotelController> _logger;

        public AdminHotelController(IHotelService hotelService, ILogger<AdminHotelController> logger)
        {
            _hotelService = hotelService;
            _logger = logger;
        }

        // GET: /admin/hotel
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var hotels = await _hotelService.GetHotelsAsync();
                return View(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Oteller getirme hatası");
                return View(new List<HotelDto>());
            }
        }

        // GET: /admin/hotel/details/5
        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var hotel = await _hotelService.GetHotelByIdAsync(id);
                if (hotel == null)
                    return NotFound();

                return View(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otel detayı getirme hatası");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /admin/hotel/create
        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /admin/hotel/create
        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateHotelDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Create form validation başarısız");
                    return View(dto);
                }

                await _hotelService.CreateHotelAsync(dto);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otel oluşturma hatası");
                ModelState.AddModelError("", "Otel oluşturulurken hata oluştu");
                return View(dto);
            }
        }

        // GET: /admin/hotel/edit/5
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var hotel = await _hotelService.GetHotelByIdAsync(id);
                if (hotel == null)
                    return NotFound();

                var updateDto = new UpdateHotelDto
                {
                    Name = hotel.Name,
                    
                    Address = hotel.Address,
                    PhoneNumber = hotel.PhoneNumber,
                    Email = hotel.Email,
                    
                    Description = hotel.Description,
                    
                };
                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otel düzenleme sayfası hatası");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/hotel/edit/5
        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateHotelDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Edit form validation başarısız");
                    return View(dto);
                }

                await _hotelService.UpdateHotelAsync(id, dto);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otel güncelleme hatası");
                ModelState.AddModelError("", "Otel güncellenirken hata oluştu");
                return View(dto);
            }
        }

        // GET: /admin/hotel/delete/5
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var hotel = await _hotelService.GetHotelByIdAsync(id);
                if (hotel == null)
                    return NotFound();

                return View(hotel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otel silme sayfası hatası");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /admin/hotel/delete/5
        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _hotelService.DeleteHotelAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otel silme hatası");
                return RedirectToAction(nameof(Index));
            }
        }
    }
}