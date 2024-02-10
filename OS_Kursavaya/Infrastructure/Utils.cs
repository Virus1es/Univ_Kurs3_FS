using System.Security.Cryptography;
using System.Text;

namespace OS_Kursavaya.Infrastructure;


// уровни доступа для файлов и каталогов
public enum AccessLevel
{
    None,
    Read,
    Write,
    FullAccess
}

// типы файлов в системе
public enum FileType
{
    Empty,
    File,
    Catalog,
    Metadata
}

internal class Utils
{
    // префикс файлов для сериализации
    public const string filesPrefix = @"../../../";

    // путь к корню файловой системы
    public const string rootPath = "fileSystem/";

    public static Random random = new Random();

    // вывести строку определённым цветом и затем вернуть цвет обратно
    public static void PrintWithBackColor(ConsoleColor change, ConsoleColor save, string title)
    {
        Console.BackgroundColor = change;
        Console.Write(title);
        Console.BackgroundColor = save;
        Console.WriteLine();
    }

    // формирование случайных чисел в диапазоне от lo до hi
    public static int GetRandom(int lo, int hi) =>
        random.Next(lo, hi + 1);
    public static double GetRandom(double lo, double hi) =>
        lo + (hi - lo) * random.NextDouble();


    // Установить текущий цвет символов и фона с сохранением
    // текущего цвета символов и фона
    private static (ConsoleColor Fore, ConsoleColor Back) _storeColor;
    public static void SetColor(ConsoleColor fore, ConsoleColor back)
    {
        _storeColor = (Console.ForegroundColor, Console.BackgroundColor);
        Console.ForegroundColor = fore;
        Console.BackgroundColor = back;
    }

    // Сохранить цвет
    public static void SaveColor() =>
        _storeColor = (Console.ForegroundColor, Console.BackgroundColor);

    // Восстановить сохраненный цвет
    public static void RestoreColor() =>
        (Console.ForegroundColor, Console.BackgroundColor) = _storeColor;


    // Вспомогательный метод для вывода в заданных координатах окна консоли текста
    // заданным цветом
    public static void WriteXY(int x, int y, string s, ConsoleColor color)
    {
        // сохранить текущий цвет консоли и установить заданный
        ConsoleColor oldColor = Console.ForegroundColor;
        Console.ForegroundColor = color;

        Console.SetCursorPosition(x, y);
        Console.Write(s);

        // восстановить цвет консоли
        Console.ForegroundColor = oldColor;
    }

    // получение целого числа из консоли
    public static int GetInt(string title, int low = int.MinValue, int high = int.MaxValue)
    {
        while (true)
        {
            Console.Write(title);
            string content = Console.ReadLine();

            if(int.TryParse(content, out int value))
            {
                if(low <= value && value <= high)
                    return value;
                else
                    Console.WriteLine("\n\t\tДанное число не попадает в диапозон\n\n");
            }

            Console.WriteLine("\n\t\tВведено не число\n");
        }
    }

    // подготовкка строки к записи в бинарный файл
    public static byte[] SetReadyStringToWrite(string str, int length)
    {
        byte[] buf = new byte[length];

        Encoding.UTF8
            .GetBytes(str)
            .CopyTo(buf, 0);

        return buf;
    }

    // вывод ошибки
    public static void PrintError(string message)
    {
        string spaceErr = new string(' ', 80);

        // сохраняем текущий цвет консоли
        ConsoleColor forwSave, backSave;
        (forwSave, backSave) = (Console.ForegroundColor, Console.BackgroundColor);

        // окрашиваем вывод ошибки
        (Console.ForegroundColor, Console.BackgroundColor) = (ConsoleColor.White, ConsoleColor.DarkRed);

        // расчитываем сколько пробелов нужно добавить справа и слева
        int space = (80 - message.Length) / 2;

        // фомируем страку с информацией об ошибке
        string err = message;
        err = err.PadLeft(space + err.Length);
        err = err.PadRight(space + err.Length + 1);

        // выводим пойманную ошибку
        Console.WriteLine("\n\n\n\n" +
            $"\t{spaceErr}\n\t{spaceErr}\n" +
            $"\t{err}\n" +
            $"\t{spaceErr}\n\t{spaceErr}\n"
        );

        // восстанавливаем цвет
        (Console.ForegroundColor, Console.BackgroundColor) = (forwSave, backSave);
    }

    // кодирование пароля пользователя алгоритмом SHA-256
    public static string GenerateHash(string password)
    {
        using SHA256 sha256 = SHA256.Create();

        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));

        StringBuilder sb = new StringBuilder();

        foreach (byte b in bytes)
        {
            sb.Append(b.ToString("x2"));
        }

        return sb.ToString();
    }

    // проверка введённого пароля
    public static bool ValidatePassword(string password, string hashedPassword)
    {
        string inputHash = GenerateHash(password);
        return inputHash.Equals(hashedPassword);
    }

}
