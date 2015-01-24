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
using MahApps.Metro.Controls.Dialogs;
using SearchingIamges.Enums;

namespace SearchingIamges
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {

        private readonly BackgroundWorker backgroundWorker = new BackgroundWorker();

        #region Properties
        public ObservableCollection<BasicImage> ImagesList { get; private set; }
        public string SelectedAlgorithm { get; set; }

        public Image<Bgr, float> processImage { get; set; }
        public Image<Bgr, float> source { get; set; }

        public string IterationsValue { get; set; }
        public string EpsilonValue { get; set; }
        public string ClusterCountValue { get; set; }
        public string AttemptsValue { get; set; }
        #endregion


        public MainWindow()
        {
            InitializeComponent();
            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            PrepareImage();
            comboBoxImages.ItemsSource = ImagesList;
            comboBoxAlgorithm.ItemsSource = AlgorithmName.algorithmsTable;

        }

        #region kwantyzacja

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

            MCvTermCriteria term = new MCvTermCriteria(Convert.ToInt32(IterationsValue), Convert.ToDouble(EpsilonValue));
            term.type = TERMCRIT.CV_TERMCRIT_ITER | TERMCRIT.CV_TERMCRIT_EPS;

            int clusterCount = Int32.Parse(ClusterCountValue);
            int attempts = Int32.Parse(AttemptsValue);
            CvInvoke.cvKMeans2(samples, clusterCount, finalClusters, term, attempts, IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero);

            Image<Bgr, float> processImage = new Image<Bgr, float>(source.Size);

            int levels = Int32.Parse(ClusterCountValue);
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

        private float[] Histogram(Image<Bgr, float> img)
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




        #endregion

        #region Events
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ringBar.IsActive = true;
            IterationsValue = Iterations.Text;
            ClusterCountValue = ClusterCount.Text;
            EpsilonValue = Epsilon.Text;
            AttemptsValue = Attempts.Text;
            ProcessImage.Visibility = Visibility.Hidden;
            backgroundWorker.RunWorkerAsync(OrginalImage.Source);
        }

        private void comboBoxImages_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = (sender as ComboBox).SelectedItem as BasicImage;
            if (item != null)
            {
                var pathUri = new Uri(string.Format(@"{0}", item.ImageUri), UriKind.Relative);
                OrginalImage.Source = new BitmapImage(pathUri);
            }

        }
        private void ComboBoxAlgorithm_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string comboBoxItem = ((sender as ComboBox).SelectedValue as string);
            if (comboBoxItem != null)
            {
                SelectedAlgorithm = comboBoxItem;
            }

        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs completedEventArgs)
        {
            ringBar.IsActive = false;
            ProcessImage.Visibility = Visibility.Visible;
            if (processImage != null)
                ProcessImage.Source = BitmapSourceConvert.ToBitmapSource(processImage);

        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs doWorkEvent)
        {
            try
            {

                switch (SelectedAlgorithm)
                {
                    case AlgorithmName.Manhattan:
                        source = ConvertSourceToImage(doWorkEvent.Argument as ImageSource);
                        processImage = Quantization(source);

                        float[] h1 = Histogram(processImage);
                        float[] h2 = Histogram(source);
                        float d = Algorithms.Manhattan(h1, h1);
                        d++;
                        break;
                    case AlgorithmName.Corelation:
                        throw new NotImplementedException();
                        break;
                    case AlgorithmName.Cosinus:
                        throw new NotImplementedException();
                        break;
                    case AlgorithmName.PrzekrojHistogramow:
                        throw new NotImplementedException();
                        break;
                    case AlgorithmName.Euklidesa:
                        throw new NotImplementedException();
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
