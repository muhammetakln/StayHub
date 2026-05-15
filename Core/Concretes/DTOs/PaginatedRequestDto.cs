using System.ComponentModel.DataAnnotations;

public class PaginatedRequestDto
{
    [Display(Name = "Sayfa Numarası")]
    [Range(1, int.MaxValue, ErrorMessage = "Sayfa numarası 1'den büyük olmalıdır")]
    public int PageNumber { get; set; } = 1;

    [Display(Name = "Sayfa Boyutu")]
    [Range(1, 100, ErrorMessage = "Sayfa boyutu 1-100 arasında olmalıdır")]
    public int PageSize { get; set; } = 10;

    [Display(Name = "Arama")]
    [StringLength(100, ErrorMessage = "Arama 100 karakteri geçemez")]
    public string? SearchTerm { get; set; }

    [Display(Name = "Sıralama")]
    [StringLength(50, ErrorMessage = "Sıralama 50 karakteri geçemez")]
    public string? SortBy { get; set; } = "CreatedAt";

    [Display(Name = "Azalan Sıralama")]
    public bool SortDescending { get; set; } = true;


    /// <summary>
    /// Paged Result DTO
    /// Generic paginated response
    /// </summary>
    public class PagedResultDto<T>
    {
        [Display(Name = "Veriler")]
        public IEnumerable<T> Items { get; set; } = new List<T>();

        [Display(Name = "Toplam Kayıt")]
        public int TotalCount { get; set; }

        [Display(Name = "Sayfa Numarası")]
        public int PageNumber { get; set; }

        [Display(Name = "Sayfa Boyutu")]
        public int PageSize { get; set; }

        [Display(Name = "Toplam Sayfa")]
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        [Display(Name = "Önceki Sayfa Var mı")]
        public bool HasPreviousPage => PageNumber > 1;

        [Display(Name = "Sonraki Sayfa Var mı")]
        public bool HasNextPage => PageNumber < TotalPages;

        [Display(Name = "Timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Filter DTO
    /// Advanced filtering için
    /// </summary>
    public class FilterDto
    {
        [Display(Name = "Arama")]
        [StringLength(100)]
        public string? SearchTerm { get; set; }

        [Display(Name = "Filtreler")]
        public Dictionary<string, string>? Filters { get; set; } = new Dictionary<string, string>();

        [Display(Name = "Sıralama")]
        [StringLength(50)]
        public string? SortBy { get; set; }

        [Display(Name = "Azalan")]
        public bool SortDescending { get; set; } = true;
    }
}
