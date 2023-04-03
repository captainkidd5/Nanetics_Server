using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Yelp
{
    public class YelpDTO
    {
    }

    public class AutoCompleteRequest
    {
        public string Name { get; set; }
        public int? Latitude { get; set; }
        public int? Longitude { get; set; }

    }

    public class AutoCompleteReponse
    {
        public int Total { get; set; }
        public List<YelpBusinessOverView> Businesses { get; set; }
    }

    public class YelpBusinessOverView
    {
        public int Rating { get; set; }
        public int Price { get; set; }
        public string Phone { get; set; }
        public string Id { get; set; }
        public List<YelpCategory> Categories { get; set; }
        public int Review_Count { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
        public Coordinates Coordinates { get; set; }
        public string Image_Url { get; set; }
        public Location Location { get; set; }
    }

    public class YelpCategory
    {
        public string Alias { get; set; }
        public string Title { get; set; }
    }

    public class Coordinates
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }

    public class Location
    {
        public string City { get; set; }
        public string Country { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string State { get; set; }
        public string Address1 { get; set; }
        public string Zip_Code { get; set; }

    }
}
