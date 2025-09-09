# Test der POI-Datenbank
$connectionString = "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=POI;Integrated Security=True;TrustServerCertificate=True"

try {
    Write-Host "=== Database Test Start ===" -ForegroundColor Green
    
    # Verbindung testen
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    Write-Host "Database connection successful" -ForegroundColor Green
    
    # Prüfen ob Tabelle existiert
    $cmd = $connection.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'POI'"
    $tableExists = $cmd.ExecuteScalar()
    Write-Host "Table POI exists: $($tableExists -eq 1)" -ForegroundColor Yellow
    
    if ($tableExists -eq 1) {
        # Anzahl der Einträge prüfen
        $cmd.CommandText = "SELECT COUNT(*) FROM POI"
        $count = $cmd.ExecuteScalar()
        Write-Host "Number of POIs in database: $count" -ForegroundColor Yellow
        
        if ($count -gt 0) {
            # Erste paar Einträge anzeigen
            $cmd.CommandText = "SELECT TOP 5 poiID, name, latitude, longitude FROM POI"
            $reader = $cmd.ExecuteReader()
            Write-Host "Sample POIs:" -ForegroundColor Cyan
            while ($reader.Read()) {
                Write-Host "  ID: $($reader[0]), Name: $($reader[1]), Lat: $($reader[2]), Lon: $($reader[3])" -ForegroundColor White
            }
            $reader.Close()
        }
    }
    
    $connection.Close()
    Write-Host "=== Database Test End ===" -ForegroundColor Green
}
catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}
