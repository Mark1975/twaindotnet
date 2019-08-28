using System;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using Microsoft.Win32;
using TwainDotNet;
using TwainDotNet.TwainNative;
using TwainDotNet.Wpf;
using TwainDotNet.Win32;

namespace TestAppWpf
{
    public partial class Window1 : Window
    {
        private static AreaSettings AreaSettings = new AreaSettings(0.1f, 5.7f, 0.1F + 2.6f, 5.7f + 2.6f);

        private Twain _twain;
        private ScanSettings _settings;

        private Bitmap resultImage;

        public Window1()
        {
            InitializeComponent();

            Loaded += delegate
            {
                _twain = new Twain(new WpfWindowMessageHook(this));
                _twain.TransferImage += delegate(Object sender, TransferImageEventArgs args)
                {
                    MainImage.Source = null;

                    if( args.HBitmap != IntPtr.Zero)
                    {
                        using( var renderer = new BitmapRenderer( args.HBitmap ) )
                        {
                            resultImage = renderer.RenderToBitmap();
                        }
                        IntPtr hbitmap = new Bitmap(resultImage).GetHbitmap();
                        MainImage.Source = Imaging.CreateBitmapSourceFromHBitmap(
                                hbitmap,
                                IntPtr.Zero,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());
                        Gdi32Native.DeleteObject(hbitmap);
                    }

                    if( args.MemoryTransferData != null )
                    {
                        Image image = ImageParser.ParseImage( args.MemoryTransferData );

                        if( image != null )
                        {
                            Bitmap bitmap = new Bitmap( image );
                            using( var ms = new MemoryStream() )
                            {
                                bitmap.Save( ms, System.Drawing.Imaging.ImageFormat.Bmp );
                                ms.Seek( 0, SeekOrigin.Begin );

                                var bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                                bitmapImage.StreamSource = ms;
                                bitmapImage.EndInit();

                                MainImage.Source = bitmapImage;
                            }
                        }
                    }

                    MainImage.InvalidateVisual();
                };
                _twain.ScanningComplete += delegate(Object sender, ScanningCompleteEventArgs args)
                {
					if( args.Exception != null )
					{
						MessageBox.Show( args.Exception.Message );
					}
					IsEnabled = true;
                };

                var sourceList = _twain.SourceNames;
                ManualSource.ItemsSource = sourceList;

                if (sourceList != null && sourceList.Count > 0)
                    ManualSource.SelectedItem = sourceList[0];
            };
        }

        private void OnSelectSourceButtonClick(object sender, RoutedEventArgs e)
        {
            _twain.SelectSource();
        }

        private void scanButton_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;

            try
            {
                if (SourceUserSelected.IsChecked == true)
                    _twain.SelectSource(ManualSource.SelectedItem.ToString());

                _twain.DebugCapabilities();
                //_settings = new ScanSettings { ShouldTransferAllPages = true, ShowTwainUI = true, };

                _settings = new ScanSettings
                {
                    UseDocumentFeeder = UseAdfCheckBox.IsChecked,
                    UseAutoFeeder = UseAdfCheckBox.IsChecked,
                    ShowTwainUI = UseUICheckBox.IsChecked ?? false,
                    ShowProgressIndicatorUI = ShowProgressCheckBox.IsChecked,
                    UseDuplex = UseDuplexCheckBox.IsChecked,
                    Units = Units.Inches,
                    Area = !( GrabAreaCheckBox.IsChecked ?? false ) ? null : AreaSettings,
                    ShouldTransferAllPages = true,
                    AutomaticRotate = AutoRotateCheckBox.IsChecked ?? false,
                    AutomaticBorderDetection = AutoDetectBorderCheckBox.IsChecked ?? false,
                    DataTransferMode = MemoryTransferMechanism.IsChecked.GetValueOrDefault() ? TransferMechanism.Memory : TransferMechanism.Native,
                };
                GetResolutionSettings( _settings );

                _twain.StartScanning(_settings);
            }
            catch (TwainException ex)
            {
                MessageBox.Show(ex.Message);
            }

            IsEnabled = true;
        }

        private void GetResolutionSettings( ScanSettings scanSettings )
        {
            ColourSetting colourSetting = ColourSetting.Default;
            float? dpi = null;
            if( ColourBlackAndWhite.IsChecked.GetValueOrDefault() )
            {
                colourSetting = ColourSetting.BlackAndWhite;
                dpi = 200;
            }
            else if( ColourGrayscale.IsChecked.GetValueOrDefault() )
            {
                colourSetting = ColourSetting.GreyScale;
                dpi = 300;
            }
            else if( ColourColour.IsChecked.GetValueOrDefault() )
            {
                colourSetting = ColourSetting.Colour;
                dpi = 300;
            }

            scanSettings.ColourSetting = colourSetting;
            scanSettings.Dpi = dpi;
        }

        private void OnSaveButtonClick(object sender, RoutedEventArgs e)
        {
            if (resultImage != null)
            {
                var saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == true)
                    resultImage.Save(saveFileDialog.FileName);
            }
        }
    }
}
