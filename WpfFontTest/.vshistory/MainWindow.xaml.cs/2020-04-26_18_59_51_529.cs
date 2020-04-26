using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfFontTest
{
    sealed class MemoryPackage : IDisposable
    {
        private static int packageCounter;

        private readonly Uri packageUri = new Uri("payload://memorypackage" + Interlocked.Increment(ref packageCounter), UriKind.Absolute);
        private readonly Package package = Package.Open(new MemoryStream(), FileMode.Create);
        private int partCounter;

        public MemoryPackage()
        {
            PackageStore.AddPackage(this.packageUri, this.package);
        }

        public Uri CreatePart(Stream stream)
        {
            return this.CreatePart(stream, "application/octet-stream");
        }

        public Uri CreatePart(Stream stream, string contentType)
        {
            var partUri = new Uri("/stream" + (++this.partCounter) + ".ttf", UriKind.Relative);

            var part = this.package.CreatePart(partUri, contentType);

            using (var partStream = part.GetStream())
                CopyStream(stream, partStream);

            // Each packUri must be globally unique because WPF might perform some caching based on it.
            return PackUriHelper.Create(this.packageUri, partUri);
        }

        public void DeletePart(Uri packUri)
        {
            this.package.DeletePart(PackUriHelper.GetPartUri(packUri));
        }

        public void Dispose()
        {
            PackageStore.RemovePackage(this.packageUri);
            this.package.Close();
        }

        private static void CopyStream(Stream source, Stream destination)
        {
            const int bufferSize = 4096;

            byte[] buffer = new byte[bufferSize];
            int read;
            while ((read = source.Read(buffer, 0, buffer.Length)) != 0)
                destination.Write(buffer, 0, read);
        }
    }



    public partial class MainWindow : Window
    {

        string fontName;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            refreshFonts();
        }

        private void btn_refreshcombo_Click(object sender, RoutedEventArgs e)
        {
            refreshFonts();
        }

        private void btn_setfont_Click(object sender, RoutedEventArgs e)
        {
            if (cmb_fonts.SelectedIndex == -1)
            {
                return;
            }
            // Get fonts names
            using (var memoryPackage = new MemoryPackage())
            {
                using (var fontStream = new MemoryStream(File.ReadAllBytes(AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/") + cmb_fonts.SelectedItem.ToString())))
                {
                    GlyphTypeface glyphTypeface;
                    var typefaceSource = memoryPackage.CreatePart(fontStream);
                    glyphTypeface = new GlyphTypeface(typefaceSource);
                    fontName = String.Join(" ", glyphTypeface.FamilyNames.Values.ToArray<string>());
                    lbl_textFont.Text = "Selected Font Name: " + fontName;
                    memoryPackage.DeletePart(typefaceSource);
                }
            }
            //
            lbl_text.FontFamily = new FontFamily("file:///" + AppDomain.CurrentDomain.BaseDirectory + cmb_fonts.SelectedItem.ToString().Substring(1) + "#" + fontName);
            lbl_text2.FontFamily = new FontFamily("file:///" + AppDomain.CurrentDomain.BaseDirectory + cmb_fonts.SelectedItem.ToString().Substring(1) + "#" + fontName);
            lbl_text3.FontFamily = new FontFamily("file:///" + AppDomain.CurrentDomain.BaseDirectory + cmb_fonts.SelectedItem.ToString().Substring(1) + "#" + fontName);
            //lbl_textFont2.Text = "Textblock said it's: " + String.Join(" ", lbl_text.FontFamily.FamilyNames.Values.ToArray<string>());
        }

        private void btn_addWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindow tempwindow = new MainWindow();
            tempwindow.Show();
        }

        public void refreshFonts()
        {
            cmb_fonts.Items.Clear();
            Directory.CreateDirectory(".\\fonts");
            foreach (var item in Directory.GetFiles(".\\fonts"))
            {
                cmb_fonts.Items.Add(item.ToString());
            }
        }
    }
}
