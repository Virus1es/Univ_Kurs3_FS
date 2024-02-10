using Newtonsoft.Json;
using OS_Kursavaya.Infrastructure;

namespace OS_Kursavaya.Models.SystemFiles;

// класс описывающий индексный дескриптор
// для кажого файла он свой
internal class Inode
{
    // di_mode - тип файла, дополнительные атрибуты и права доступа
    // если = 0, то инод свободен
    public DIMode DIMode { get; set; }

    // di_nlinks - число ссылок на файл, количество имён файла в ФС
    public int CountNamesFile { get; set; }

    // di_uid - идентификатор пользователя владельца
    public int AuthorId { get; private set; } = -1;

    // di_size - размер файла в байтах
    public int FileSize { get; set; }

    // di_atime - время последнего доступа(обращения) к файлу
    public DateTime LastOpenFile { get; set; }

    // di_mtime - время последней модиикации файла
    public DateTime LastModificationFile { get; set; }

    // di_ctime - время последней модиикации inode
    public DateTime LastModificationInode { get; set; }

    // di_addr - массив адресов дисковых блоков хранения данных
    public int[] DataDiskBlocks { get; set; }

    // конструктор
    public Inode() : this(new DIMode(), 0, -1, 0, new DateTime(2000, 1, 1), new DateTime(2000, 1, 1), new DateTime(2000, 1, 1), new int[0]) { }


    [JsonConstructor]
    public Inode(DIMode dIMode, int countNamesFile, int authorId, int fileSize, DateTime lastOpenFile,
                 DateTime lastModificationFile, DateTime lastModificationInode, int[] dataDiskBlocks)
    {
        DIMode = dIMode;
        CountNamesFile = countNamesFile;
        AuthorId = authorId;
        FileSize = fileSize;
        LastOpenFile = lastOpenFile;
        LastModificationFile = lastModificationFile;
        LastModificationInode = lastModificationInode;
        DataDiskBlocks = dataDiskBlocks;
    }
}
