using Business.Mappings;
using Business.Services;
using Core.Abstracts;
using Core.Abstracts.Interfaces;
using Core.Abstracts.IServices;
using Core.Concretes.Entities;
using Data;
using Data.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Business
{
    /// <summary>
    /// Inversion of Control Container
    /// Dependency Injection ayarlaması
    /// SQLite için uyarlanmış (FINAL)
    /// </summary>
    public static class IOC
    {
        public static IServiceCollection AddGuestServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<StayHubContext>(opt =>
                 opt.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
            services.AddIdentity<Guest, IdentityRole<int>>()
                .AddEntityFrameworkStores<StayHubContext>()
                .AddDefaultTokenProviders();
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AuthProfile>();
                cfg.AddProfile<HotelProfile>();
                cfg.AddProfile<RoomProfile>();
                cfg.AddProfile<AmenityProfile>();
                cfg.AddProfile<ReservationProfile>();
                cfg.AddProfile<ReviewProfile>();
                cfg.AddProfile<PaymentProfile>();
            });

            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IHotelService, HotelService>();
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IAmenityService, AmenityService>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IReviewService, ReviewService>();
            services.AddScoped<IPaymentService, PaymentService>();
            return services;


        }

    }
}