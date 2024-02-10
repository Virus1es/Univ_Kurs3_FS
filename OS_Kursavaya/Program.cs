using OS_Kursavaya.Application;
using OS_Kursavaya.Infrastructure;

Console.Title = "Курсавая работа Якубенко В.В. группа ПИ-21в";
(Console.BackgroundColor, Console.ForegroundColor) = (ConsoleColor.DarkGray, ConsoleColor.Cyan);
Console.Clear();
Console.CursorVisible = false;

// устанавливаем размер консоли 140 символов на 40 строк
Console.SetWindowSize(140, 40);

// создаём класс для работы
App app = new App();

// переменные для меню
string sep = new string('─', 80);

string[] menu = {
      "\n",
      sep,
      "\nОперации с пользователями\n",
      "\t1.Добавить пользователя\n",
      "\t2.Удалить пользователя\n",
      "\t3.Сменить пользователя\n",
      "\t4.Выйти из системы\n",
      "\t5.Просмотреть список пользователей\n",
      sep,
      "\nCRUD операции с файлами/директориями\n",
      "\tQ.Создать файл/директорию\n",
      "\tW.Удалить файл/директорию\n",
      "\tE.Переименовать файл/директорию\n",
      "\tR.Переместить файл/директорию\n",
      "\tT.Сменить права для файла/директории\n",
      "\tY.Открыть файл\n",
      "\tU.Перейти в директорию\n",
      sep,
      "\nЗапись в файл\n",
      "\tA.Записать строку в файл\n",
      "\tS.Записать в конец файла\n",
      sep,
      "\n\n\nНажмите кнопку команды для продолжения. . ."
};

while (true)
{
    Console.Clear();
    // вывод меню
    // выводим текущего пользователя
    Console.WriteLine($"{sep}\n\tПользователь в системе: {app.ActiveUser.Name}" + 
                        (app.ActiveUser.Id == 0 ? "(админ)" : ""));

    // вывод файлов текущей директории
    app.ShowDirFiles(app.ActiveDir);

    // вывод меню (выбор действий с файловой системой)
    Array.ForEach(menu, Console.Write);

    // читаем введённый символ
    ConsoleKey key = Console.ReadKey(true).Key;
    Console.Clear();

    // если пользователь нажал 0 или Esc выходим из цикла
    if (key == ConsoleKey.Escape || key == ConsoleKey.Enter)
        break;

    try
    {
        switch (key)
        {

            #region Операции с пользователями

            // Добавить пользователя
            case ConsoleKey.D1:
                app.AddUser();
                break;

            // Удалить пользователя
            case ConsoleKey.D2:
                app.DeleteUser();
                break;

            // Сменить пользователя
            case ConsoleKey.D3:
                app.TryEnterUser();
                break;

            // Выйти из системы
            case ConsoleKey.D4:
                if (!app.TryEnterUser())
                    Environment.Exit(0);
                break;

            // Просмотреть список пользователей
            case ConsoleKey.D5:
                app.ShowUsers();
                break;

            #endregion

            #region CRUD файл/директорию

            // Создать файл/директорию
            case ConsoleKey.Q:
                app.CreateFileOrDirectory();
                break;

            // Удалить файл/директорию
            case ConsoleKey.W:
                app.DeleteFileOrDirectory();
                break;

            // Переименовать файл/директорию
            case ConsoleKey.E:
                app.RenameFileOrDirectory();
                break;

            // Переместить файл/директорию
            case ConsoleKey.R:
                app.MoveFileOrDirectory();
                break;

            // Сменить права для файла/директории
            case ConsoleKey.T:
                app.ChangeAccessLvlFileOrDirectory();
                break;

            // Открыть файл
            case ConsoleKey.Y:
                app.ChooseAndOpenFile();
                break;

            // Перейти в директорию
            case ConsoleKey.U:
                app.OpenDir();
                break;


            #endregion

            #region Запись в файл

            // Записать строку в файл
            case ConsoleKey.A:
                app.WriteToFile(false);
                break;

            // Записать в конец файла
            case ConsoleKey.S:
                app.WriteToFile(true);
                break;


            #endregion

            default:
                throw new Exception("Ошибка такой команды нет!");
        }
    }
    catch (Exception ex)
    {
        // вывод текста пойманной ошибки
        Utils.PrintError(ex.Message);
    }

    // вывод сообщения о ожидании нажатия клавиши
    Console.WriteLine("\n\n\n\n\tНажмите любую клавишу для продолжения. . . ");
    Console.ReadKey();

}
