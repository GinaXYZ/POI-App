namespace POI_App
{
    internal class Poi
    {
        public int PoiID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Image { get; set; } = string.Empty;
        public string WikiLink { get; set; } = string.Empty;
    }
}
