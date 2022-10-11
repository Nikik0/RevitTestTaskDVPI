using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RevitTestTaskDVPI
{
    internal class JsonParser
    {
        private readonly string path = $"D://Revit 2022 plugin custom//RevitTestTaskDVPI//RevitTestTaskDVPI//bin//Debug//infoStored.json";
        public JsonInfo ReadJson() //парсер для запоминания введных значений текстовых полей на форме
        {   
            try
            {
                string jsonFromFile;
                using (var reader = new StreamReader(path))
                {
                    jsonFromFile = reader.ReadToEnd();
                }

                JsonInfo infoFromJson = JsonConvert.DeserializeObject<JsonInfo>(jsonFromFile);

                //System.Diagnostics.Debug.WriteLine(infoFromJson.FirstBox);
                return infoFromJson;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public void WriteJson(JsonInfo jsonInfo)
        {
            try
            {

                JsonParser jsonParser = new JsonParser();
                JsonInfo existingInfo = jsonParser.ReadJson();

                if (jsonInfo.FirstBox == null)
                {
                    jsonInfo.FirstBox = existingInfo.FirstBox;
                }

                if (jsonInfo.SecondBox == null)
                {
                    jsonInfo.SecondBox = existingInfo.SecondBox;
                }

                var jsonToWrite = JsonConvert.SerializeObject(jsonInfo, Formatting.Indented);

               // System.Diagnostics.Debug.WriteLine(jsonInfo.FirstBox);

                using (var writer = new StreamWriter(path))
                {
                    writer.Write(jsonToWrite);
                }
            }
            catch (Exception ex)
            {
                // ignored
            }
        }
        
    }
    class JsonInfo
    {
        public string FirstBox { get; set; }
        /// <summary>
        /// some changesвыв
        /// </summary>
        public string SecondBox { get; set; }

    }
}
