using OS_Kursavaya.Infrastructure;
using OS_Kursavaya.Models.SystemFiles;
using System.Text;

namespace OS_Kursavaya.Application;

// CRUD опеpации с файлами
public partial class App
{
    #region Вспомогательные функции

    private delegate void VoidFunc();

    // Выбрать действие для файла/директории
    private void ChooseFileOrDirectory(string process, VoidFunc file, VoidFunc dir)
    {
        while (true)
        {
            Console.Clear();
            ShowDirFiles(ActiveDir);
            // вывод меню
            Console.WriteLine($"\n\tУкажите что вы хотите {process} 1 - файл 2 - диреторию:\n(Enter или Esc - выход)");

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
                    // Создать файл
                    case ConsoleKey.D1:
                        file();
                        break;

                    // Создать директорию
                    case ConsoleKey.D2:
                        dir();
                        break;

                    default:
                        throw new Exception("Ошибка такой команды нет!");
                }

                // сохранение результатов работы пользователя
                SerializeSuperBlock();
                SerializeIList();
                SerializeCatalog();
                SerializeMemory();
            }
            catch (Exception ex)
            {
                Utils.PrintError(ex.Message);
            }

            // вывод сообщения о ожидании нажатия клавиши
            Console.WriteLine("\n\n\n\n\tНажмите любую клавишу для продолжения. . .");
            Console.ReadKey();
        
        }
    }

    // получить инод файла
    private Inode GetInodeByPath(string path)
    {
        int indexInode = _catalog.FileNamesAndInodeIndexes.First(v => v.Key == path).Value;

        return _iList.Inodes[indexInode];
    }

    // привязка inode к файлу
    private void WriteFileMemory(string creationPath, string fileName)
    {
        FileType type = fileName.EndsWith(".bin") ? FileType.File : FileType.Catalog;

        var dimode = new DIMode(type, AccessLevel.FullAccess, AccessLevel.Read);

        var inode = new Inode(dimode, 1, ActiveUser.Id, 4096, DateTime.Now, DateTime.Now, DateTime.Now, [_superBlock.FirstFreeBlock, -1, -1, -1]);

        int inodeIndex = _superBlock.FindFreeInode();

        _superBlock.WriteInode(inodeIndex);

        _memory.DataBlocks[inode.DataDiskBlocks[0]] = new DataBlock();

        _superBlock.FirstFreeBlock = Array.FindIndex(_memory.DataBlocks, d => d == null);

        _iList.Inodes[inodeIndex] = inode;

        _catalog.FileNamesAndInodeIndexes.Add($"{creationPath}{fileName}", inodeIndex);

        if (_superBlock.CountFreeInods <= 5) RefreshInodeIndexList();
    }

    // ввод имени существующего файла
    private string EnterExistNameFile(string creation, string path)
    {
        // имя создаваемого файла
        string fileName;

        // запрашиваем имя файла 
        while (true)
        {
            Console.WriteLine($"\n\tВведите имя {creation}:");

            // удаляем ненужные пробелы в начале и конце
            fileName = Console.ReadLine()!.Trim();

            // если имя файла пустое - ошибка 
            // если файл с таким именем по указанному пути есть - ошибка
            // иначе всё хорошо
            if (!string.IsNullOrEmpty(fileName) && File.Exists(path + fileName + ".bin")) break;

            Console.WriteLine("\n\n\nИмя файла не может быть пустым или файл не существует\n\n");
        }

        return fileName;
    }

    // ввод имени существующей диретории
    private string EnterExistNameDirectory(string creation, string path)
    {
        // имя создаваемого диретории
        string fileName;

        // запрашиваем имя диретории
        while (true)
        {
            Console.WriteLine($"\n\tВведите имя {creation}:");

            // удаляем ненужные пробелы в начале и конце
            fileName = Console.ReadLine()!.Trim();

            // если имя диретории не пустое и
            // диретории с таким именем по указанному пути нет,
            // то пропускаем
            if (!string.IsNullOrEmpty(fileName) && Directory.Exists(path + fileName)) break;

            Console.WriteLine("\n\n\nИмя диретории не может быть пустым или директория не существует\n\n");
        }

        return fileName;
    }

    // вывод содержимого директории
    public void ShowDirFiles(string path)
    {
        Console.WriteLine($"\tСодержимое каталога {path}/ :");
        Console.WriteLine(GetHeaderFiles());

        if (!path.StartsWith("fileSystem/")) path = "fileSystem/" + path;

        if (!path.EndsWith('/')) path += '/';

        string[] files = Directory.GetFileSystemEntries(path);

        foreach (var item in files)
        {
            char sep = item.Contains('\\') ? '\\' : '/';

            string name = item.Substring(item.LastIndexOf(sep) + 1);

            Inode inode = GetInodeByPath(path + name);

            string access = (inode.DIMode.OtherUsersAccessLevel == AccessLevel.FullAccess) ? "полный доступ" :
                            (inode.DIMode.OtherUsersAccessLevel == AccessLevel.Read) ? "чтение" : "запись";

            string catalog = inode.DIMode.FileType == FileType.File ? "нет" : "да";

            Console.Write($"\t    │ {name,-15} │ {inode.AuthorId,9} │ {inode.FileSize,-9} │ {access,-29} │ {catalog,-7} │\n");
        }

        Console.WriteLine(GetFooterFiles());
    }

    // шапка таблицы
    public static string GetHeaderFiles() =>
        "\t    ┌─────────────────┬───────────┬───────────┬───────────────────────────────┬─────────┐ \n"
      + "\t    │       Имя       │ Id автора │ Размер, б │ Уровень доступа для остальных │ Каталог │ \n"
      + "\t    ├─────────────────┼───────────┼───────────┼───────────────────────────────┼─────────┤ ";

    // подвал таблицы
    public static string GetFooterFiles() =>
        "\t    └─────────────────┴───────────┴───────────┴───────────────────────────────┴─────────┘";

    // обновление списка индексов свободных inode
    private void RefreshInodeIndexList()
    {
        _superBlock.ListIndexesFreeInods = _iList.FindIndexFreeInodes();

        _superBlock.CountFreeInods = 100;
    }

    // занимаем свободный блок и теперь только он хранит данные файла (данные < 4 кб)
    private void SetOneDataBlock(byte[] bytes, string path)
    {
        // берём первый свободный блок
        DataBlock data = _memory.DataBlocks[_superBlock.FirstFreeBlock] ?? new DataBlock();

        // записываем в него данные
        for (int i = 0; i < bytes.Length; i++)
            data.Data[i] = bytes[i];

        // назначаем суперблоку новый первый свободный блок
        _superBlock.FirstFreeBlock = data.NextId;

        // блок занят один - продолжения файла нет
        data.NextId = -1;

        // сохраняем записанный файл в памяти
        _memory.DataBlocks[data.Id] = data;

        // ищем инод записываемого файла
        int indexInode = _catalog.FileNamesAndInodeIndexes.First(v => v.Key == path).Value;

        Inode inode = _iList.Inodes[indexInode];

        // ищем свободное место в массиве адресов блоков
        int indexFreeBlock = -1;
        for (int i = 0; i < inode.DataDiskBlocks.Length; i++)
        {
            if (inode.DataDiskBlocks[i] == -1)
            {
                indexFreeBlock = i;
                break;
            }
        }

        // записываем номер блока с данными файла
        inode.DataDiskBlocks[indexFreeBlock] = data.Id;

        // уменьшаем количество свободных блоков в суперблоке
        _superBlock.CountFreeBlocks--;
    }

    // занимаем несколько свободных блоков (данные > 4 кб)
    private void SetManyDataBlocks(byte[] bytes, string path)
    {
        // берём первый свободный блок
        DataBlock data = _memory.DataBlocks[_superBlock.FirstFreeBlock] ?? new DataBlock();

        // ищем инод записываемого файла
        int indexInode = _catalog.FileNamesAndInodeIndexes.First(v => v.Key == path).Value;

        Inode inode = _iList.Inodes[indexInode];

        // ищем свободное место в массиве адресов блоков
        int indexFreeBlock = -1;
        for (int i = 0; i < inode.DataDiskBlocks.Length; i++)
        {
            if (inode.DataDiskBlocks[i] == -1) {
                indexFreeBlock = i;
                break;
            }
        }

        for (int i = 0; i < bytes.Length; i++)
        {
            // записываем в каждый блок кластер данных
            for (int j = 0; j < 4096; j++)
                data.Data[j] = bytes[i];

            // записываем номер блока с данными файла
            inode.DataDiskBlocks[indexFreeBlock++] = data.Id;

            // записываем блок в память
            _memory.DataBlocks[data.Id] = data;

            // меняем блок для записи
            data = _memory.DataBlocks[data.NextId] ?? new DataBlock();

            // уменьшаем количество свободных блоков
            _superBlock.CountFreeBlocks--;
        }

        // назначаем суперблоку новый первый свободный блок
        _superBlock.FirstFreeBlock = data.NextId;

        // Последний блок не указывает на следующий свободный блок
        data.NextId = -1;

        // записываем блок в память
        _memory.DataBlocks[data.Id] = data;
    }

    // очистка блоков (удаление файла)
    private void ClearFileInfoSys(string path)
    {
        // получаем инод для очистки блоков
        Inode inode = GetInodeByPath(path);

        // очистка блоков
        for (int i = 0; i < inode.DataDiskBlocks.Length; i++)
        {
            if (inode.DataDiskBlocks[i] == -1) break;

            _memory.DataBlocks[inode.DataDiskBlocks[i]] = new DataBlock();

            inode.DataDiskBlocks[i] = -1;
        }

        // очистка инода
        int indexInode = _catalog.FileNamesAndInodeIndexes.First(v => v.Key == path).Value;
        _iList.Inodes[indexInode] = new Inode();

        // очистка каталога
        _catalog.FileNamesAndInodeIndexes.Remove(path);
    }

    #endregion


    #region CRUD файл/директория

    // созадать файл или директорию
    public void CreateFileOrDirectory() => ChooseFileOrDirectory("создать", CreateFile, CreateDirectory);

    // создание файла
    private void CreateFile()
    {
        // путь создания файла
        // просим ввести путь создания файла
        string creationPath = EnterPathDirectory("создаваемого файла");

        var inode = GetInodeByPath(creationPath);

        // проверка прав доступа
        // админ - может игнорировать права доступа
        if (ActiveUser.Id != 0 && inode.AuthorId != ActiveUser.Id 
            && inode.DIMode.OtherUsersAccessLevel == AccessLevel.Read) 
            throw new Exception("Не достаточно прав для создания файла!");

        if (!creationPath.EndsWith('/')) creationPath += '/';

        // имя создаваемого файла
        // запрашиваем имя файла
        string fileName = EnterNameFileForCreation("создаваемого файла", creationPath);

        // привязка inode к файлу
        WriteFileMemory(creationPath, $"{fileName}.bin");

        // создать файл
        File.Create($"{creationPath}{fileName}.bin");
    }
    
    // создание директории
    private void CreateDirectory()
    {
        // путь создания директории
        // просим ввести путь создания директории
        string creationPath = EnterPathDirectory("создаваемой диретории");

        var inode = GetInodeByPath(creationPath);

        // проверка прав доступа
        // админ - может игнорировать права доступа
        if (ActiveUser.Id != 0 && inode.AuthorId != ActiveUser.Id && inode.DIMode.OtherUsersAccessLevel == AccessLevel.Read)
            throw new Exception("Не достаточно прав для создания директории!");

        if (!creationPath.EndsWith('/')) creationPath += '/';

        // имя создаваемого файла
        // запрашиваем имя файла
        string directoryName = EnterNameDirectoryForCreation("создаваемой диретории", creationPath);

        // привязка inode к файлу
        WriteFileMemory(creationPath, directoryName);

        // создать файл
        Directory.CreateDirectory($"{creationPath}{directoryName}");
    }

    // ввод имени файла
    private string EnterNameFileForCreation(string creation, string path)
    {
        // имя создаваемого файла
        string fileName;

        // запрашиваем имя файла 
        while (true)
        {
            Console.WriteLine($"\n\tВведите имя {creation}:");

            // удаляем ненужные пробелы в начале и конце
            fileName = Console.ReadLine()!.Trim();

            // если имя файла пустое - ошибка 
            // если файл с таким именем по указанному пути есть - ошибка
            // иначе всё хорошо
            if (!string.IsNullOrEmpty(fileName) && !_catalog.FileNamesAndInodeIndexes.ContainsKey(path + fileName + ".bin")) break;

            Console.WriteLine("\n\n\nИмя файла не может быть пустым или совпадать с другим файлом\n\n");
        }

        return fileName;
    }

    // ввод имени диретории
    private string EnterNameDirectoryForCreation(string creation, string path)
    {
        // имя создаваемого диретории
        string fileName;

        // запрашиваем имя диретории
        while (true)
        {
            Console.WriteLine($"\n\tВведите имя {creation}:");

            // удаляем ненужные пробелы в начале и конце
            fileName = Console.ReadLine()!.Trim();

            // если имя диретории не пустое и
            // диретории с таким именем по указанному пути нет,
            // то пропускаем
            if (!string.IsNullOrEmpty(fileName) && !_catalog.FileNamesAndInodeIndexes.ContainsKey(path + fileName)) break;

            Console.WriteLine("\n\n\nИмя диретории не может быть пустым или совпадать с другой диреторией\n\n");
        }

        return fileName;
    }

    // ввод пути создания файла или диретории
    private string EnterPathDirectory(string creation)
    {
        // имя создаваемого или диретории
        string path;

        // запрашиваем имя файла или диретории
        while (true)
        {
            Console.WriteLine($"\n\tВведите путь для {creation}" +
                               "\n\t(/ - если хотите остаться в текущей директории, иначе введите полный путь от корневой директории)");

            // удаляем ненужные пробелы в начале и конце
            path = Console.ReadLine()!.Trim();

            // если выбрали остаться в текущей директории
            if (path == "/") path = ActiveDir;

            if (path.StartsWith('/')) path = path.Substring(1);

            if (!path.StartsWith("fileSystem/")) path = Utils.rootPath + path;

            // если указанная диретория (путь) существует,
            // то пропускаем
            if (Directory.Exists(path)) break;
        }

        return path;
    }




    // Удалить файл/директорию
    public void DeleteFileOrDirectory() => ChooseFileOrDirectory("удалить", DeleteFile, DeleteDirectory);

    // удалить файл
    private void DeleteFile()
    {
        // путь создания файла
        // просим ввести путь создания файла
        string deletePath = EnterPathDirectory("удаляемого файла");

        if (!deletePath.EndsWith('/')) deletePath += '/';

        // имя создаваемого файла
        // запрашиваем имя файла
        string fileName = EnterExistNameFile("удаляемого файла", deletePath);

        string fullName = $"{deletePath}{fileName}.bin";

        var inode = GetInodeByPath(fullName);

        // проверка прав доступа
        // админ - может игнорировать права доступа
        if (ActiveUser.Id != 0 && inode.AuthorId != ActiveUser.Id 
            && inode.DIMode.OtherUsersAccessLevel == AccessLevel.Read)
            throw new Exception("Не достаточно прав для удаления файла!");

        // очистка блоков данных (удаление файла)
        ClearFileInfoSys(fullName);

        // удалить файл
        File.Delete(fullName);        
    }

    // удалить директорию
    private void DeleteDirectory()
    {
        // просим ввести путь удаляемой директории
        string deletePath = EnterPathDirectory("удаляемой директории (без самой директории)");

        if (!deletePath.EndsWith('/')) deletePath += '/';

        // запрашиваем имя директории
        string dirName = EnterExistNameDirectory("удаляемой директории", deletePath);

        string fullName = $"{deletePath}{dirName}";

        var inode = GetInodeByPath(fullName);

        // проверка прав доступа
        // админ - может игнорировать права доступа
        if (ActiveUser.Id != 0 && inode.AuthorId != ActiveUser.Id 
            && inode.DIMode.OtherUsersAccessLevel == AccessLevel.Read)
            throw new Exception("Не достаточно прав для удаления директории");

        // для простоты: не удалем директории с другими директориями
        if (Directory.GetDirectories(fullName).Length != 0) 
            throw new Exception("Не возможно удалить директорию содержащую другие директории");

        // удалить все файлы в директории
        string[] files = Directory.GetFiles(fullName);

        foreach (var item in files)
        {
            // отвязать inode от файла
            ClearFileInfoSys(deletePath + item);

            // удалить файл
            File.Delete(fullName);
        }

        // очистка блоков данных (удаление файла)
        ClearFileInfoSys(fullName);

        // удалить саму директорию
        Directory.Delete(fullName);
    }




    // Переименовать файл/директорию
    public void RenameFileOrDirectory() => ChooseFileOrDirectory("переименовать", RenameFile, RenameDirectory);

    // Переименовать файл
    private void RenameFile()
    {
        // просим ввести путь файла для переименования
        string path = EnterPathDirectory("файла для переименования");

        if (!path.EndsWith('/')) path += '/';

        // имя создаваемого файла
        // запрашиваем имя файла
        string fileName = EnterExistNameFile("файла для переименования", path);

        string fullName = $"{path}{fileName}.bin";

        var inode = GetInodeByPath(fullName);

        // проверка прав доступа
        // админ - может игнорировать права доступа
        if (ActiveUser.Id != 0 && inode.AuthorId != ActiveUser.Id
            && inode.DIMode.OtherUsersAccessLevel == AccessLevel.Read)
            throw new Exception("Не достаточно прав для переименования файла!");

        string newName = EnterNameFileForCreation("новое для файла", path);

        // изменяем имя файла в каталоге
        int val = _catalog.FileNamesAndInodeIndexes.First(f => f.Key == fullName).Value;

        _catalog.FileNamesAndInodeIndexes.Add($"{path}{newName}.bin", val);

        _catalog.FileNamesAndInodeIndexes.Remove(fullName);

        // файл
        File.Move(fullName, $"{path}{newName}.bin");
    }

    // Переименовать директорию
    private void RenameDirectory()
    {
        // просим ввести путь директории для переименования
        string path = EnterPathDirectory("директории для переименования (без самой директории)");

        if (!path.EndsWith('/')) path += '/';

        // запрашиваем имя директории
        string dirName = EnterExistNameDirectory("директории для переименования", path);

        string fullName = $"{path}{dirName}";

        var inode = GetInodeByPath(fullName);

        // проверка прав доступа
        // админ - может игнорировать права доступа
        if (ActiveUser.Id != 0 && inode.AuthorId != ActiveUser.Id
            && inode.DIMode.OtherUsersAccessLevel == AccessLevel.Read)
            throw new Exception("Не достаточно прав для удаления директории");

        if (Directory.GetFileSystemEntries(fullName).Length != 0)
            throw new Exception("Нельзя переименовать директорию с файлами :(");

        string newName = EnterNameDirectoryForCreation("новое для директории", path);

        // изменяем имя директории в каталоге
        int val = _catalog.FileNamesAndInodeIndexes.First(f => f.Key == fullName).Value;

        _catalog.FileNamesAndInodeIndexes.Add($"{path}{newName}", val);

        _catalog.FileNamesAndInodeIndexes.Remove(fullName);

        // удалить саму директорию
        Directory.Move(fullName, $"{path}{newName}");
    }




    // Переместить файл/директорию
    public void MoveFileOrDirectory() => ChooseFileOrDirectory("переместить", MoveFile, MoveDirectory);

    // Переместить файл
    private void MoveFile()
    {
        // просим ввести путь файла для перемещения
        string path = EnterPathDirectory("файла для перемещения");

        if (!path.EndsWith('/')) path += '/';

        // запрашиваем имя файла
        string fileName = EnterExistNameFile("файла для перемещения", path);

        string fullName = $"{path}{fileName}.bin";

        var inode = GetInodeByPath(fullName);

        // проверка прав доступа
        // админ - может игнорировать права доступа
        if (ActiveUser.Id != 0 && inode.AuthorId != ActiveUser.Id
            && inode.DIMode.OtherUsersAccessLevel == AccessLevel.Read)
            throw new Exception("Не достаточно прав для перемещения файла");

        string newPath = EnterPathDirectory("вставки файла");

        // если перемещение происходит туда же, то ничего не делаем
        if (path == newPath) return;

        // изменяем имя файла в каталоге
        int val = _catalog.FileNamesAndInodeIndexes.First(f => f.Key == fullName).Value;

        _catalog.FileNamesAndInodeIndexes.Add($"{newPath}{fileName}.bin", val);

        _catalog.FileNamesAndInodeIndexes.Remove(fullName);

        // файл
        File.Move(fullName, $"{newPath}{fileName}.bin");
    }

    // Переместить директорию
    private void MoveDirectory()
    {
        // просим ввести путь директории для перемещения
        string path = EnterPathDirectory("директории для перемещения (без самой директории)");

        if (!path.EndsWith('/')) path += '/';

        // запрашиваем имя директории
        string dirName = EnterExistNameDirectory("директории для перемещения", path);

        string fullName = $"{path}{dirName}";

        var inode = GetInodeByPath(fullName);

        // проверка прав доступа
        // админ - может игнорировать права доступа
        if (ActiveUser.Id != 0 && inode.AuthorId != ActiveUser.Id
            && inode.DIMode.OtherUsersAccessLevel == AccessLevel.Read)
            throw new Exception("Не достаточно прав для перемещения директории");

        if (Directory.GetFileSystemEntries(fullName).Length != 0)
            throw new Exception("Нельзя перемещать директорию с файлами :(");

        string newPath = EnterPathDirectory("вставки директории");

        // если перемещение происходит туда же, то ничего не делаем
        if (path == newPath) return;

        // изменяем имя директории в каталоге
        int val = _catalog.FileNamesAndInodeIndexes.First(f => f.Key == fullName).Value;

        _catalog.FileNamesAndInodeIndexes.Add($"{newPath}{dirName}", val);

        _catalog.FileNamesAndInodeIndexes.Remove(fullName);

        // переместить саму директорию
        Directory.Move(fullName, $"{newPath}{dirName}");
    }




    // Сменить права для файла/директории
    public void ChangeAccessLvlFileOrDirectory() => ChooseFileOrDirectory("менять", ChangeAccessLvlFile, ChangeAccessLvlDirectory);

    // Переместить файл
    private void ChangeAccessLvlFile()
    {
        // просим ввести путь файла
        string path = EnterPathDirectory("файла");

        if (!path.EndsWith('/')) path += '/';

        // запрашиваем имя файла
        string fileName = EnterExistNameFile("файла", path);

        string fullName = $"{path}{fileName}.bin";

        var inode = GetInodeByPath(fullName);

        // проверка прав доступа
        // админ - может игнорировать права доступа
        if (ActiveUser.Id != 0 && inode.AuthorId != ActiveUser.Id
            && inode.DIMode.OtherUsersAccessLevel != AccessLevel.FullAccess)
            throw new Exception("Не достаточно прав для смены прав для файла");

        AccessLevel newAccess = AccessLevel.None;

        while (true)
        {
            Console.Clear();
            // вывод меню
            Console.WriteLine("\n\tУкажите права доступа для других пользователей: " +
                              "\n\t1 - чтение 2 - запись 3 - полный доступ:" +
                              "\n\t(Enter или Esc - выход)");

            // читаем введённый символ
            ConsoleKey key = Console.ReadKey(true).Key;
            Console.Clear();

            // если пользователь нажал 0 или Esc выходим из цикла
            if (key == ConsoleKey.Escape || key == ConsoleKey.Enter)
                break;

            try
            {
                newAccess = key switch
                {
                    ConsoleKey.D1 => AccessLevel.Read,
                    ConsoleKey.D2 => AccessLevel.Write,
                    ConsoleKey.D3 => AccessLevel.FullAccess,
                    _ => throw new Exception("Ошибка такой команды нет!")
                };
            }
            catch (Exception ex)
            {
                Utils.PrintError(ex.Message);
            }

            // вывод сообщения о ожидании нажатия клавиши
            Console.WriteLine("\n\n\n\n\tНажмите любую клавишу для продолжения. . . ");
            Console.ReadKey();

        }

        // поменять режим доступа другим пользователям в ilist
        int indexInode = _catalog.FileNamesAndInodeIndexes[fullName];

        _iList.Inodes[indexInode].DIMode.OtherUsersAccessLevel = newAccess;
    }

    // Переместить директорию
    private void ChangeAccessLvlDirectory()
    {
        // просим ввести путь директории для перемещения
        string path = EnterPathDirectory("директории для перемещения (без самой директории)");

        if (!path.EndsWith('/')) path += '/';

        // запрашиваем имя директории
        string dirName = EnterExistNameDirectory("директории для перемещения", path);

        string fullName = $"{path}{dirName}";

        var inode = GetInodeByPath(fullName);

        // проверка прав доступа
        // админ - может игнорировать права доступа
        if (ActiveUser.Id != 0 && inode.AuthorId != ActiveUser.Id
            && inode.DIMode.OtherUsersAccessLevel == AccessLevel.FullAccess)
            throw new Exception("Не достаточно прав для смены прав для директории");

        AccessLevel newAccess = AccessLevel.None;

        while (true)
        {
            Console.Clear();
            // вывод меню
            Console.WriteLine("\n\tУкажите права доступа для других пользователей: " +
                              "\n\t1 - чтение 2 - запись 3 - полный доступ:" +
                              "\n\t(Enter или Esc - выход)");

            // читаем введённый символ
            ConsoleKey key = Console.ReadKey(true).Key;
            Console.Clear();

            // если пользователь нажал 0 или Esc выходим из цикла
            if (key == ConsoleKey.Escape || key == ConsoleKey.Enter)
                break;

            try
            {
                newAccess = key switch
                {
                    ConsoleKey.D1 => AccessLevel.Read,
                    ConsoleKey.D2 => AccessLevel.Write,
                    ConsoleKey.D3 => AccessLevel.FullAccess,
                    _ => throw new Exception("Ошибка такой команды нет!")
                };
            }
            catch (Exception ex)
            {
                Utils.PrintError(ex.Message);
            }

            // вывод сообщения о ожидании нажатия клавиши
            Console.WriteLine("\n\n\n\n\tНажмите любую клавишу для продолжения. . . ");
            Console.ReadKey();

        }

        // поменять режим доступа другим пользователям в ilist
        int indexInode = _catalog.FileNamesAndInodeIndexes[fullName];

        _iList.Inodes[indexInode].DIMode.OtherUsersAccessLevel = newAccess;
    }


    // Открыть файл
    public void ChooseAndOpenFile()
    {
        // просим ввести путь файла
        string path = EnterPathDirectory("файла");

        if (!path.EndsWith('/')) path += '/';

        // для удобства вывод содержимого директории
        ShowDirFiles(path);

        // запрашиваем имя файла
        string fileName = EnterExistNameFile("файла", path);

        string fullName = $"{path}{fileName}.bin";

        var inode = GetInodeByPath(fullName);

        // проверка прав доступа
        // админ - может игнорировать права доступа
        if (ActiveUser.Id != 0 && inode.AuthorId != ActiveUser.Id
            && inode.DIMode.OtherUsersAccessLevel == AccessLevel.Write)
            throw new Exception("Не достаточно прав для открытия файла");

        if (inode.DIMode.FileType != FileType.File) throw new Exception("Попытка открыть не файл");

        // читаем файл
        string data = "";
        using (FileStream fs = new FileStream(fullName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            using StreamReader streamReader = new StreamReader(fullName);
            data = streamReader.ReadToEnd();
        }

        Console.WriteLine($"Содержиоме файла {fileName}:\n\n\t{data}");
    }


    // Перейти в директорию
    public void OpenDir()
    {
        // для удобства вывод содержимого директории
        ShowDirFiles(ActiveDir);

        // просим ввести путь директории
        string path = EnterPathDirectory("директории (без названия нужной директории)");

        if (!path.EndsWith('/')) path += '/';

        Console.Clear();

        // для удобства вывод содержимого директории
        ShowDirFiles(path);

        // запрашиваем имя директории
        string fileName = EnterExistNameDirectory("директории", path);

        string fullName = $"{path}{fileName}";

        var inode = GetInodeByPath(fullName);

        // проверка прав доступа
        // админ - может игнорировать права доступа
        if (ActiveUser.Id != 0 && inode.AuthorId != ActiveUser.Id
            && inode.DIMode.OtherUsersAccessLevel == AccessLevel.Write)
            throw new Exception("Не достаточно прав для открытия директории");

        if (inode.DIMode.FileType != FileType.Catalog) throw new Exception("Попытка открыть не директорию");

        _activeDir = fullName;

        Console.WriteLine($"\n\n\tТекущая директория: {ActiveDir}");
    }

    #endregion


    #region Запись в файл

    // Записать строку в файл
    // Записать в конец файла - append = true
    public void WriteToFile(bool append)
    {
        // просим ввести путь файла
        string path = EnterPathDirectory("файла");

        if (!path.EndsWith('/')) path += '/';

        // для удобства вывод содержимого директории
        ShowDirFiles(path);

        // запрашиваем имя файла
        string fileName = EnterExistNameFile("файла", path);

        string fullPath = $"{path}{fileName}.bin";

        var inode = GetInodeByPath(fullPath);

        // проверка прав доступа
        // админ - может игнорировать права доступа
        if (ActiveUser.Id != 0 && inode.AuthorId != ActiveUser.Id
            && inode.DIMode.OtherUsersAccessLevel == AccessLevel.Write)
            throw new Exception("Не достаточно прав для открытия файла");

        if (inode.DIMode.FileType != FileType.File) throw new Exception("Попытка записи не в файл");

        Console.WriteLine("\n\n\tВведите строку для записи:");
        string data = Console.ReadLine()!;

        // если ничего не ввели молча выходим
        if (string.IsNullOrEmpty(data)) return;

        using (MemoryStream stream = new MemoryStream())
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            int dataSize = dataBytes.Length;
            if (_superBlock.CountFreeBlocks != 0)
            {
                // Запись данных в файл, размер которых не превышает размер одного кластера(4кб)
                if (dataSize <= 4096)
                {
                    SetOneDataBlock(dataBytes, fullPath);

                    WriteBytesToFile(fullPath, stream, dataBytes, dataSize, append);
                }
                // Запись данных в файл, размер которых больше одного кластера(4кб)
                else if (dataSize > 4096)
                {
                    SetManyDataBlocks(dataBytes, fullPath);
                    WriteBytesToFile(fullPath, stream, dataBytes, dataSize, append);
                }
            }
            else
                throw new Exception("Нет свободных блоков для записи");

            stream.Dispose();
            stream.Close();
        }
    }

    // записьбайтов в файл
    private void WriteBytesToFile(string fullPath, MemoryStream stream, byte[] dataBytes, int dataSize, bool append)
    {
        FileMode fileMode = FileMode.Create;
        // если нужно дозаписать в файл меняем тип доступа
        if (append)
        {
            fileMode = FileMode.Append;
        }

        // собственно запись в файл
        using BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(dataBytes);
        using (FileStream fileStream = new FileStream(fullPath, fileMode, FileAccess.Write,
               FileShare.None, dataSize, FileOptions.WriteThrough))
        {
            stream.WriteTo(fileStream);
            fileStream.Close();
        }
        writer.Flush();
        writer.Close();
    }

    #endregion
}