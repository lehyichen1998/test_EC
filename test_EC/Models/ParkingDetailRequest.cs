using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test_EC.Models
{
    public class ParkingDetailRequest
    {
        public int _id { get; set; }
        public string? car_park_no { get; set; }
        public string? address { get; set; }
        public string? x_coord { get; set; }
        public string? y_coord { get; set; }
        public string? car_park_type { get; set; }
        public string? type_of_parking_system { get; set; }
        public string? short_term_parking { get; set; }
        public string? free_parking { get; set; }
        public string? night_parking { get; set; }
        public string? car_park_decks { get; set; }
        public string? gantry_height { get; set; }
        public string? car_park_basement { get; set; }
    }
}
