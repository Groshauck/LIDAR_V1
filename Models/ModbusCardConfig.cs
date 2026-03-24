namespace WinFormsApp1.Models
{
    public class ModbusCardConfig
    {
        public string Nom { get; set; } = "Carte 1";
        public string IpAddress { get; set; } = "192.168.0.10";
        public int Port { get; set; } = 502;
        public int SlaveId { get; set; } = 1;
        public bool Enabled { get; set; } = true;
        public int NbEntrees { get; set; } = 16;
        public int NbSorties { get; set; } = 16;
        // Assignation fonctionnelle des bits d'entrée
        public int BitEntreeDemarrerTest { get; set; } = 0;   // bit entrée → déclenche test 30s
        public int BitEntreeCachePresent { get; set; } = 1;   // bit entrée → bloque si=1
        // Assignation fonctionnelle des bits de sortie
        public int BitSortieNormal { get; set; } = 0;         // sortie bit 0 → résultat Normal (voyant VERT)
        public int BitSortieDeboite { get; set; } = 1;        // sortie bit 1 → résultat Déboîté (voyant ROUGE)
        public int BitSortieTestEnCours { get; set; } = 2;    // sortie bit 2 → test en cours
    }
}
