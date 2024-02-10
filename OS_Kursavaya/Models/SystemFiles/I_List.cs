using Newtonsoft.Json;
using OS_Kursavaya.Infrastructure;

namespace OS_Kursavaya.Models.SystemFiles;

// класс описывающий Массив индексных дескрипторов
internal class I_List
{
    [JsonIgnore]
    // размер массива inode
    public readonly uint ArrSize = 32000; 

    // массив свободных и занятых Inode
    // 1 - корневой Inode 
    public Inode[] Inodes { get; set; }

    // конструкторы
    public I_List() {
        Inodes = new Inode[ArrSize];

        Inodes[0] = new Inode(new DIMode(FileType.Catalog, AccessLevel.FullAccess, AccessLevel.FullAccess), 1, 0, 10, 
                              DateTime.Now, DateTime.Now, DateTime.Now, [0]);
    }

    public I_List(Inode[] inodes)
    {
        Inodes = inodes;
    }

    // обновление списка индексов свободных Inode
    public int[] FindIndexFreeInodes()
    {
        int[] inodes = new int[100];

        int k = 0;
        for (int i = 0; i < Inodes.Length; i++)
        {
            if (Inodes[i] == null)
                inodes[k++] = i;

            if (k == 100) break;
        }

        return inodes;
    }

}
