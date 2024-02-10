using OS_Kursavaya.Infrastructure;
using OS_Kursavaya.Models;
using OS_Kursavaya.Models.SystemFiles;
using System.Text;

namespace OS_Kursavaya.Application;

public partial class App
{
    // файлы сохранения состояний классов файловой системы
    private const string _usersFile = $"{Utils.filesPrefix}/App_Data/users.json";
    private const string _superBlockFile = $"{Utils.filesPrefix}/App_Data/SuperBlock.json";
    private const string _iListFile = $"{Utils.filesPrefix}/App_Data/IList.json";
    private const string _catalogFile = $"{Utils.filesPrefix}/App_Data/Catalog.json";
    private const string _memoryFile = $"{Utils.filesPrefix}/App_Data/Memory.json";

    // сохранение в файл / загрузка из файла каталога файловой системы
    public void SerializeCatalog() => JsonSerializer<Catalog>.Save(_catalog, _catalogFile);
    public void DeserializeCatalog() => _catalog = JsonSerializer<Catalog>.Load(_catalogFile);

    // сохранение в файл / загрузка из файла суперблока файловой системы
    public void SerializeSuperBlock() => JsonSerializer<SuperBlock>.Save(_superBlock, _superBlockFile);
    public void DeserializeSuperBlock() => _superBlock = JsonSerializer<SuperBlock>.Load(_superBlockFile);

    // сохранение в файл / загрузка из файла списка инод файловой системы
    public void SerializeIList() => JsonSerializer<I_List>.Save(_iList, _iListFile);
    public void DeserializeIList() => _iList = JsonSerializer<I_List>.Load(_iListFile);

    // сохранение в файл / загрузка из файла памяти файловой системы
    public void SerializeMemory() => JsonSerializer<Memory>.Save(_memory, _memoryFile);
    public void DeserializeMemory() => _memory = JsonSerializer<Memory>.Load(_memoryFile);


    // список пользователей которые есть в системе
    private List<User> _users = new List<User>();
    public List<User> Users => _users;

    // пользователь, который сейчас работает в системе
    private User _activeUser = null!; 
    public User ActiveUser => _activeUser;

    // директория в которой сейчас работает пользователь
    private string _activeDir = "";
    public string ActiveDir => _activeDir;

    // каталог - франит иформацию о файлах
    private Catalog _catalog = null!;
    public Catalog Catalog => _catalog;

    // суперблок
    private SuperBlock _superBlock = null!;

    // списов свободных и занятых inode
    private I_List _iList = null!;

    // списов свободных и занятых блоков
    private Memory _memory = null!;

    // конструктор
    public App() {
        // проверить корневой каталог ФС
        CheckFolderFileSystem();

        // прверить все файлы системы
        ReadOrCrateUsersFile();
        ReadOrCrateMemoryFile();
        ReadOrCrateIListFile();
        ReadOrCrateSuperBlockFile();
        ReadOrCrateCatalogFile();

        // пробуем войти в систему при включении 
        if (!TryEnterUser())
            Environment.Exit(0);
    }

    // проверить наличие папки fileSystem 
    public void CheckFolderFileSystem()
    {
        // если нет папки "корня" создадим её
        if (!Directory.Exists("fileSystem"))
            Directory.CreateDirectory("fileSystem");
    }

    // чтение или создание суперблока
    public void ReadOrCrateSuperBlockFile()
    {
        // если не нашли файл создаём его
        if (File.Exists(_superBlockFile))
            DeserializeSuperBlock();
        else
        {
            _superBlock = new SuperBlock(100, 31999, _iList.FindIndexFreeInodes(), 1);
            SerializeSuperBlock();
        }
    }

    // чтение или создание ilist
    public void ReadOrCrateIListFile()
    {
        // если не нашли файл создаём его
        if (File.Exists(_iListFile))
            DeserializeIList();
        else
        {
            _iList = new I_List();
            SerializeIList();
        }
    }

    // чтение или создание каталог
    public void ReadOrCrateCatalogFile()
    {
        // если не нашли файл создаём его
        if (File.Exists(_catalogFile))
            DeserializeCatalog();
        else
        {
            _catalog = new Catalog();
            SerializeCatalog();
        }
    }

    // чтение или создание блоков памяти
    public void ReadOrCrateMemoryFile()
    {
        // если не нашли файл создаём его
        if (File.Exists(_memoryFile))
            DeserializeMemory();
        else
        {
            _memory = new Memory();
            _memory.DataBlocks[0] = new DataBlock(-1, Encoding.UTF8.GetBytes("string for write"));
            SerializeMemory();
        }
    }
}
