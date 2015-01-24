using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SearchingIamges
{
    public static class Algorithms
    {
        #region MetodyPomocnicze
        private static float SumOfSquares(float[] vector)
        {
            float sum = 0;
            foreach (float e in vector)
                sum += e * e;
            return sum;
        }

        private static float ScalarMultipy(float[] firstVector, float[] secondVector)
        {
            float sumOfMultiply = SumOfMultiply(firstVector, secondVector);
            return sumOfMultiply;
        }

        private static float LenghtEuklides(float[] vector)
        {
            float sumOfSquares = SumOfSquares(vector);
            return (float)Math.Sqrt(sumOfSquares);
        }

        private static float SumOfMultiply(float[] firstVector, float[] secondVector)
        {
            float sum = 0;
            for (int i = 0; i < firstVector.Length; i++)
                sum += firstVector[i] * secondVector[i];
            return sum;
        }

        private static float SumOfDifferenceSquared(float[] firstVector, float[] secondVector)
        {
            float sum = 0;
            float dif = 0;
            for (int i = 0; i < firstVector.Length; i++)
            {
                dif = firstVector[i] - secondVector[i];
                sum += dif * dif;
            }
            return sum;
        }

        private static float SumOfMax(float[] firstVector, float[] secondVector)
        {
            float sum = 0;
            for (int i = 0; i < firstVector.Length; i++)
                sum += Math.Max(firstVector[i], secondVector[i]);
            return sum;
        }
        #endregion

        #region MetodyDoLiczeniaOdległości
        public static float Manhattan(float[] firstHistogram, float[] secondHistogram)
        {
            float sum = 0;
            try
            {
                if (firstHistogram.Length == secondHistogram.Length)
                {
                    for (int i = 0; i < firstHistogram.Length; i++)
                    {
                        sum += Math.Abs(firstHistogram[i] - secondHistogram[i]);
                    }
                    return sum;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Tablice nie mają identycznych wymiarów");
                }
            }
            catch (ArgumentOutOfRangeException exception)
            {
                System.Windows.MessageBoxResult mb = System.Windows.MessageBox.Show(exception.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return -1;
            }
        }

        public static float Cosinus(float[] firstHistogram, float[] secondHistogram)
        {
            try
            {
                float cosinus = ScalarMultipy(firstHistogram, secondHistogram) / (LenghtEuklides(firstHistogram) * LenghtEuklides(secondHistogram));
                return (1 - cosinus);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                System.Windows.MessageBoxResult mb = System.Windows.MessageBox.Show(exception.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return -1;
            }
        }
        public static float Corelation(float[] firstHistogram, float[] secondHistogram)
        {
            float sumOfMultiply = SumOfMultiply(firstHistogram, secondHistogram);
            float sumOfSquares = SumOfSquares(firstHistogram);
            return sumOfMultiply / sumOfSquares;
        }

        public static float Euklidesa(float[] firstHistogram, float[] secondHistogram)
        {
            try
            {
                float sumOfDifferenceSquared = SumOfDifferenceSquared(firstHistogram, secondHistogram);
                return (float)Math.Sqrt(sumOfDifferenceSquared);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                System.Windows.MessageBoxResult mb = System.Windows.MessageBox.Show(exception.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return -1;
            }
        }

        public static float Przekrój_histogramów(float[] firstHistogram, float[] secondHistogram)
        {
            try
            {
                float distance = 1 - SumOfMax(firstHistogram, secondHistogram) / firstHistogram.Length;
                return 1;
            }
            catch (ArgumentOutOfRangeException exception)
            {
                System.Windows.MessageBoxResult mb = System.Windows.MessageBox.Show(exception.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return -1;
            }
        }
        #endregion
    }
}
