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
                // 1) Ensure database exists (connect to master)
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

                // 2) Ensure table exists
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

                // 3) Add seed data if table is empty
                SeedIfEmpty();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EnsureDatabaseAndTable failed: {ex}");
                throw;
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

        public void SeedIfEmpty()
        {
            if (HasAnyPoi()) return;

            AddPoi(new Poi
            {
                Name = "Brandenburger Tor",
                Latitude = 52.516275,
                Longitude = 13.377704,
                Description = "Wahrzeichen in Berlin",
                Image = "https://upload.wikimedia.org/wikipedia/commons/6/6e/Brandenburger_Tor_abends.jpg",
                WikiLink = "https://de.wikipedia.org/wiki/Brandenburger_Tor"
            });

            AddPoi(new Poi
            {
                Name = "Deutsches Museum",
                Latitude = 48.1303,
                Longitude = 11.5820,
                Description = "Museum in München",
                Image = "https://upload.wikimedia.org/wikipedia/commons/2/22/Deutsches_Museum_2010.jpg",
                WikiLink = "https://de.wikipedia.org/wiki/Deutsches_Museum"
            });

            AddPoi(new Poi
            {
                Name = "Hamburger Hafen",
                Latitude = 53.5511,
                Longitude = 9.9937,
                Description = "Größter Hafen Deutschlands",
                Image = "https://upload.wikimedia.org/wikipedia/commons/8/8b/Hamburg_Hafen.jpg",
                WikiLink = "https://de.wikipedia.org/wiki/Hamburger_Hafen"
            });

            AddPoi(new Poi
            {
                Name = "Neuschwanstein",
                Latitude = 47.5576,
                Longitude = 10.7498,
                Description = "Märchenschloss in Bayern",
                Image = "https://upload.wikimedia.org/wikipedia/commons/5/55/Neuschwanstein_Castle_LOC_print.jpg",
                WikiLink = "https://de.wikipedia.org/wiki/Schloss_Neuschwanstein"
            });
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
            var poiList = new List<GetSet>();
            const string sql = "SELECT poiID, name, latitude, longitude, description, image, wikilink FROM POI ORDER BY name";
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                poiList.Add(new GetSet
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
            return poiList;
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