using OS_Kursavaya.Infrastructure;

namespace OS_Kursavaya.Models;

// класс описывающий пользователя
public class User
{
    // счётчик для идентификаторов пользователей
    private static int Cur_id = 0;

    // идентификатор
    public int Id { get; private set; }

    // имя
    private string _name = null!;
    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException("Неверно указан логин пользователя");
            _name = value;
        }
    }

    // пароль
    private string _password = null!;
    public string Password
    {
        get => _password;
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException("Неверно указан пароль пользователя");
            _password = value;
        }
    }

    // конструкторы
    public User() : this("admin", "781227xyZ")
    { }

    public User(string name, string password)
    {
        Id = Cur_id++;
        Name = name;
        Password = Utils.GenerateHash(password);
    }
}
