using System;
using System.IO;
using System.Configuration;
using Newtonsoft.Json;

namespace dbms
{
    public class FormConfiguration
    {
        public string FormCaption { get; set; }
        public string ConnectionString { get; set; }
        public TableConfig MasterTable { get; set; }
        public TableConfig DetailTable { get; set; }
    }

    public class TableConfig
    {
        public string TableName { get; set; }
        public string DisplayName { get; set; }
        public string SelectQuery { get; set; }
        public string InsertQuery { get; set; }
        public string UpdateQuery { get; set; }
        public string DeleteQuery { get; set; }
        public string[] DisplayColumns { get; set; }
        public string[] InputColumns { get; set; }
    }

    public static class ConfigurationManager
    {
        private static FormConfiguration _config;

        public static FormConfiguration GetConfiguration()
        {
            if (_config == null)
            {
                string configPath = System.Configuration.ConfigurationManager.AppSettings["ConfigFilePath"];
                if (File.Exists(configPath))
                {
                    string json = File.ReadAllText(configPath);
                    _config = JsonConvert.DeserializeObject<FormConfiguration>(json);
                }
                else
                {
                    throw new FileNotFoundException($"Configuration file not found at: {configPath}");
                }
            }
            return _config;
        }
    }
} 