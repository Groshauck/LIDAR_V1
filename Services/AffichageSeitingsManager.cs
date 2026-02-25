using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WinFormsApp1.Models;

namespace WinFormsApp1.Services
{
    public static class AffichageSettingsManager
    {
        private static readonly string Chemin = "affichage.json"; // ou ailleurs si tu veux

        public static Affichage Charger()
        {
            if (File.Exists(Chemin))
                return JsonSerializer.Deserialize<Affichage>(File.ReadAllText(Chemin)) ?? new Affichage();
            return new Affichage();
        }
        public static void Sauvegarder(Affichage a)
        {
            string json = JsonSerializer.Serialize(a, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Chemin, json);
        }
    }
}
