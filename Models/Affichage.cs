using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1.Models
{
    public class Affichage
    {
        public double xMin { get; set; }
        public double xMax { get; set; }
        public double yMin { get; set; }
        public double yMax { get; set; }
        public bool auto { get; set; }
        public double FiltreplasticMin { get; set; }
        public double FiltreplasticMax { get; set; }
        public double FiltrepouletMin { get; set; }
        public double FiltrepouletMax { get; set; }
        public bool FilterPoulet { get; set; }
        public bool FilterPlastic { get; set; }
        public bool ColorAuto { get; set; }


        // ========== ZONE PLASTIQUE 1 ========== 
        public double numericYmaxPlastic { get; set; }
        public double numericYminPlastic { get; set; }
        public double numericXmaxPlastic { get; set; }
        public double numericXminPlastic { get; set; }
        public double numericNbPointMin { get; set; }


        // ========== ZONE PLASTIQUE 2 ==========
        public double numericXminPlastic2 { get; set; } 
        public double numericXmaxPlastic2 { get; set; } 
        public double numericYminPlastic2 { get; set; } 
        public double numericYmaxPlastic2 { get; set; } 
        public double numericNbPointMinPlastic2 { get; set; } 

        // ========== ZONE PLASTIQUE 3 ==========
        public double numericXminPlastic3 { get; set; } 
        public double numericXmaxPlastic3 { get; set; } 
        public double numericYminPlastic3 { get; set; } 
        public double numericYmaxPlastic3 { get; set; } 
        public double numericNbPointMinPlastic3 { get; set; }

        // ========== ZONE IA/CSV ==========
        public double numericXminIA { get; set; } = -600;
        public double numericXmaxIA { get; set; } = 600;
        public double numericYminIA { get; set; } = -600;
        public double numericYmaxIA { get; set; } = 600;

        public double AngleMin { get; set; }
        public double AngleMax { get; set; }
        public double SignalStrengthMin { get; set; }
        public double SignalStrengthMax { get; set; }
        public bool SignalStrengthAuto { get; set; }
        public string textBoxCheminExport { get; set; }
    }
}
