﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchingIamges.Enums
{
    public static class AlgorithmName
    {
        public static string Manhattan = "Manhattan";
        public static string KullbackaLeiblera = "Kullbacka-Leiblera";
        public static string Euklidesa = "Euklidesa";
        public static string Jeffreya = "Jeffreya";
        public static string PrzekrojHistogramow = "Przekrój histogramów";

        public static List<string>  algorithmsTable = new List<string>()
        {
            Manhattan,
            KullbackaLeiblera,
            Euklidesa,
            Jeffreya,
            PrzekrojHistogramow
        };
    }
}