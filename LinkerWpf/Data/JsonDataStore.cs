using System;
using System.IO;
using System.Threading;
using Newtonsoft.Json;

namespace Linker.Data
{
    public class JsonDataStore
    {
        public const string DataFileName = "linker-data.json";
        private readonly string _path;
        private readonly Mutex _mutex = new Mutex(false, @"Local\LinkerJsonDataStore");

        public JsonDataStore(string path)
        {
            _path = path;
        }

        public static string DefaultDataPath()
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Linker");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, DataFileName);
        }

        public void EnsureCreated()
        {
            if (!File.Exists(_path))
            {
                Save(new LinkerData());
            }
        }

        public T Read<T>(Func<LinkerData, T> reader)
        {
            _mutex.WaitOne();
            try
            {
                return reader(Load());
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        public void Mutate(Action<LinkerData> mutator)
        {
            _mutex.WaitOne();
            try
            {
                var data = Load();
                mutator(data);
                Save(data);
            }
            finally
            {
                _mutex.ReleaseMutex();
            }
        }

        private LinkerData Load()
        {
            if (!File.Exists(_path)) return new LinkerData();
            using (var stream = File.OpenRead(_path))
            using (var reader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return JsonSerializer.CreateDefault().Deserialize<LinkerData>(jsonReader) ?? new LinkerData();
            }
        }

        private void Save(LinkerData data)
        {
            var tmpPath = _path + ".tmp";
            using (var stream = File.Create(tmpPath))
            using (var writer = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(writer))
            {
                JsonSerializer.CreateDefault().Serialize(jsonWriter, data);
            }
            File.Copy(tmpPath, _path, true);
            File.Delete(tmpPath);
        }
    }
}
