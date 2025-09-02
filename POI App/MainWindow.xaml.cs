using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace POI_App
{
    public partial class MainWindow : Window
    {
        private SqlRepository? _repo;
        private List<GetSet> _pois = new();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;

            string connectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=POI;Integrated Security=True;TrustServerCertificate=True";
            _repo = new SqlRepository(connectionString);

            try
            {
                _repo.EnsureDatabaseAndTable();
                System.Diagnostics.Debug.WriteLine("Database and table initialized successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
                MessageBox.Show($"Datenbankfehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CbxPoi_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CbxPoi.SelectedItem is GetSet selected)
            {
                UpdateMapAndImage(selected);
            }
        }

        private async void UpdateMapAndImage(GetSet poi)
        {
            try
            {
                await Karte.EnsureCoreWebView2Async();
                if (poi.Latitude != 0 && poi.Longitude != 0)
                {
                    Karte.Source = new System.Uri($"https://www.openstreetmap.org/#map=14/{poi.Latitude}/{poi.Longitude}");
                }

                if (!string.IsNullOrWhiteSpace(poi.Image))
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource = new System.Uri(poi.Image, System.UriKind.Absolute);
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    MyImage.Source = bmp;
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateMapAndImage failed: {ex}");
            }
        }

        private async void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                await Karte.EnsureCoreWebView2Async();
                Karte.Source = new System.Uri("https://www.openstreetmap.org/");

                _pois = _repo!.GetAllPOI();
                CbxPoi.DisplayMemberPath = nameof(GetSet.Name);
                CbxPoi.ItemsSource = _pois;

                System.Diagnostics.Debug.WriteLine($"Loaded {_pois.Count} POIs from database");


                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new System.Uri("https://upload.wikimedia.org/wikipedia/commons/7/75/Berlin-Kreuzberg_Postkarte_055.jpg", System.UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                MyImage.Source = bmp;

                Karte.NavigationCompleted += (s, ev) =>
                {
                    if (!ev.IsSuccess)
                        System.Diagnostics.Debug.WriteLine($"WebView2 Navigation failed: {ev.WebErrorStatus}");
                    else
                        System.Diagnostics.Debug.WriteLine("WebView2 Navigation successful");
                };
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow_Loaded failed: {ex}");
                MessageBox.Show($"Fehler beim Laden: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}