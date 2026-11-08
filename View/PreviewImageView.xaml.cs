using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace eSufara.View
{
    /// <summary>
    /// Interaction logic for PreviewImageView.xaml
    /// </summary>
    public partial class PreviewImageView : Window
    {
        public PreviewImageView(BitmapImage image)
        {
            InitializeComponent();
            PreviewImage.Source = image;
        }
    }
}
