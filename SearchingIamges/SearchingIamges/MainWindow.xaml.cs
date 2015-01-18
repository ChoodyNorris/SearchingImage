using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace SearchingIamges
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public ObservableCollection<BasicImage> ImagesList { get; private set; }

        public List<Accent> AccentColors { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            PrepareImage();
            comboBoxImages.ItemsSource = ImagesList;
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
            CvInvoke.cvKMeans2(samples, clusterCount, finalClusters, term, attempts, IntPtr.Zero, 2, IntPtr.Zero, IntPtr.Zero);

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

        private void Histogram(Image<Bgr,float> img)
        {
            float[] BlueHist;
            float[] GreenHist;
            float[] RedHist;

            DenseHistogram Histo = new DenseHistogram(255, new RangeF(0, 255));

            Image<Gray, float> img2Blue = img[0];
            Image<Gray, float> img2Green = img[1];
            Image<Gray, float> img2Red = img[2];

            Histo.Calculate(new Image<Gray, float>[] { img2Blue }, true, null);
            BlueHist = new float[256];
            Histo.MatND.ManagedArray.CopyTo(BlueHist, 0);

            Histo.Clear();

            Histo.Calculate(new Image<Gray, float>[] { img2Green }, true, null);
            GreenHist = new float[256];
            Histo.MatND.ManagedArray.CopyTo(GreenHist, 0);

            Histo.Clear();

            Histo.Calculate(new Image<Gray, float>[] { img2Red }, true, null);
            RedHist = new float[256];
            Histo.MatND.ManagedArray.CopyTo(RedHist, 0);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Image<Bgr, float> source = ConvertSourceToImage(OrginalImage.Source);
            Image<Bgr, float> processImage = Quantization(source);
            ProcessImage.Source = BitmapSourceConvert.ToBitmapSource(processImage);
            Histogram(processImage);
        }
        #endregion

        #region Events
        private void comboBoxImages_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var item = (sender as ComboBox).SelectedItem  as BasicImage;
            if (item != null)
            {
                var pathUri = new Uri(string.Format(@"{0}", item.ImageUri),UriKind.Relative);
                OrginalImage.Source = new BitmapImage(pathUri);
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
