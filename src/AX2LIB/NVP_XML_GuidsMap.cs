using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AX2LIB
{
    public class NVP_XML_GuidsMap_Item
    {
        /// <summary>
        /// Путь к элементу (namespace + class + element_name)
        /// </summary>
        public string Name { get; set; }
        public string Id { get; set; }
    }
    /// <summary>
    /// Вспомогательный класс для сопоставления названий функций и их Guid (для предотвращения новой генерации идентификаторов)
    /// </summary>
    public class NVP_XML_GuidsMap
    {
        public NVP_XML_GuidsMap()
        {
            items = new List<NVP_XML_GuidsMap_Item>();
        }
        public List<NVP_XML_GuidsMap_Item> items { get; set; }

        public static NVP_XML_GuidsMap LoadSchema(string schemaPath)
        {
            if (!File.Exists(schemaPath)) return new NVP_XML_GuidsMap();

            NVP_XML_GuidsMap map;
            string file_data = File.ReadAllText(schemaPath);

            map = (NVP_XML_GuidsMap)System.Text.Json.JsonSerializer.Deserialize(file_data,
                 typeof(NVP_XML_GuidsMap), new System.Text.Json.JsonSerializerOptions
                 {
                     PropertyNameCaseInsensitive = true
                 });

            return map;
        }

        public void Save(string savePath)
        {
            string json = System.Text.Json.JsonSerializer.Serialize(this, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            });

            File.WriteAllText(savePath, json);
        }
    }
}
