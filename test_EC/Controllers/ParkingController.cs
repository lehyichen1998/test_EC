using Microsoft.AspNetCore.Mvc;
using test_EC.Models;
using Newtonsoft.Json;

namespace test_EC.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ParkingController : ControllerBase
    {
        private readonly ParkingDbContext _DbContext;
        private readonly HttpClient _httpClient;

        public ParkingController(ParkingDbContext parkingDbContext, IHttpClientFactory httpClientFactory)
        {
            _DbContext = parkingDbContext;
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet("ImportParkingData")]
        public async Task<ActionResult> ImportParkingData()
        {
            string apiUrl = "https://data.gov.sg/api/action/datastore_search?resource_id=d_23f946fa557947f93a8043bbef41dd09";

            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    APIModel.Root data = JsonConvert.DeserializeObject<APIModel.Root>(json);

                    if (data?.result?.records != null)
                    {
                        foreach (var record in data.result.records)
                        {
                            // Check if the record already exists based on _id (or another unique identifier)
                            if (!_DbContext.ParkingDetails.Any(p => p.car_park_no == record.car_park_no))
                            {
                                // Map API data to ParkingDetail
                                var parkingDetail = new ParkingDetail
                                {
                                    _id = record._id,
                                    car_park_no = record.car_park_no,
                                    address = record.address,
                                    x_coord = record.x_coord,
                                    y_coord = record.y_coord,
                                    car_park_type = record.car_park_type,
                                    type_of_parking_system = record.type_of_parking_system,
                                    short_term_parking = record.short_term_parking,
                                    free_parking = record.free_parking,
                                    night_parking = record.night_parking,
                                    car_park_decks = record.car_park_decks,
                                    gantry_height = record.gantry_height,
                                    car_park_basement = record.car_park_basement
                                };

                                // Add to DbContext
                                _DbContext.ParkingDetails.Add(parkingDetail);
                            }
                        }

                        // Save changes to the database
                        await _DbContext.SaveChangesAsync();

                        return Ok("Parking data imported successfully (duplicates skipped).");
                    }
                    else
                    {
                        return BadRequest("No records found in the API response.");
                    }
                }
                else
                {
                    return StatusCode((int)response.StatusCode, $"Error fetching data: {response.ReasonPhrase}");
                }
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
            catch (Newtonsoft.Json.JsonReaderException ex)
            {
                return StatusCode(500, $"Internal server error: Invalid JSON received: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("nearest")]
        public ActionResult<IEnumerable<ParkingDetail>> GetNearestCarParks(
            [FromQuery] double latitude,
            [FromQuery] double longitude,
            [FromQuery] int page = 1,
            [FromQuery] int per_page = 3)
        {
            if (latitude == 0 || longitude == 0)
            {
                return BadRequest("Latitude and Longitude are required.");
            }

            try
            {
                var carParks = _DbContext.ParkingDetails.ToList(); // Retrieve all parking details

                var nearestCarParks = carParks
                    .OrderBy(cp => CalculateDistance(latitude, longitude, ParseDouble(cp.y_coord), ParseDouble(cp.x_coord))) // Calculate distance and order
                    .Skip((page - 1) * per_page) // Pagination: skip to the correct page
                    .Take(per_page) // Pagination: take the specified number of items
                    .ToList();

                return Ok(nearestCarParks);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetParkingDetails")]
        public ActionResult<IEnumerable<ParkingDetail>> GetParkingDetails()
        {
            try
            {
                return _DbContext.ParkingDetails.ToList(); // Corrected: Return a list
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving parking details: {ex.Message}");
            }
        }

        [HttpGet("GetById/{Id:int}")]
        public async Task<ActionResult<ParkingDetail>> GetById(int Id)
        {
            var parkingDetail = await _DbContext.ParkingDetails.FindAsync(Id);

            if (parkingDetail == null)
            {
                return NotFound($"Parking detail with ID {Id} not found.");
            }

            return Ok(parkingDetail);
        }

        [HttpPost("Create")]
        public async Task<ActionResult> Create(ParkingDetailRequest request)
        {
            var parkingDetail = new ParkingDetail
            {
                _id = request._id,
                car_park_no = request.car_park_no,
                address = request.address,
                x_coord = request.x_coord,
                y_coord = request.y_coord,
                car_park_type = request.car_park_type,
                type_of_parking_system = request.type_of_parking_system,
                short_term_parking = request.short_term_parking,
                free_parking = request.free_parking,
                night_parking = request.night_parking,
                car_park_decks = request.car_park_decks,
                gantry_height = request.gantry_height,
                car_park_basement = request.car_park_basement
            };

            await _DbContext.ParkingDetails.AddAsync(parkingDetail);
            await _DbContext.SaveChangesAsync();

            return Ok("Parking detail created successfully.");
        }

        [HttpPut("Update")]
        public async Task<ActionResult> Update(ParkingDetail parkingDetail)
        {
            if (!_DbContext.ParkingDetails.Any(p => p.Id == parkingDetail.Id))
            {
                return NotFound($"Parking detail with ID {parkingDetail.Id} not found.");
            }

            _DbContext.ParkingDetails.Update(parkingDetail);
            await _DbContext.SaveChangesAsync();

            return Ok("Parking detail updated successfully.");
        }

        [HttpDelete("Delete/{Id:int}")]
        public async Task<ActionResult> Delete(int Id)
        {
            var parkingDetail = await _DbContext.ParkingDetails.FindAsync(Id);

            if (parkingDetail == null)
            {
                return NotFound($"Parking detail with ID {Id} not found.");
            }

            _DbContext.ParkingDetails.Remove(parkingDetail);
            await _DbContext.SaveChangesAsync();

            return Ok("Parking detail deleted successfully.");
        }

        

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371; // Radius of the earth in km
            var dLat = Deg2rad(lat2 - lat1);
            var dLon = Deg2rad(lon2 - lon1);
            var a =
                Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(Deg2rad(lat1)) * Math.Cos(Deg2rad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2)
                ;
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            var d = R * c; // Distance in km
            return d;
        }

        private double Deg2rad(double deg)
        {
            return deg * (Math.PI / 180);
        }

        private double ParseDouble(string value)
        {
            if (double.TryParse(value, out double result))
            {
                return result;
            }

            return 0; // Or throw an exception, or return null, depending on your error handling policy
        }
    }
}