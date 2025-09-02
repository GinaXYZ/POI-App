using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POI_App
{

    internal interface IRepository
    {
        List<GetSet> GetAllPOI();
        Poi GetPoiByID(int poiID);
        void AddPoi(Poi poi);
        void UpdatePoi(Poi poi);
        void DeletePoi(int poiID);

    }
}
