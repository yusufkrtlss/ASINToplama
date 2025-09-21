namespace ASINToplama_EntityLayer.Dtos
{
    public sealed class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = "";
        public string? FullName { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public bool IsActive { get; set; }
    }

    public sealed class UserCreateRequest
    {
        public string Email { get; set; } = "";
        public string? FullName { get; set; }
        public string? Password { get; set; } // PLAIN gelir
    }

    public sealed class UserUpdateRequest
    {
        public string? FullName { get; set; }
        public bool? IsActive { get; set; }
        public string? Password { get; set; } // PLAIN gelir
    }

    public sealed class PagedResult<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    }
}
