using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Markup;
using System;

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
                Console.WriteLine("Database and table initialized successfully");
                System.Diagnostics.Debug.WriteLine("Database and table initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization failed: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Database initialization failed: {ex.Message}");
                MessageBox.Show($"Datenbankfehler: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            SetupTextBoxHandlers();
        }

        private void SetupTextBoxHandlers()
        {
            TxtPoiName.GotFocus += (s, e) => { if (TxtPoiName.Text == "Name") TxtPoiName.Text = ""; };
            TxtPoiLatitude.GotFocus += (s, e) => { if (TxtPoiLatitude.Text == "Latitude") TxtPoiLatitude.Text = ""; };
            TxtPoiLongitude.GotFocus += (s, e) => { if (TxtPoiLongitude.Text == "Longitude") TxtPoiLongitude.Text = ""; };
            TxtPoiDesc.GotFocus += (s, e) => { if (TxtPoiDesc.Text == "Beschreibung") TxtPoiDesc.Text = ""; };
            TxtPoiImage.GotFocus += (s, e) => { if (TxtPoiImage.Text == "Bild-URL") TxtPoiImage.Text = ""; };
            TxtPoiWiki.GotFocus += (s, e) => { if (TxtPoiWiki.Text == "Wikipedia-Link") TxtPoiWiki.Text = ""; };
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
                LblName.Content = poi.Name;
                LblLatitude.Content = poi.Latitude.ToString("F6");
                LblLongitude.Content = poi.Longitude.ToString("F6");
                LblDescription.Text = string.IsNullOrWhiteSpace(poi.Description) ? "Keine Beschreibung verfügbar" : poi.Description;

                await Karte.EnsureCoreWebView2Async();
                if (poi.Latitude != 0 && poi.Longitude != 0)
                {
                    Karte.Source = new System.Uri($"https://www.openstreetmap.org/#map=14/{poi.Latitude}/{poi.Longitude}");
                }

                if (!string.IsNullOrWhiteSpace(poi.Image))
                {
                    try
                    {
                        BitmapImage bmp = null;

                        if (poi.Image.StartsWith("http://") || poi.Image.StartsWith("https://"))
                        {
                            bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.UriSource = new System.Uri(poi.Image, System.UriKind.Absolute);
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.EndInit();
                        }
                        else if (System.IO.File.Exists(poi.Image))
                        {
                            bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.UriSource = new System.Uri(poi.Image, System.UriKind.Absolute);
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.EndInit();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Image path not found: {poi.Image}");
                            LoadFallbackImage();
                            return;
                        }

                        MyImage.Source = bmp;
                        System.Diagnostics.Debug.WriteLine($"Image loaded successfully: {poi.Image}");
                    }
                    catch (Exception imgEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Image loading failed: {imgEx.Message}");
                        LoadFallbackImage();
                    }
                }
                else
                {
                    LoadFallbackImage();
                }
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdateMapAndImage failed: {ex}");
            }
        }

        private void LoadFallbackImage()
        {
            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new System.Uri("https://upload.wikimedia.org/wikipedia/commons/7/75/Berlin-Kreuzberg_Postkarte_055.jpg", System.UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                MyImage.Source = bmp;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Fallback image loading failed: {ex.Message}");
                MyImage.Source = null;
            }
        }

        private async void MainWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                await Karte.EnsureCoreWebView2Async();
                Karte.Source = new System.Uri("https://www.openstreetmap.org/");

                bool hasData = _repo!.HasAnyPoi();

                _pois = _repo!.GetAllPOI();
                if (_pois.Count > 0)
                {
                    foreach (var poi in _pois)
                    {
                    }
                }
                else
                {
                }

                CbxPoi.DisplayMemberPath = nameof(GetSet.Name);
                CbxPoi.ItemsSource = _pois;

                LoadFallbackImage();

                Karte.NavigationCompleted += (s, ev) =>
                {
                    if (!ev.IsSuccess)
                    {
                    }
                    else
                    {
                    }
                };
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"MainWindow_Loaded failed: {ex}");
                MessageBox.Show($"Fehler beim Laden: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnPoiLoeschen_Click(object sender, RoutedEventArgs e)
        {
            if (CbxPoi.SelectedItem is GetSet selectedPoi)
            {
                var result = MessageBox.Show($"Möchten Sie die POI '{selectedPoi.Name}' wirklich löschen?", "POI löschen", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _repo!.DeletePoi(selectedPoi.PoiID);
                        
                        _pois = _repo!.GetAllPOI();
                        CbxPoi.ItemsSource = null;
                        CbxPoi.ItemsSource = _pois;
                        
                        LblName.Content = "Wählen Sie eine POI aus...";
                        LblLatitude.Content = "-";
                        LblLongitude.Content = "-";
                        LblDescription.Text = "Wählen Sie eine POI aus, um die Beschreibung zu sehen...";
                        LoadFallbackImage();
                        
                        MessageBox.Show("POI erfolgreich gelöscht!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Fehler beim Löschen der POI: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Bitte wählen Sie eine POI zum Löschen aus.", "Keine POI ausgewählt", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void BtnPoiSpeichern_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TxtPoiName.Text) || TxtPoiName.Text == "Name")
                {
                    MessageBox.Show("Bitte geben Sie einen Namen für die POI ein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!double.TryParse(TxtPoiLatitude.Text, out double latitude) || TxtPoiLatitude.Text == "Latitude")
                {
                    MessageBox.Show("Bitte geben Sie eine gültige Latitude ein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!double.TryParse(TxtPoiLongitude.Text, out double longitude) || TxtPoiLongitude.Text == "Longitude")
                {
                    MessageBox.Show("Bitte geben Sie eine gültige Longitude ein.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var newPoi = new Poi
                {
                    Name = TxtPoiName.Text.Trim(),
                    Latitude = latitude,
                    Longitude = longitude,
                    Description = (TxtPoiDesc.Text == "Beschreibung") ? "" : TxtPoiDesc.Text.Trim(),
                    Image = (TxtPoiImage.Text == "Bild-URL") ? "" : TxtPoiImage.Text.Trim(),
                    WikiLink = (TxtPoiWiki.Text == "Wikipedia-Link") ? "" : TxtPoiWiki.Text.Trim()
                };

                _repo!.AddPoi(newPoi);

                _pois = _repo!.GetAllPOI();
                CbxPoi.ItemsSource = null;
                CbxPoi.ItemsSource = _pois;

                TxtPoiName.Text = "Name";
                TxtPoiLatitude.Text = "Latitude";
                TxtPoiLongitude.Text = "Longitude";
                TxtPoiDesc.Text = "Beschreibung";
                TxtPoiImage.Text = "Bild-URL";
                TxtPoiWiki.Text = "Wikipedia-Link";

                MessageBox.Show("POI erfolgreich gespeichert!", "Erfolg", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Speichern der POI: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Bild auswählen",
                    Filter = "Bilddateien (*.jpg, *.jpeg, *.png, *.gif, *.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Alle Dateien (*.*)|*.*",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedFilePath = openFileDialog.FileName;

                    TxtPoiImage.Text = selectedFilePath;

                    MessageBox.Show($"Bild ausgewählt: {System.IO.Path.GetFileName(selectedFilePath)}", "Bild hinzugefügt", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Auswählen des Bildes: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuItem_Imp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = "Datei importieren",
                    Filter = "CSV-Dateien (*.csv)|*.csv|JSON-Dateien (*.json)|*.json|Alle Dateien (*.*)|*.*",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    string extension = System.IO.Path.GetExtension(filePath).ToLower();

                    if (extension == ".csv")
                    {
                        ImportCsv(filePath);
                    }
                    else if (extension == ".json")
                    {
                        ImportJson(filePath);
                    }
                    else
                    {
                        MessageBox.Show("Nicht unterstütztes Dateiformat.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Importieren: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ImportCsv(string filePath)
        {
            var lines = System.IO.File.ReadAllLines(filePath);
            int importedCount = 0;

            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length >= 5)
                {
                    try
                    {
                        var poi = new Poi
                        {
                            Name = parts[0].Trim('"'),
                            Latitude = double.Parse(parts[1]),
                            Longitude = double.Parse(parts[2]),
                            Description = parts.Length > 3 ? parts[3].Trim('"') : "",
                            Image = parts.Length > 4 ? parts[4].Trim('"') : "",
                            WikiLink = parts.Length > 5 ? parts[5].Trim('"') : ""
                        };

                        _repo!.AddPoi(poi);
                        importedCount++;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error importing POI: {ex.Message}");
                    }
                }
            }

            _pois = _repo!.GetAllPOI();
            CbxPoi.ItemsSource = null;
            CbxPoi.ItemsSource = _pois;

            MessageBox.Show($"{importedCount} POIs erfolgreich importiert!", "Import abgeschlossen", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ImportJson(string filePath)
        {
            var json = System.IO.File.ReadAllText(filePath);
            var pois = System.Text.Json.JsonSerializer.Deserialize<List<Poi>>(json);
            int importedCount = 0;

            if (pois != null)
            {
                foreach (var poi in pois)
                {
                    try
                    {
                        _repo!.AddPoi(poi);
                        importedCount++;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error importing POI: {ex.Message}");
                    }
                }
            }

            _pois = _repo!.GetAllPOI();
            CbxPoi.ItemsSource = null;
            CbxPoi.ItemsSource = _pois;

            MessageBox.Show($"{importedCount} POIs erfolgreich importiert!", "Import abgeschlossen", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItem_Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Datei exportieren",
                    Filter = "CSV-Dateien (*.csv)|*.csv",
                    DefaultExt = "csv",
                    FileName = "pois_export.csv"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    ExportCsv(saveFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Exportieren: {ex.Message}", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportCsv(string filePath)
        {
            var csvLines = new List<string>
            {
                "\"Name\",\"Latitude\",\"Longitude\",\"Description\",\"Image\",\"WikiLink\""
            };

            foreach (var poi in _pois)
            {
                var line = $"\"{poi.Name}\",\"{poi.Latitude}\",\"{poi.Longitude}\",\"{poi.Description}\",\"{poi.Image}\",\"{poi.WikiLink}\"";
                csvLines.Add(line);
            }

            System.IO.File.WriteAllLines(filePath, csvLines);
            MessageBox.Show($"Daten erfolgreich nach {filePath} exportiert!", "Export abgeschlossen", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}