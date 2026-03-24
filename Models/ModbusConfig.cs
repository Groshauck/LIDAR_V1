using System.Collections.Generic;

namespace WinFormsApp1.Models
{
    public class ModbusConfig
    {
        public bool ActivationGlobale { get; set; } = false;
        public List<ModbusCardConfig> Cartes { get; set; } = new List<ModbusCardConfig>();
        public int PollingIntervalMs { get; set; } = 200;
    }
}
