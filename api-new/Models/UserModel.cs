public class UserModel
{
    public int ID { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Role { get; set; }
    public string CreatedAt { get; set; }
    public string BirthYear { get; set; }
    public bool Active { get; set; }

    public UserModel(int id, string username, string password, string name, string email,
    string phone, string role, string createdAt, string birthYear, int active)
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
