using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1.Models
{
    public class LidarPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool Prediction { get; set; }  // ✅ Type bool pour classification binaire


        [ColumnName("Score")]
        public float Score { get; set; }  // ✅ Type float scalaire, pas tableau

        //public float Probability { get; set; }  // Optionnel :  pour la probabilité
    }

}
