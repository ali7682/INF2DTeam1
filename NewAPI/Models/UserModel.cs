public sealed class UserModel
{
    public int Id { get; init; }
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
    public DateTime? CreatedAt { get; init; }
    public short? BirthYear { get; set; }
    public bool Active { get; set; }
    public bool IsAdmin()
    {
        return Role == "ADMIN";
    }
    public async Task<bool> Update()
    {
        return await UserAccess.UpdateUserAsync(this);
    }

    public UserModel()
    {
        CreatedAt = DateTime.UtcNow;
    }
}