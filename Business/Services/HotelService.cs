using AutoMapper;
using Core.Abstracts;
using Core.Abstracts.IServices;
using Core.Concretes.DTOs;
using Core.Concretes.Entities;
using Core.Concretes.Enum;
using Data.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Business.Services
{
    public class HotelService : IHotelService
    {
        private readonly StayHubContext _context;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<HotelService> _logger;
        private readonly UserManager<Guest> _userManager;
        private readonly IPasswordHasher<Hotel> _passwordHasher;

        public HotelService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            StayHubContext context,
            ILogger<HotelService> logger,
            UserManager<Guest> userManager,
            IPasswordHasher<Hotel> passwordHasher)
        {
            _context = context;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _userManager = userManager;
            _passwordHasher = passwordHasher;
        }

        public async Task<int> CreateHotelAsync(CreateHotelDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                var hotel = _mapper.Map<Hotel>(dto);

                if (string.IsNullOrEmpty(hotel.Description) && !string.IsNullOrEmpty(dto.Description))
                {
                    hotel.Description = dto.Description;
                }

                if (!string.IsNullOrEmpty(dto.HotelPassword))
                {
                    hotel.HotelPassword = _passwordHasher.HashPassword(hotel, dto.HotelPassword);
                }

                hotel.CreatedAt = DateTime.UtcNow;
                hotel.IsActive = true;
                hotel.IsDeleted = false;

                await _unitOfWork.HotelRepository.AddAsync(hotel);
                await _unitOfWork.SaveChangesAsync();

                var hotelAdmin = new Guest
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.Name,
                    LastName = "Yöneticisi",
                    HotelId = hotel.Id,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    IdentificationNumber = "00000000000",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Address = dto.Address,
                    Country = dto.Country
                };

                var userResult = await _userManager.CreateAsync(hotelAdmin, dto.HotelPassword);

                if (userResult.Succeeded)
                {
                    await _userManager.AddToRoleAsync(hotelAdmin, "Admin");

                    await _unitOfWork.CommitTransactionAsync();
                    return hotel.Id;
                }
                else
                {
                    await _unitOfWork.RollbackAsync();
                    var errors = string.Join(", ", userResult.Errors.Select(e => e.Description));
                    _logger.LogError("Otel kullanıcısı oluşturulamadı: {Errors}", errors);
                    return 0;
                }
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "CreateHotelAsync hatası");
                return 0;
            }
        }

        public async Task DeleteHotelAsync(int id)
        {
            try
            {
                var hotel = await _unitOfWork.HotelRepository.GetFirstAsync(h => h.Id == id);
                if (hotel != null)
                {
                    hotel.IsDeleted = true;
                    hotel.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.HotelRepository.UpdateAsync(hotel);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteHotelAsync hatası");
            }
        }

        public async Task<HotelDetailDto?> GetHotelByIdAsync(int id)
        {
            try
            {
                var hotel = await _context.Hotels
                    .AsNoTracking()
                    .Include(h => h.Rooms)
                    .Include(h => h.Reviews).ThenInclude(r => r.Guest)
                    .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

                // ✅ Amenities ve AddOnServices ayrı sorguda yükle (Cartesian Product sorunu önle)
                if (hotel != null)
                {
                    hotel.Amenities = await _context.Amenities
                        .AsNoTracking()
                        .Where(a => a.HotelId == hotel.Id && !a.IsDeleted)
                        .ToListAsync();

                    hotel.AddOnServices = await _context.AddOnServices
                        .AsNoTracking()
                        .Where(s => s.HotelId == hotel.Id && !s.IsDeleted)
                        .ToListAsync();
                }

                if (hotel == null) return null;

                if (hotel.Rooms != null && hotel.Rooms.Any())
                {
                    var roomIds = hotel.Rooms.Where(r => !r.IsDeleted).Select(r => r.Id).ToList();
                    var images = await _unitOfWork.RoomImageRepository.GetManyAsync(img => roomIds.Contains(img.RoomId) && !img.IsDeleted);

                    hotel.Rooms = hotel.Rooms.Where(r => !r.IsDeleted).ToList();

                    foreach (var room in hotel.Rooms)
                    {
                        room.RoomImage = images.Where(img => img.RoomId == room.Id).ToList();
                    }
                }

                var dto = _mapper.Map<HotelDetailDto>(hotel);
                var activeReviews = hotel.Reviews?.Where(r => !r.IsDeleted).ToList();

                if (activeReviews != null && activeReviews.Any())
                {
                    dto.AverageRating = Math.Round(activeReviews.Average(r => (double)r.Rating), 1);
                    dto.ReviewCount = activeReviews.Count;
                }

                return dto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"GetHotelByIdAsync hatası: {id}");
                return null;
            }
        }

        public async Task<List<HotelDto>> GetHotelsAsync()
        {
            try
            {
                var hotels = await _unitOfWork.HotelRepository.GetAll()
                    .AsNoTracking()
                    .Include(h => h.Rooms)
                    .Include(h => h.Reviews)
                    .Where(h => !h.IsDeleted && h.IsActive)
                    .ToListAsync();

                return CalculateDynamicRatings(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetHotelsAsync hatası");
                return new List<HotelDto>();
            }
        }

        public async Task<List<HotelDto>> FilterHotelsAsync(HotelSearchFilterDto dto)
        {
            try
            {
                var query = _unitOfWork.HotelRepository.GetAll()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(h => h.Rooms)
                    .Include(h => h.Reviews)
                    .Where(h => !h.IsDeleted);

                if (!string.IsNullOrEmpty(dto.SearchKeyword))
                {
                    query = query.Where(h => h.Name.Contains(dto.SearchKeyword) || h.City.Contains(dto.SearchKeyword));
                }

                if (!string.IsNullOrEmpty(dto.City)) query = query.Where(h => h.City == dto.City);
                if (dto.MinStarRating.HasValue) query = query.Where(h => h.StarRating >= dto.MinStarRating.Value);

                var hotels = await query
                    .Skip((dto.PageNumber - 1) * dto.PageSize)
                    .Take(dto.PageSize)
                    .ToListAsync();

                return CalculateDynamicRatings(hotels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FilterHotelsAsync hatası");
                return new List<HotelDto>();
            }
        }

        public async Task<List<HotelDto>> GetHotelsByCityAsync(string city)
        {
            var filter = new HotelSearchFilterDto { City = city };
            return await FilterHotelsAsync(filter);
        }

        public async Task<List<HotelDto>> GetHotelsByRatingAsync(decimal minRating)
        {
            var hotels = await GetHotelsAsync();
            return hotels.Where(h => (decimal)h.AverageRating >= minRating).ToList();
        }

        public async Task<bool> IsHotelExistsAsync(int id)
        {
            return await _unitOfWork.HotelRepository.AnyAsync(h => h.Id == id && !h.IsDeleted);
        }

        public async Task<List<HotelDto>> SearchHotelsAsync(string searchTerm)
        {
            var filter = new HotelSearchFilterDto { SearchKeyword = searchTerm };
            return await FilterHotelsAsync(filter);
        }

        public async Task UpdateHotelAsync(int id, UpdateHotelDto dto)
        {
            try
            {
                await _unitOfWork.BeginTransactionAsync();

                // ✅ DÜZELTME: ChangeTracker.Clear() KALDIR
                var hotel = await _context.Hotels
                    .Include(h => h.Amenities)
                    .Include(h => h.AddOnServices)
                    .FirstOrDefaultAsync(h => h.Id == id && !h.IsDeleted);

                if (hotel == null) return;

                hotel.Name = dto.Name;
                hotel.Address = dto.Address;
                hotel.PhoneNumber = dto.PhoneNumber;
                hotel.Email = dto.Email;
                hotel.City = dto.City;
                hotel.Country = dto.Country;
                hotel.StarRating = dto.StarRating;
                hotel.IsActive = dto.IsActive;
                hotel.CheckInTime = dto.CheckInTime;
                hotel.CheckOutTime = dto.CheckOutTime;

                if (!string.IsNullOrEmpty(dto.CoverImageUrl)) hotel.CoverImageUrl = dto.CoverImageUrl;
                if (!string.IsNullOrEmpty(dto.Description)) hotel.Description = dto.Description;

                if (!string.IsNullOrEmpty(dto.HotelPassword))
                {
                    hotel.HotelPassword = _passwordHasher.HashPassword(hotel, dto.HotelPassword);
                }

                hotel.UpdatedAt = DateTime.UtcNow;

                // 🚨 OLANAKLAR (AMENITIES) İÇİN AKILLI BİRLEŞTİRME (DÜZELTILMIŞ) 🚨
                if (hotel.Amenities == null) hotel.Amenities = new List<Amenity>();

                var incomingAmenityIds = dto.Amenities?.Where(a => a.Id > 0).Select(a => a.Id).ToList() ?? new List<int>();

                // 1. Ekrandan silinenleri bul ve IsDeleted = true yap (Soft Delete)
                var amenitiesToRemove = hotel.Amenities.Where(a => !incomingAmenityIds.Contains(a.Id) && !a.IsDeleted).ToList();
                foreach (var item in amenitiesToRemove)
                {
                    item.IsDeleted = true;
                    item.UpdatedAt = DateTime.UtcNow;
                }

                // 2. Mevcutları güncelle, silinmiş olanları geri al, yenileri ekle
                if (dto.Amenities != null)
                {
                    foreach (var amenityDto in dto.Amenities)
                    {
                        if (amenityDto.Id > 0)
                        {
                            var existing = hotel.Amenities.FirstOrDefault(a => a.Id == amenityDto.Id);
                            if (existing != null)
                            {
                                existing.Name = amenityDto.Name;
                                existing.IconUrl = amenityDto.IconUrl;
                                existing.Description = amenityDto.Description;
                                existing.UpdatedAt = DateTime.UtcNow;
                                existing.IsDeleted = false; // ✅ Silinmiş olanı geri al
                            }
                        }
                        else
                        {
                            // ✅ DÜZELTME: Boş satırları ekleme! Sadece gerçek veri ekleme
                            if (!string.IsNullOrWhiteSpace(amenityDto.Name))
                            {
                                // ✅ DÜZELTME 2: DbContext'e doğrudan Add et
                                _context.Amenities.Add(new Amenity
                                {
                                    Name = amenityDto.Name,
                                    IconUrl = amenityDto.IconUrl,
                                    Description = amenityDto.Description,
                                    CreatedAt = DateTime.UtcNow,
                                    HotelId = hotel.Id,
                                    IsDeleted = false
                                });
                            }
                        }
                    }
                }

                // 🚨 EK HİZMETLER (ADD-ON SERVICES) İÇİN AKILLI BİRLEŞTİRME (DÜZELTILMIŞ) 🚨
                if (hotel.AddOnServices == null) hotel.AddOnServices = new List<AddOnService>();

                var incomingServiceIds = dto.AddOnServices?.Where(s => s.Id > 0).Select(s => s.Id).ToList() ?? new List<int>();

                // 1. Ekrandan silinenleri bul ve IsDeleted = true yap
                var servicesToRemove = hotel.AddOnServices.Where(s => !incomingServiceIds.Contains(s.Id) && !s.IsDeleted).ToList();
                foreach (var item in servicesToRemove)
                {
                    item.IsDeleted = true;
                    item.UpdatedAt = DateTime.UtcNow;
                }

                // 2. Mevcutları güncelle, silinmiş olanları geri al, yenileri ekle
                if (dto.AddOnServices != null)
                {
                    foreach (var serviceDto in dto.AddOnServices)
                    {
                        if (serviceDto.Id > 0)
                        {
                            var existing = hotel.AddOnServices.FirstOrDefault(s => s.Id == serviceDto.Id);
                            if (existing != null)
                            {
                                existing.Name = serviceDto.Name;
                                existing.Price = serviceDto.Price;
                                existing.Unit = string.IsNullOrEmpty(serviceDto.Unit) ? "Adet" : serviceDto.Unit;
                                existing.UpdatedAt = DateTime.UtcNow;
                                existing.IsDeleted = false; // ✅ Silinmiş olanı geri al
                            }
                        }
                        else
                        {
                            // ✅ DÜZELTME: Boş satırları ekleme! Sadece gerçek veri ekleme
                            if (!string.IsNullOrWhiteSpace(serviceDto.Name) && serviceDto.Price > 0)
                            {
                                // ✅ DÜZELTME 2: DbContext'e doğrudan Add et
                                _context.AddOnServices.Add(new AddOnService
                                {
                                    Name = serviceDto.Name,
                                    Price = serviceDto.Price,
                                    Unit = string.IsNullOrEmpty(serviceDto.Unit) ? "Adet" : serviceDto.Unit,
                                    IsActive = true,
                                    CreatedAt = DateTime.UtcNow,
                                    HotelId = hotel.Id,
                                    IsDeleted = false
                                });
                            }
                        }
                    }
                }

                _context.Hotels.Update(hotel);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "UpdateHotelAsync hatası");
            }
        }

        private List<HotelDto> CalculateDynamicRatings(List<Hotel> hotels)
        {
            var dtos = _mapper.Map<List<HotelDto>>(hotels);
            foreach (var dto in dtos)
            {
                var entity = hotels.First(h => h.Id == dto.Id);
                var activeReviews = entity.Reviews?.Where(r => !r.IsDeleted).ToList();
                dto.AverageRating = (activeReviews != null && activeReviews.Any())
                    ? Math.Round(activeReviews.Average(r => (double)r.Rating), 1)
                    : 0;
            }
            return dtos;
        }

        public async Task<AdminDashboardDto> GetDashboardSummaryAsync()
        {
            try
            {
                var stats = new DashboardStatsDto
                {
                    TotalHotels = await _context.Hotels.CountAsync(h => !h.IsDeleted),
                    TotalReservations = await _context.Reservations.CountAsync(r => !r.IsDeleted),
                    TotalGuests = await _context.Users.CountAsync(),
                    TotalRevenue = await _context.Reservations
                        .Where(r => r.Status == ReservationStatus.Confirmed && !r.IsDeleted)
                        .SumAsync(r => r.TotalPrice),
                    PendingReservations = await _context.Reservations
                        .CountAsync(r => r.Status == ReservationStatus.Pending && !r.IsDeleted),
                    AverageRating = await _context.Reviews
                        .Where(r => !r.IsDeleted)
                        .Select(r => (double?)r.Rating)
                        .AverageAsync() ?? 0
                };

                var recentReservationsRaw = await _context.Reservations
                    .AsNoTracking()
                    .Include(r => r.Room).ThenInclude(rm => rm.Hotel)
                    .Where(r => !r.IsDeleted)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                var recentReservations = recentReservationsRaw.Select(r => new ReservationDetailDto
                {
                    Id = r.Id,
                    HotelName = r.Room?.Hotel?.Name ?? "N/A",
                    RoomNumber = r.Room?.RoomNumber ?? "N/A",
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    Status = r.Status.ToString(),
                    GrandTotal = r.TotalPrice
                }).ToList();

                var topGuests = await _context.Users
                    .AsNoTracking()
                    .OrderByDescending(g => _context.Reservations.Count(r => r.GuestId == g.Id && !r.IsDeleted))
                    .Take(5)
                    .Select(g => new GuestSummaryDto
                    {
                        Id = g.Id,
                        FullName = g.UserName ?? "Bilinmeyen",
                        Email = g.Email ?? "",
                        PhoneNumber = g.PhoneNumber ?? "",
                        JoinDate = DateTime.UtcNow,
                        ReservationCount = _context.Reservations.Count(r => r.GuestId == g.Id && !r.IsDeleted)
                    })
                    .ToListAsync();

                var lastSevenDays = Enumerable.Range(0, 7)
                    .Select(i => DateTime.UtcNow.Date.AddDays(-i))
                    .ToList();

                var dailyRevenueList = new List<RevenueDto>();
                foreach (var date in lastSevenDays)
                {
                    var dayData = await _context.Reservations
                        .Where(r => r.CreatedAt.Date == date && r.Status == ReservationStatus.Confirmed && !r.IsDeleted)
                        .ToListAsync();

                    dailyRevenueList.Add(new RevenueDto
                    {
                        Date = date,
                        Amount = dayData.Sum(x => x.TotalPrice),
                        ReservationCount = dayData.Count
                    });
                }

                var hotelsData = await GetHotelsAsync();

                return new AdminDashboardDto
                {
                    Stats = stats,
                    RecentReservations = recentReservations,
                    Guests = topGuests,
                    DailyRevenue = dailyRevenueList.OrderBy(x => x.Date).ToList(),
                    Hotels = hotelsData.Take(5).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Dashboard verileri getirilirken hata oluştu.");
                return new AdminDashboardDto();
            }
        }
    }
}