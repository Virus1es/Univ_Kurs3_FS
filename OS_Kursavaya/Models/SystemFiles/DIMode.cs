using OS_Kursavaya.Infrastructure;
using System.Text.Json.Serialization;

namespace OS_Kursavaya.Models.SystemFiles;

// класс описывающий поле Inode di_mode
// хранит тип файла, дополнительные атрибуты и права доступа

internal class DIMode
{
    // тип файла
    public FileType FileType { get; set; } 

    // права доступа для владельца
    public AccessLevel AuthorAccessLevel { get; set; }

    // права доступа для остальных пользователей
    public AccessLevel OtherUsersAccessLevel { get; set; }

    // конструкторы
    public DIMode() : this(FileType.Empty, AccessLevel.None, AccessLevel.None) { }

    [JsonConstructor]
    public DIMode(FileType fileType, AccessLevel authorAccess, AccessLevel otherUsersAccess)
    {
        FileType = fileType;
        AuthorAccessLevel = authorAccess;
        OtherUsersAccessLevel = otherUsersAccess;
    }
}
