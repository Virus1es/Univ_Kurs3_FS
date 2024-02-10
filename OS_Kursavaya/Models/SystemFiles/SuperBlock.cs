using Newtonsoft.Json;

namespace OS_Kursavaya.Models.SystemFiles;

// класс описывающий суперблок
internal class SuperBlock
{
    [JsonIgnore]
    // тип файловой системы
    public readonly string SystemType = "TrojanOS";

    [JsonIgnore]
    // размер файловой системы в логических блоках(включается всё)
    public readonly ulong FileSysSize = 8589934592;

    [JsonIgnore]
    // размер массива индексных дескрипторов
    public readonly uint InodeArrSize = 100;

    // число свободных блоков для размещения
    public int CountFreeBlocks { get; set; }

    // число свободных inode для размещения
    public int CountFreeInods { get; set; }

    // флаг модификации
    public bool IsModification { get; private set; } = false;

    // флаг режима монтирования
    public bool IsMountingStatus { get; private set; } = false;

    // размер логического блока(кластера)
    public readonly uint ClusterUnitSize = 4096; // 4 кб

    // список номеров свободных inode
    // храним только часть списка, когда число почти = 0,
    // пересматриваем ilist и вновь формируем список
    public int[] ListIndexesFreeInods { get; set; }

    // список адресов свободных блоков
    // хранит только 1 блок первый элемент этого блока указывает на продолжение списка
    // выделение свободных блоков для размещения файла производиться с конца спискасупер блока
    public int FirstFreeBlock { get; set; }

    // конструктор
    public SuperBlock(int countFreeInods, int countFreeBlocks, int[] listIndexesFreeInods, int firstFreeBlock)
    {
        CountFreeInods = countFreeInods;
        CountFreeBlocks = countFreeBlocks;
        ListIndexesFreeInods = listIndexesFreeInods;
        FirstFreeBlock = firstFreeBlock;
    }

    

    // находим свободный inode для записи
    public int FindFreeInode() => Array.Find(ListIndexesFreeInods, i => i != -1);

    // занимаем inode в списке
    public void WriteInode(int index)
    {
        ListIndexesFreeInods[Array.FindIndex(ListIndexesFreeInods, i => i == index)] = -1;

        CountFreeInods--;
    }
}
