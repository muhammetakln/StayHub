namespace Core.Concretes.DTOs
{
   
    
        public class HotelSearchFilterDto
        {
            public string? City { get; set; }
            public string? Country { get; set; }
            public string? SearchKeyword { get; set; } // Otel adı veya açıklamasında arama yapmak için

            public decimal? MinPrice { get; set; }
            public decimal? MaxPrice { get; set; }

            public int? MinStarRating { get; set; }
            public string? SortBy { get; set; } // Örn: "price_asc", "rating_desc"

            // Pagination (Sayfalama) için standart alanlar
            public int PageNumber { get; set; } = 1;
            public int PageSize { get; set; } = 10;
        }
    
}