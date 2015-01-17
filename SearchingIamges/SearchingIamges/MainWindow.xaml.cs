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

        ImageSource imgSource;
        Bitmap bmp;
        Image<Bgr, float> source;

        private void InitializeImage()
        {
            imgSource = OrginalImage.Source;
            bmp = BitmapSourceConvert.ToBitmap(imgSource as BitmapSource);
            source = new Image<Bgr, float>(bmp);
        }


        Matrix<float> samples;
        Matrix<int> finalClusters;

        private void InitializeStructuresToProcess()
        {
            samples = new Matrix<float>(source.Rows * source.Cols, 1, 3);
            finalClusters = new Matrix<int>(source.Rows * source.Cols, 1);

            for (int y = 0; y < source.Rows; y++)
            {
                for (int x = 0; x < source.Cols; x++)
                {
                    samples.Data[y + x * source.Rows, 0] = (float)source[y, x].Blue;
                    samples.Data[y + x * source.Rows, 1] = (float)source[y, x].Green;
                    samples.Data[y + x * source.Rows, 2] = (float)source[y, x].Red;
                }
            }
        }

        MCvTermCriteria term;

        private void Quantization()
        {
            MCvTermCriteria term = new MCvTermCriteria(Convert.ToInt32(Iterations.Text), Convert.ToDouble(Epsilon.Text));
            term.type = TERMCRIT.CV_TERMCRIT_ITER | TERMCRIT.CV_TERMCRIT_EPS;

            int clusterCount = Int32.Parse(ClusterCount.Text);
            int attempts = Int32.Parse(Attempts.Text);
            Matrix<Single> centers = new Matrix<Single>(clusterCount, source.Rows * source.Cols);
            CvInvoke.cvKMeans2(samples, clusterCount, finalClusters, term, attempts, IntPtr.Zero, 2, IntPtr.Zero, IntPtr.Zero);
        }

        Image<Bgr, float> processImage;

        private void ReductingNumberColor()
        {
            processImage = new Image<Bgr, float>(source.Size);

            double max;
            double min;
            System.Drawing.Point pmin;
            System.Drawing.Point pmax;
            finalClusters.MinMax(out min, out max, out pmin, out pmax);
            int difference = (int)(max - min) + 1;
            Bgr[] clusterColors = new Bgr[difference];
            int maxValueColor = 255;

            for (int i = 0; i < difference; i++)
            {
                int div = maxValueColor / (difference - 1);
                clusterColors[i] = new Bgr(i * div, i * div, i * div);
            }

            for (int y = 0; y < source.Rows; y++)
            {
                for (int x = 0; x < source.Cols; x++)
                {
                    PointF p = new PointF(x, y);
                    processImage.Draw(new CircleF(p, 0.1f), clusterColors[finalClusters[y + x * source.Rows, 0]], 1);
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            InitializeImage();

            InitializeStructuresToProcess();

            Quantization();

            ReductingNumberColor();

            ProcessImage.Source = BitmapSourceConvert.ToBitmapSource(processImage);
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
