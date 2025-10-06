public class UserModel
{
    public int ID { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public Enum Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public int BirthYear { get; set; }
    public bool Active { get; set; }

    public UserModel(int id, string username, string password, string name, string email,
    string phone, Enum role, DateTime createdAt, int birthYear, int active)
    {
        ID = id;
        UserName = username;
        Password = password;
        Name = name;
        Email = email;
        Phone = phone;
        Role = role;
        CreatedAt = createdAt;
        BirthYear = birthYear;
        Active = Convert.ToBoolean(active);
    }
}
