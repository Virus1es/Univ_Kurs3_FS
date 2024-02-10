namespace OS_Kursavaya.Models;

// класс описывающий каталог
// хранит инормацию о файлах: номер индексного дескриптора, имя файла
public class Catalog
{ 
    // номер индексного дескриптора файла (2 байта)
    // имя файла (14 байт)
    public Dictionary<string, int> FileNamesAndInodeIndexes { get; set; }

    // конструкторы
    public Catalog() : this(new Dictionary<string, int>()) {
        FileNamesAndInodeIndexes.Add("fileSystem/", 0);
    }

    public Catalog(Dictionary<string, int> fileNamesAndInodeIndexes)
    {
        FileNamesAndInodeIndexes = fileNamesAndInodeIndexes;
    }

}
