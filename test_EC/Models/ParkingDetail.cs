using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace test_EC.Models
{
    [Table("parking_details")]
    public class ParkingDetail
    {
        [Key]
        [Column("Id")]
        public int Id { get; set; }
        [Column("_id")]
        public int _id { get; set; }
        [Column("car_park_no")]
        public string? car_park_no { get; set; }
        [Column("address")]
        public string? address { get; set; }
        [Column("x_coord")]
        public string? x_coord { get; set; }
        [Column("y_coord")]
        public string? y_coord { get; set; }
        [Column("car_park_type")]
        public string? car_park_type { get; set; }
        [Column("type_of_parking_system")]
        public string? type_of_parking_system { get; set; }
        [Column("short_term_parking")]
        public string? short_term_parking { get; set; }
        [Column("free_parking")]
        public string? free_parking { get; set; }
        [Column("night_parking")]
        public string? night_parking { get; set; }
        [Column("car_park_decks")]
        public string? car_park_decks { get; set; }
        [Column("gantry_height")]
        public string? gantry_height { get; set; }
        [Column("car_park_basement")]
        public string? car_park_basement { get; set; }
    }
}
