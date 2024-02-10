namespace OS_Kursavaya.Models.SystemFiles;

public class Memory
{
    public DataBlock[] DataBlocks { get; set; }

    public Memory() : this(new DataBlock[32000]) { }

    public Memory(DataBlock[] dataBlocks)
    {
        DataBlocks = dataBlocks;
    }

    // запись данных
    public void WriteData(byte[] bytes, int dataBlockId)
    {
        int countBlocks = bytes.Length / 4096;

        int dataBlock = Array.FindIndex(DataBlocks, d => d.Id == dataBlockId);

        if (dataBlock == -1) return;

        for (int i = 0; i < bytes.Length; i++)
        {

            DataBlocks[dataBlock].Data = bytes;

            if(DataBlocks[i].Data.Length == 4096)
                dataBlock = Array.FindIndex(DataBlocks, d => d.Id == DataBlocks[dataBlock].NextId);
        }

        DataBlocks[dataBlock].NextId = -1;
    }

    // дозапись новых блоков
    public void AddData(byte[] bytes, int dataBlockId, int dataBlockIdAdd)
    {
        int countBlocks = bytes.Length / 4096;

        int dataBlock = Array.FindIndex(DataBlocks, d => d.Id == dataBlockId);
        int dataBlockAdd = Array.FindIndex(DataBlocks, d => d.Id == dataBlockIdAdd);

        if (dataBlock == -1 || dataBlockAdd == -1) return;

        while (dataBlock != -1)
        {
            dataBlock = Array.FindIndex(DataBlocks, d => d.Id == DataBlocks[dataBlock].NextId);
        }

        DataBlocks[dataBlock].NextId = dataBlockIdAdd;

        for (int i = 0; i < bytes.Length; i++)
        {
            DataBlocks[dataBlockAdd].Data = bytes;

            if (DataBlocks[i].Data.Length == 4096)
                dataBlockAdd = Array.FindIndex(DataBlocks, d => d.Id == DataBlocks[dataBlockAdd].NextId);
        }

        DataBlocks[dataBlockAdd].NextId = -1;
    }

    // удаление данных
    public void ClearData(int dataBlockId)
    {
        int dataBlock = Array.FindIndex(DataBlocks, d => d.Id == dataBlockId);

        if (dataBlock == -1) return;

        while (dataBlock != -1)
        {
            DataBlocks[dataBlock].Data = new byte[4096];

            dataBlock = Array.FindIndex(DataBlocks, d => d.Id == DataBlocks[dataBlock].NextId);
        }
    }
}
