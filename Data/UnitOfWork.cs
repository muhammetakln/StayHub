using Core.Abstracts;
using Core.Abstracts.IRepositories;
using Core.Concretes.Entities;
using Data.Contexts;
using Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Data;
using System.Threading.Tasks;
using Utils.Responses;

namespace Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StayHubContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(StayHubContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // ========== REPOSITORIES ==========
        private IHotelRepository? _hotelRepository;
        public IHotelRepository HotelRepository => _hotelRepository ??= new HotelRepository(_context);

        private IAmenityRepository? _amenityRepository;
        public IAmenityRepository AmenityRepository => _amenityRepository ??= new AmenityRepository(_context);

        private IAddOnServiceRepository? _addOnServiceRepository;
        public IAddOnServiceRepository AddOnServiceRepository => _addOnServiceRepository ??= new AddOnServiceRepository(_context);

        private IRoomImageRepository? _roomImageRepository;
        public IRoomImageRepository RoomImageRepository => _roomImageRepository ??= new RoomImageRepository(_context);

        private IRoomRepository? _roomRepository;
        public IRoomRepository RoomRepository => _roomRepository ??= new RoomRepository(_context);

        private IPaymentRepository? _paymentRepository;
        public IPaymentRepository PaymentRepository => _paymentRepository ??= new PaymentRepository(_context);

        private IReviewRepository? _reviewRepository;
        public IReviewRepository ReviewRepository => _reviewRepository ??= new ReviewRepository(_context);

        private IReservationAddOnServiceRepository? _reservationAddOnServiceRepository;
        public IReservationAddOnServiceRepository ReservationAddOnServiceRepository => _reservationAddOnServiceRepository ??= new ReservationAddOnServiceRepository(_context);

        private IReservationRepository? _reservationRepository;
        public IReservationRepository ReservationRepository => _reservationRepository ??= new ReservationRepository(_context);

        // ========== METHOD 1: BeginTransactionAsync ==========
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        // ========== METHOD 2: CommitTransactionAsync ==========
        public async Task CommitTransactionAsync()
        {
            try
            {
                await _transaction?.CommitAsync();
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        // ========== METHOD 3: RollbackAsync ==========
        public async Task RollbackAsync()
        {
            try
            {
                await _transaction?.RollbackAsync();
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        // ========== METHOD 4: CommitAsync ==========
        public async Task<IResult> CommitAsync()
        {
            try
            {
                var changes = await _context.SaveChangesAsync();
                return Result.Success($"{changes} kayıt başarıyla işlendi", 200);
            }
            catch (DbUpdateException ex)
            {
                return Result.Failure($"Veritabanı hatası: {ex.Message}", ex.Message, 400);
            }
            catch (OperationCanceledException ex)
            {
                return Result.Failure($"İşlem zaman aşımına uğradı: {ex.Message}", ex.Message, 408);
            }
            catch (Exception ex)
            {
                return Result.ServerError($"Beklenmeyen hata: {ex.Message}", ex.Message);
            }
        }

        // ✅ DÜZELTME: Interface'iniz ile birebir aynı yapıldı (Task<int> yerine Task dönüyor)
        // ========== METHOD 5: SaveChangesAsync ==========
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        // ========== METHOD 6: CloseConnectionAsync ==========
        public async Task CloseConnectionAsync()
        {
            if (_context.Database.GetDbConnection().State == ConnectionState.Open)
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        // ========== METHOD 7: DisposeAsync ==========
        public async ValueTask DisposeAsync()
        {
            try
            {
                if (_transaction != null)
                    await _transaction.DisposeAsync();
            }
            catch { }

            try
            {
                if (_context != null)
                    await _context.DisposeAsync();
            }
            catch { }

            GC.SuppressFinalize(this);
        }
    }
}