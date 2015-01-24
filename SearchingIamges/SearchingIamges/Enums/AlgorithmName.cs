using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SearchingIamges.Enums
{
    public static class AlgorithmName
    {
        public const string Manhattan = "Manhattan";
        public const string Cosinus = "Cosinus";
        public const string Euklidesa = "Euklidesa";
        public const string Corelation = "Corelation";
        public const string PrzekrojHistogramow = "Przekrój histogramów";

        public static List<string>  algorithmsTable = new List<string>()
        {
            Manhattan,
            Cosinus,
            Euklidesa,
            Corelation,
            PrzekrojHistogramow
        };
    }
}
