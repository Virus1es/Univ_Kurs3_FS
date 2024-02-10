namespace OS_Kursavaya.Models.SystemFiles;

public class DataBlock
{
    // счётчик для идентификаторов пользователей
    private static int Cur_id = 0;

    public int Id { get; private set; }

    // ссылка на следующий блок памяти 
    public int NextId { get; set; }

    // данные которые хранит блок
    public byte[] Data { get; set; }

    // конструктор
    public DataBlock() : this(Cur_id + 1, new byte[4096]) { }

    public DataBlock(int nextId, byte[] data)
    {
        Id = Cur_id++;
        NextId = nextId;
        Data = data;
    }
}
