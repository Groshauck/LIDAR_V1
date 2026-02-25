using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsApp1.Models
{
    public class LidarSummaryFeatures
    {
        [LoadColumn(0)] public bool Label { get; set; }
        [LoadColumn(1)] public float NbPoints { get; set; }
        [LoadColumn(2)] public float XMin { get; set; }
        [LoadColumn(3)] public float XMax { get; set; }
        [LoadColumn(4)] public float YMin { get; set; }
        [LoadColumn(5)] public float YMax { get; set; }
        [LoadColumn(6)] public float XSpan { get; set; }
        [LoadColumn(7)] public float YSpan { get; set; }
        [LoadColumn(8)] public float XMoy { get; set; }
        [LoadColumn(9)] public float YMoy { get; set; }
        [LoadColumn(10)] public float XStd { get; set; }
        [LoadColumn(11)] public float YStd { get; set; }
        [LoadColumn(12)] public float SignalStrengthMoyen { get; set; }
        [LoadColumn(13)] public float SignalStrengthStd { get; set; }

        // SuperX1 à SuperX10 (colonnes 14-23)
        [LoadColumn(14)] public float SuperX1 { get; set; }
        [LoadColumn(15)] public float SuperX2 { get; set; }
        [LoadColumn(16)] public float SuperX3 { get; set; }
        [LoadColumn(17)] public float SuperX4 { get; set; }
        [LoadColumn(18)] public float SuperX5 { get; set; }
        [LoadColumn(19)] public float SuperX6 { get; set; }
        [LoadColumn(20)] public float SuperX7 { get; set; }
        [LoadColumn(21)] public float SuperX8 { get; set; }
        [LoadColumn(22)] public float SuperX9 { get; set; }
        [LoadColumn(23)] public float SuperX10 { get; set; }

        // SuperY1 à SuperY10 (colonnes 24-33)
        [LoadColumn(24)] public float SuperY1 { get; set; }
        [LoadColumn(25)] public float SuperY2 { get; set; }
        [LoadColumn(26)] public float SuperY3 { get; set; }
        [LoadColumn(27)] public float SuperY4 { get; set; }
        [LoadColumn(28)] public float SuperY5 { get; set; }
        [LoadColumn(29)] public float SuperY6 { get; set; }
        [LoadColumn(30)] public float SuperY7 { get; set; }
        [LoadColumn(31)] public float SuperY8 { get; set; }
        [LoadColumn(32)] public float SuperY9 { get; set; }
        [LoadColumn(33)] public float SuperY10 { get; set; }

        // SuperSignal1 à SuperSignal10 (colonnes 34-43)
        [LoadColumn(34)] public float SuperSignal1 { get; set; }
        [LoadColumn(35)] public float SuperSignal2 { get; set; }
        [LoadColumn(36)] public float SuperSignal3 { get; set; }
        [LoadColumn(37)] public float SuperSignal4 { get; set; }
        [LoadColumn(38)] public float SuperSignal5 { get; set; }
        [LoadColumn(39)] public float SuperSignal6 { get; set; }
        [LoadColumn(40)] public float SuperSignal7 { get; set; }
        [LoadColumn(41)] public float SuperSignal8 { get; set; }
        [LoadColumn(42)] public float SuperSignal9 { get; set; }
        [LoadColumn(43)] public float SuperSignal10 { get; set; }
    }
}
