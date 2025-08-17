using LangLang.Storage.Serialization;
using System.Collections.Generic;
using System.IO;

namespace LangLang.Storage
{
    public class Storage<T> where T : ISerializable, new()
    {
        private readonly string _fileName = @"../../../../LangLang/Data/{0}";
        private readonly Serializer<T> _serializer = new();

        public Storage(string fileName)
        {
            _fileName = string.Format(_fileName, fileName);
        }
        public List<T> Load()
        {
            if (!File.Exists(_fileName))
            {
                FileStream fs = File.Create(_fileName);
                fs.Close();
            }

            string content = File.ReadAllText(_fileName);
            List<T> objects = _serializer.FromJSON(content);

            return objects;
        }
        public void Save(List<T> objects)
        {
            string serializedObjects = _serializer.ToJSON(objects);
            using (StreamWriter streamWriter = new StreamWriter(_fileName))
            {
                streamWriter.Write(serializedObjects);
            }
        }
    }
}
