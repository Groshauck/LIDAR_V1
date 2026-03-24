using Newtonsoft.Json;
using System.IO;
using WinFormsApp1.Models;

namespace WinFormsApp1.Services
{
    public class ModbusConfigManager
    {
        private static readonly string ConfigPath = Path.Combine(
            System.AppDomain.CurrentDomain.BaseDirectory, "modbus_config.json");

        public ModbusConfig Load()
        {
            if (!File.Exists(ConfigPath))
                return new ModbusConfig();
            try
            {
                string json = File.ReadAllText(ConfigPath);
                return JsonConvert.DeserializeObject<ModbusConfig>(json) ?? new ModbusConfig();
            }
            catch { return new ModbusConfig(); }
        }

        public void Save(ModbusConfig config)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(ConfigPath, json);
        }
    }
}
