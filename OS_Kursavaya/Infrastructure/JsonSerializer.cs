using Newtonsoft.Json;
using System.Text;

namespace OS_Kursavaya.Infrastructure;

internal class JsonSerializer<T>
{
    // чтение
    public static T Load(string path) => (!File.Exists(path)
        ? throw new FileNotFoundException("Файл не найден", path)
        : JsonConvert.DeserializeObject<T>(File.ReadAllText(path, Encoding.UTF8)))!;


    // запись
    public static void Save(T data, string path) =>
        File.WriteAllText(path, JsonConvert.SerializeObject(data, Formatting.Indented), Encoding.UTF8);
}
