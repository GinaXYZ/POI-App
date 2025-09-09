using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace POI_App
{
    internal class SqlRepository : IRepository
    {
        private readonly string _connectionString;

        public SqlRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void EnsureDatabaseAndTable()
        {
            try
            {

                var builder = new SqlConnectionStringBuilder(_connectionString);
                var dbName = builder.InitialCatalog;
                var masterCs = new SqlConnectionStringBuilder(_connectionString)
                {
                    InitialCatalog = "master"
                }.ConnectionString;

                using (var conn = new SqlConnection(masterCs))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = $"IF DB_ID('{dbName}') IS NULL CREATE DATABASE [{dbName}];";
                    cmd.ExecuteNonQuery();
                }


                using (var conn = new SqlConnection(_connectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = @"IF OBJECT_ID('dbo.POI', 'U') IS NULL 
                                        BEGIN
                                            CREATE TABLE dbo.POI (
                                                poiID int not null primary key identity(1,1),
                                                [name] nvarchar(100) not null,
                                                latitude float not null,
                                                longitude float not null,
                                                [description] nvarchar(500) null,
                                                [image] nvarchar(255) null,
                                                wikilink nvarchar(255) null
                                            );
                                        END";
                    cmd.ExecuteNonQuery();
                }

                // Füge Testdaten hinzu, wenn die Tabelle leer ist
                if (!HasAnyPoi())
                {
                    AddSampleData();
                    System.Diagnostics.Debug.WriteLine("Sample data added to empty database");
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureDatabaseAndTable failed: {ex}");
                throw;
            }
        }

        private void AddSampleData()
        {
            var samplePois = new List<Poi>
            {
                new Poi
                {
                    Name = "Brandenburger Tor",
                    Latitude = 52.5163,
                    Longitude = 13.3777,
                    Description = "Das Brandenburger Tor ist ein klassizistisches Triumphtor in Berlin und eines der bekanntesten Wahrzeichen Deutschlands.",
                    Image = "https://upload.wikimedia.org/wikipedia/commons/thumb/a/a6/Brandenburger_Tor_abends.jpg/640px-Brandenburger_Tor_abends.jpg",
                    WikiLink = "https://de.wikipedia.org/wiki/Brandenburger_Tor"
                },
                new Poi
                {
                    Name = "Kölner Dom",
                    Latitude = 50.9413,
                    Longitude = 6.9583,
                    Description = "Der Kölner Dom ist eine römisch-katholische Kirche in Köln unter dem Patrozinium des Apostels Petrus.",
                    Image = "https://upload.wikimedia.org/wikipedia/commons/thumb/8/81/Cologne_Cathedral.jpg/640px-Cologne_Cathedral.jpg",
                    WikiLink = "https://de.wikipedia.org/wiki/Kölner_Dom"
                },
                new Poi
                {
                    Name = "Neuschwanstein",
                    Latitude = 47.5576,
                    Longitude = 10.7498,
                    Description = "Schloss Neuschwanstein steht oberhalb von Hohenschwangau bei Füssen im südöstlichen bayerischen Allgäu.",
                    Image = "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f8/Schloss_Neuschwanstein_2013.jpg/640px-Schloss_Neuschwanstein_2013.jpg",
                    WikiLink = "https://de.wikipedia.org/wiki/Schloss_Neuschwanstein"
                },
                new Poi
                {
                    Name = "Hamburger Hafen",
                    Latitude = 53.5459,
                    Longitude = 9.9687,
                    Description = "Der Hamburger Hafen ist ein Seehafen an der Unterelbe in der Freien und Hansestadt Hamburg.",
                    Image = "https://upload.wikimedia.org/wikipedia/commons/thumb/7/75/Berlin-Kreuzberg_Postkarte_055.jpg/640px-Berlin-Kreuzberg_Postkarte_055.jpg",
                    WikiLink = "https://de.wikipedia.org/wiki/Hamburger_Hafen"
                },
                new Poi
                {
                    Name = "Münchener Frauenkirche",
                    Latitude = 48.1385,
                    Longitude = 11.5732,
                    Description = "Die Frauenkirche in München ist die Kathedralkirche des Erzbistums München und Freising.",
                    Image = "https://upload.wikimedia.org/wikipedia/commons/thumb/d/d9/Frauenkirche_Munich_-_View_from_Peterskirche_Tower2.jpg/640px-Frauenkirche_Munich_-_View_from_Peterskirche_Tower2.jpg",
                    WikiLink = "https://de.wikipedia.org/wiki/Frauenkirche_(München)"
                }
            };

            foreach (var poi in samplePois)
            {
                AddPoi(poi);
            }
        }

        public bool HasAnyPoi()
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT TOP 1 1 FROM dbo.POI", conn);
            conn.Open();
            var result = cmd.ExecuteScalar();
            return result != null && result != DBNull.Value;
        }

     

        public Poi GetPoiByID(int poiID)
        {
            const string sql = "SELECT poiID, name, latitude, longitude, description, image, wikilink FROM POI WHERE poiID = @poiID";
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@poiID", poiID);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Poi
                {
                    PoiID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Latitude = reader.GetDouble(2),
                    Longitude = reader.GetDouble(3),
                    Description = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Image = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    WikiLink = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                };
            }
            throw new InvalidOperationException($"POI with id {poiID} not found");
        }

        public void AddPoi(Poi poi)
        {
            const string sql = "INSERT INTO POI (name, latitude, longitude, description, image, wikilink) VALUES (@name, @latitude, @longitude, @description, @image, @wikilink)";
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", poi.Name);
            cmd.Parameters.AddWithValue("@latitude", poi.Latitude);
            cmd.Parameters.AddWithValue("@longitude", poi.Longitude);
            cmd.Parameters.AddWithValue("@description", string.IsNullOrWhiteSpace(poi.Description) ? (object)DBNull.Value : poi.Description);
            cmd.Parameters.AddWithValue("@image", string.IsNullOrWhiteSpace(poi.Image) ? (object)DBNull.Value : poi.Image);
            cmd.Parameters.AddWithValue("@wikilink", string.IsNullOrWhiteSpace(poi.WikiLink) ? (object)DBNull.Value : poi.WikiLink);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public void DeletePoi(int poiID)
        {
            const string sql = "DELETE FROM POI WHERE poiID = @poiID";
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@poiID", poiID);
            conn.Open();
            cmd.ExecuteNonQuery();
        }

        public List<GetSet> GetAllPOI()
        {
            var pois = new List<GetSet>();
            const string sql = "SELECT poiID, name, latitude, longitude, description, image, wikilink FROM POI ORDER BY name";
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                pois.Add(new GetSet
                {
                    PoiID = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Latitude = reader.GetDouble(2),
                    Longitude = reader.GetDouble(3),
                    Description = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    Image = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                    WikiLink = reader.IsDBNull(6) ? string.Empty : reader.GetString(6)
                });
            }
            return pois;
        }

        public void UpdatePoi(Poi poi)
        {
            const string sql = "UPDATE POI SET name = @name, latitude = @latitude, longitude = @longitude, description = @description, image = @image, wikilink = @wikilink WHERE poiID = @poiID";
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@name", poi.Name);
            cmd.Parameters.AddWithValue("@latitude", poi.Latitude);
            cmd.Parameters.AddWithValue("@longitude", poi.Longitude);
            cmd.Parameters.AddWithValue("@description", string.IsNullOrWhiteSpace(poi.Description) ? (object)DBNull.Value : poi.Description);
            cmd.Parameters.AddWithValue("@image", string.IsNullOrWhiteSpace(poi.Image) ? (object)DBNull.Value : poi.Image);
            cmd.Parameters.AddWithValue("@wikilink", string.IsNullOrWhiteSpace(poi.WikiLink) ? (object)DBNull.Value : poi.WikiLink);
            cmd.Parameters.AddWithValue("@poiID", poi.PoiID);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}