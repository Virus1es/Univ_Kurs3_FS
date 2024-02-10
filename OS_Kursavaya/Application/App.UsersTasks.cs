using System.Text;
using Newtonsoft.Json;
using OS_Kursavaya.Models;
using OS_Kursavaya.Infrastructure;

namespace OS_Kursavaya.Application;

// Операции с пользователями
public partial class App
{
    // чтение или создание начальных учётных записей
    public void ReadOrCrateUsersFile()
    {
        // если не нашли файл с пользователями создаём 3 начальных пользователя
        // 1 админ и 2 обычных пользователя
        if (File.Exists(_usersFile))
            Deserialize();
        else
        {
            _users = new List<User> {
                        new User("root",   "781227xyZ"),
                        new User("user1",  "1234567"),
                        new User("trojan", "87654321")
                    };
            Serialize();
        }
    }

    // вход в учётную запись пользователя
    public bool TryEnterUser()
    {
        User selectUser = null!;
        string login = "";

        while (true)
        {
            // просим ввести логин
            Console.WriteLine("Для выхода нажмите Enter\nВведите логин:");

            // удаляем ненужные пробелы в начале и конце
            login = Console.ReadLine()!.Trim();

            // даём возможность выйти из процесса входа
            if (string.IsNullOrEmpty(login))
                return false;

            // проверяем есть ли такой пользователь
            selectUser = _users.FirstOrDefault(u => u.Name == login);

            // если такой пользователь есть проходим дальше
            if (selectUser != null) break;

            // если введён не существующий пользователь уведомляем
            // и очищаем экран
            Console.WriteLine("\nТакого пользователя не существует\n");

            Thread.Sleep(3000);

            Console.Clear();
        }

        // проверка пароля
        if (!CheckPassword(selectUser)) return false;
        
        // устанавливаем активного пользователя
        _activeUser = selectUser;

        return true;
    }

    // добавить пользователя
    public void AddUser()
    {
        string login = "", password = "";

        while (true)
        {
            // просим ввести логин
            Console.WriteLine("Для выхода нажмите Enter\nВведите логин нового пользователя:");

            login = Console.ReadLine()!;

            // даём возможность выйти из процесса входа
            if (string.IsNullOrEmpty(login))
                return;

            // если пользователя с таким именем до этого не было выходим
            if(!_users.Any(u => u.Name == login))
                break;

            // иначе повтор ввода
            Console.WriteLine("\nПользователь с таким именем уже существует в системе\n");

            Thread.Sleep(3000);

            Console.Clear();
        }

        while (true)
        {
            // просим ввести пароль
            Console.WriteLine("\n\nДля выхода нажмите Enter\nВведите пароль:");

            password = Console.ReadLine()!;

            // даём возможность выйти из процесса входа
            if (string.IsNullOrEmpty(password))
                return;

            if (password.Length > 6)
                break;

            Console.WriteLine("\nПароль слишком короткий\n");
        }

        // добавление нового пользователя в список
        _users.Add(new User(login, password));

        // сохранение (сериализация) списка пользователей
        Serialize();

        return;
    }


    // удалить пользователя
    public void DeleteUser()
    {
        User selectUser = null!;

        while (true)
        {
            // просим ввести логин
            Console.WriteLine("Для выхода нажмите Enter\nВведите логин пользователя для удаления:");

            string login = Console.ReadLine()!;

            // даём возможность выйти из процесса входа
            if (string.IsNullOrEmpty(login))
                return;

            // проверяем есть ли такой пользователь
            selectUser = _users.FirstOrDefault(u => u.Name == login);

            // запрет на удаление админа
            if(login == "root")
            {
                Console.WriteLine("\nНевозможно удалить администратора\n");
                continue;
            }

            // если такой пользователь есть проходим дальше
            if (selectUser != null) break;

            // если введён не существующий пользователь уведомляем
            // и очищаем экран
            Console.WriteLine("\nТакого пользователя не существует\n");

            Thread.Sleep(3000);

            Console.Clear();
        }

        // проверка пароля
        if (!CheckPassword(selectUser)) return;

        // удаление пользователя из списока
        _users.Remove(selectUser);

        // сохранение (сериализация) списка пользователей
        Serialize();

        return;
    }

    // просмотреть список пользователей
    public void ShowUsers()
    {
        Console.WriteLine($"\n\n{GetHeaderUsers()}");

        foreach (var user in _users)
        {
            string admin = user.Id == 0 ? "да" : "нет";

            Console.WriteLine($"\t    │ {user.Id,14} │ {user.Name,-13} │ {admin,-5} │");
        }

        Console.WriteLine(GetFooterUsers());
    }

    #region Вспомогательные функции

    // Сериализация в JSON коллекций сведений о клиентах
    private void Serialize()
    {
        string json = JsonConvert.SerializeObject(_users, Formatting.Indented);
        System.IO.File.WriteAllText(_usersFile, json, Encoding.UTF8);
    }

    // Десериализация из JSON коллекции сведений клиентах
    private void Deserialize()
    {
        string json = System.IO.File.ReadAllText(_usersFile, Encoding.UTF8);
        _users = JsonConvert.DeserializeObject<List<User>>(json)!;
    }

    // проверка пароля
    private bool CheckPassword(User user)
    {
        while (true)
        {
            // просим ввести пароль
            Console.WriteLine("\n\nДля выхода нажмите Enter\nВведите пароль:");

            string password = Console.ReadLine()!;

            // даём возможность выйти из процесса входа
            if (string.IsNullOrEmpty(password))
                return false;

            // проверяем пароль
            if (Utils.ValidatePassword(password, user.Password)) break;

            Console.WriteLine("\nНе верный пароль\n");
        }

        return true;
    }

    // шапка таблицы
    public static string GetHeaderUsers() =>
        "\t    ┌────────────────┬───────────────┬───────┐ \n"
      + "\t    │ Индентификатор │     Логин     │ Админ │ \n"
      + "\t    ├────────────────┼───────────────┼───────┤ ";


    // подвал таблицы
    public static string GetFooterUsers() =>
        "\t    └────────────────┴───────────────┴───────┘";

    #endregion
}
