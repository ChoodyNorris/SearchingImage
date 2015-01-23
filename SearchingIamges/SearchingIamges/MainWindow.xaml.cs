using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using MahApps.Metro;
using MahApps.Metro.Controls;
using SearchingIamges.Enums;

namespace SearchingIamges
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public ObservableCollection<BasicImage> ImagesList { get; private set; }
        private readonly BackgroundWorker backgroundWorker = new BackgroundWorker();

        public MainWindow()
        {
            InitializeComponent();
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            PrepareImage();
            comboBoxImages.ItemsSource = ImagesList;
            comboBoxAlgorithm.ItemsSource = AlgorithmName.algorithmsTable;

        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs completedEventArgs)
        {

        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs doWorkEvent)
        {
            
        }

        #region kwantyzacja
        private void image1_Initialized(object sender, EventArgs e)
        {
        }



        private Image<Bgr, float> ConvertSourceToImage(ImageSource imgSource)
        {
            Bitmap bmp = BitmapSourceConvert.ToBitmap(imgSource as BitmapSource);
            return new Image<Bgr, float>(bmp);
        }

        private Image<Bgr, float> Quantization(Image<Bgr, float> source)
        {
            Matrix<float> samples = new Matrix<float>(source.Rows * source.Cols, 1, 3);
            Matrix<int> finalClusters = new Matrix<int>(source.Rows * source.Cols, 1);

            for (int y = 0; y < source.Rows; y++)
            {
                for (int x = 0; x < source.Cols; x++)
                {
                    samples.Data[y + x * source.Rows, 0] = (float)source[y, x].Blue;
                    samples.Data[y + x * source.Rows, 1] = (float)source[y, x].Green;
                    samples.Data[y + x * source.Rows, 2] = (float)source[y, x].Red;
                }
            }

            MCvTermCriteria term = new MCvTermCriteria(Convert.ToInt32(Iterations.Text), Convert.ToDouble(Epsilon.Text));
            term.type = TERMCRIT.CV_TERMCRIT_ITER | TERMCRIT.CV_TERMCRIT_EPS;

            int clusterCount = Int32.Parse(ClusterCount.Text);
            int attempts = Int32.Parse(Attempts.Text);
            CvInvoke.cvKMeans2(samples, clusterCount, finalClusters, term, attempts, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero);

            Image<Bgr, float> processImage = new Image<Bgr, float>(source.Size);

            int levels = Int32.Parse(ClusterCount.Text);
            Bgr[] clusterColors = new Bgr[levels];

            for (int i = 0; i < levels; i++)
                clusterColors[i] = new Bgr(i * (255 / (levels - 1)), i * (255 / (levels - 1)), i * (255 / (levels - 1)));

            for (int y = 0; y < source.Rows; y++)
            {
                for (int x = 0; x < source.Cols; x++)
                {
                    PointF p = new PointF(x, y);
                    processImage.Draw(new CircleF(p, 0.1f), clusterColors[finalClusters[y + x * source.Rows, 0]], 1);
                }
            }

            return processImage;
        }

        private float[] Histogram(Image<Bgr,float> img)
        {
            float[] BlueHist;

            DenseHistogram Histo = new DenseHistogram(256, new RangeF(0, 256));

            Image<Gray, float> img2Blue = img[0];

            Histo.Calculate(new Image<Gray, float>[] { img2Blue }, true, null);
            BlueHist = new float[256];
            Histo.MatND.ManagedArray.CopyTo(BlueHist, 0);

            Histo.Clear();

            return BlueHist;
        }

        private float ManhattanDistance(float[] firstHistogram, float[] secondHistogram)
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
                System.Windows.MessageBoxResult mb = System.Windows.MessageBox.Show(exception.Message,"Alert", MessageBoxButton.OK,MessageBoxImage.Warning);
                return -1;
            }
        }

        private float SumOfSquares(float[] vector)
        {
            float sum = 0;
            foreach (float e in vector)
                sum += e * e;
            return sum;
        }

        private float LenghtEuklides(float[] vector)
        {
            float sumOfSquares = SumOfSquares(vector);
            return (float)Math.Sqrt(sumOfSquares);
        }

        private float ScalarMultipy(float[] firstVector, float[] secondVector)
        {
            float sumOfMultiply = SumOfMultiply(firstVector, secondVector);
            return sumOfMultiply;
        }

        private float CosinusDistance(float[] firstHistogram, float[] secondHistogram)
        {
            try
            {
                float cosinus = ScalarMultipy(firstHistogram, secondHistogram) / (LenghtEuklides(firstHistogram) * LenghtEuklides(secondHistogram));
                return (1-cosinus);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                System.Windows.MessageBoxResult mb = System.Windows.MessageBox.Show(exception.Message,"Alert", MessageBoxButton.OK,MessageBoxImage.Warning);
                return -1;
            }
        }

        private float SumOfMultiply(float[] firstVector, float[] secondVector)
        {
            float sum = 0;
            for (int i = 0; i < firstVector.Length; i++)
                sum += firstVector[i] * secondVector[i];
            return sum;
        }

        private float Corelation(float[] firstHistogram, float[] secondHistogram)
        {
            float sumOfMultiply = SumOfMultiply(firstHistogram, secondHistogram);
            float sumOfSquares = SumOfSquares(firstHistogram);
            return sumOfMultiply / sumOfSquares;
        }

        private float SumOfDifferenceSquared(float[] firstVector, float[] secondVector)
        {
            float sum = 0;
            float dif = 0;
            for (int i = 0; i < firstVector.Length; i++)
            {
                dif = firstVector[i] - secondVector[i];
                sum+= dif*dif;
            }
            return sum;
        }

        private float EuklidesDistance(float[] firstHistogram, float[] secondHistogram)
        {
            try
            {
                float sumOfDifferenceSquared= SumOfDifferenceSquared(firstHistogram,secondHistogram);
                return (float)Math.Sqrt(sumOfDifferenceSquared);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                System.Windows.MessageBoxResult mb = System.Windows.MessageBox.Show(exception.Message, "Alert", MessageBoxButton.OK, MessageBoxImage.Warning);
                return -1;
            }
        }

        private float SumOfMax(float[] firstVector, float[] secondVector)
        {
            float sum = 0;
            for (int i = 0; i < firstVector.Length; i++)
                sum += Math.Max(firstVector[i], secondVector[i]);
            return sum;
        }

        private float SectionDistance(float[] firstHistogram, float[] secondHistogram)
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

        #region Events
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Image<Bgr, float> source = ConvertSourceToImage(OrginalImage.Source);
            Image<Bgr, float> processImage = Quantization(source);
            ProcessImage.Source = BitmapSourceConvert.ToBitmapSource(processImage);
            float[] h1 = Histogram(processImage);
            float[] h2 = Histogram(source);
            float d = ManhattanDistance(h1, h1);
            d++;

        }

        private void comboBoxImages_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = (sender as ComboBox).SelectedItem  as BasicImage;
            if (item != null)
            {
                var pathUri = new Uri(string.Format(@"{0}", item.ImageUri),UriKind.Relative);
                OrginalImage.Source = new BitmapImage(pathUri);
            }

        }
        private void ComboBoxAlgorithm_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        
        #endregion

        #region Private Methods
        private void PrepareImage()
        {
            ObservableCollection<BasicImage> observableCollection = new ObservableCollection<BasicImage>();
            string[] filePaths = Directory.GetFiles("Images", "*.jpg");

            foreach (var filePath in filePaths)
            {
                var img = new BasicImage()
                {
                    ImageUri = filePath,
                    ImageText = filePath.Substring("Images\\".Count(), filePath.Count() - "Images\\".Count())
                };
                observableCollection.Add(img);
            }
            ImagesList = observableCollection;
        }
        #endregion

        
    }
}
