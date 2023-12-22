using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Point_Internal_API.ViewModel;
using Point_Internal_API.Models;

namespace Point_Internal_API.Controllers
{
    [RoutePrefix("api/barging_online")]
    public class BargingOnlineController : ApiController
    {
        DataClassesDataContext db = new DataClassesDataContext();


        
        [Route("Get_BargingOnline")]
        [HttpGet]
        public IHttpActionResult GetBargingOnline([FromUri] RequestData request)
        {
            try
            {
                List<ClsBarginOnline> result = new List<ClsBarginOnline>();

                using (var db = new DataClassesDataContext())
                {
                    var companies = db.TBL_M_COMPANies.Select(c => new { id = c.ID, name = c.CUSTOMER }).ToList();
                    var tugBoats = db.TBL_M_TUGBOATs.Select(tb => new { id = tb.ID, name = tb.TUG_BOAT }).ToList();
                    var barges = db.TBL_M_BARGEs.Select(b => new { id = b.ID, name = b.BARGE }).ToList();
                    var jetties = db.cusp_get_list_jetty().ToList();
                    var capacities = db.TBL_M_JETTies.Select(b => new { id = b.ID, name = b.NAME, capacity = b.CAPACITY, duration = b.DURATION }).ToList();

                    var test = new
                    {
                        company = companies,
                        tugBoat = tugBoats,
                        barge = barges,
                        jetty = jetties,
                        capacity = capacities
                    };

                    return Ok(new { Data = test, Status = true, Message = "Data Berhasil Diambil!!!" });
                }
            }
            catch(Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = e.Message });
            }
        }


        //get bargin online per id (yang telah di booking)
        [Route("Get_BargininOnlineByUserId/{userId}")]
        [HttpGet]
        public IHttpActionResult GetBargingOnlineByUserId(int userId)
        {
            try
            {
                var bargingOnlineList = db.TBL_T_BARGING_ONLINEs
                    .Where(b => b.ID_USER == userId)
                    .OrderByDescending(b => b.ID)
                    .ToList();

                if (bargingOnlineList == null)
                {
                    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Id User not found!!" });
                }

                return Ok(new { Data = bargingOnlineList, Status = true, Message = "Data Berhasil diambil per id user" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Data tidak ditemukan!!!" });
            }
        }

        [Route("Get_ActiveBarging")]
        [HttpGet]
        public IHttpActionResult Get_ActiveBarging()
        {
            try
            {
                ClsActiveBarging ActiveBarging = new ClsActiveBarging();
                var data = db.cusp_get_progress_barging_mobile().ToList();

                var result = data.Select(item => new
                {
                    item.nodeDesc,
                    item.Company,
                    item.Barge,
                    item.Boat,
                    item.Planload,
                    item.Weight,
                    item.WeightPercentage,
                    item.Kode
                }).ToList();

                return Ok(new { Data = result, Status = true, Message = "Data berhasil diambil" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Terjadi kesalahan dalam mengambil data!!!" });
            }
        }

        //barging rekap
        [Route("Get_BargingOnlineByFilter/{id}")]
        [HttpGet]
        public IHttpActionResult Get_BargingScheule(int id, DateTime startDate, DateTime endDate)
        {
            try
            {
                db.cusp_UpdateStatusSelesaiBarging();

                List<VW_REQUEST_BARGING> bargingOnlineList = new List<VW_REQUEST_BARGING>();

                bargingOnlineList = db.VW_REQUEST_BARGINGs
                    .Where(b => b.ID_USER == id && b.STATUS == "Selesai" && b.DATE_BOOKING.HasValue && b.DATE_BOOKING.Value.Date >= startDate.Date && b.DATE_BOOKING.Value.Date <= endDate.Date)
                    .ToList();

                if (bargingOnlineList == null || bargingOnlineList.Count == 0)
                {
                    return Ok(new { Data = new List<VW_REQUEST_BARGING>(), Status = true, Message = "Data tidak ditemukan!" });
                }

                return Ok(new { Data = bargingOnlineList, Status = true, Message = "Data berhasil diambil berdasarkan ID dan rentang tanggal" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Terjadi kesalahan!" });
            }
        }

        //barging schedule
        [Route("Get_BargingSchedule/{id}")]
        [HttpGet]
        public IHttpActionResult GetBargingOnlineByDate(int id, DateTime startDate, DateTime endDate)
        {
            try
            {
                List<VW_REQUEST_BARGING> bargingOnlineList = db.VW_REQUEST_BARGINGs
                    .Where(b => b.ID_USER == id && b.STATUS == "diterima" && b.DATE_BOOKING.HasValue && b.DATE_BOOKING.Value.Date >= startDate.Date && b.DATE_BOOKING.Value.Date <= endDate.Date)
                    .ToList();

                if (bargingOnlineList == null || bargingOnlineList.Count == 0)
                {
                    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Data tidak ditemukan!" });
                }

                var result = bargingOnlineList
                    .GroupBy(b => b.DATE_BOOKING.Value.Date)
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Data = g.ToList()
                    })
                    .ToList();

                return Ok(new
                {
                    Data = result,
                    Status = true,
                    Message = "Data berhasil diambil!!!"
                });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Terjadi kesalahan!" });
            }
        }

        //Barging Request
        [Route("Create_BarginOnline")]
        [HttpPost]
        public IHttpActionResult Create_BargingOnline(ClsCreateBarginOnline param)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Terdapat data yang kosong!!!" });
                }

                // Retrieve the ID_Booking from another table using a query or join statement
                var booking = db.TBL_M_USERs.SingleOrDefault(b => b.ID == param.Id_User);
                if (booking == null)
                {
                    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Id User not found!!" });
                }

                // Set FinishTime automatically based on StartTime and ProcessTime
                int finishTime = param.StartTime + param.ProcessTime;
                DateTime? finishBooking = null;

                #region LOGIC BEFORE

                //if (finishTime > 24)
                //{
                //    finishTime -= 24;
                //    finishBooking = param.DateBooking.AddDays(1);
                //}

                //// Check for booking conflicts
                //var conflictingBooking = db.TBL_T_BARGING_ONLINEs.FirstOrDefault
                //    (b => ((b.DATE_BOOKING == param.DateBooking && b.FINISH_BOOKING == null) || (b.DATE_BOOKING == param.DateBooking.AddDays(1) && b.FINISH_BOOKING != null)) && ((b.START_TIME >= param.StartTime && b.START_TIME < finishTime) || (b.FINISH_TIME > param.StartTime && b.FINISH_TIME <= finishTime) || (b.START_TIME <= param.StartTime && b.FINISH_TIME >= finishTime)));

                //if (conflictingBooking != null)
                //{
                //    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Terjadi bentrokan waktu booking!" });
                //}
                #endregion

                if (finishTime > 24)
                {
                    finishTime -= 24;
                    finishBooking = param.DateBooking.AddDays(1);

                    // Check for booking conflicts
                    var conflictingBooking = db.TBL_T_BARGING_ONLINEs.FirstOrDefault(b =>
                        (b.DATE_BOOKING == param.DateBooking || b.DATE_BOOKING == param.DateBooking.AddDays(1)) &&
                        ((b.START_TIME >= param.StartTime && b.DATE_BOOKING == param.DateBooking) ||
                        (b.START_TIME < finishTime && b.DATE_BOOKING == param.DateBooking.AddDays(1)) ||
                        (b.FINISH_TIME > param.StartTime && b.DATE_BOOKING == param.DateBooking) ||
                        (b.FINISH_TIME <= finishTime && b.DATE_BOOKING == param.DateBooking.AddDays(1))) &&
                        (b.JETTY == param.Jetty || (b.JETTY != param.Jetty && b.DATE_BOOKING != param.DateBooking)));

                    if (conflictingBooking != null)
                    {
                        return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Terjadi bentrokan waktu booking!" });
                    }
                }
                else
                {
                    // Check for booking conflicts
                    var conflictingBooking = db.TBL_T_BARGING_ONLINEs.FirstOrDefault(b =>
                        b.DATE_BOOKING == param.DateBooking &&
                        ((b.START_TIME >= param.StartTime && b.START_TIME < finishTime) ||
                        (b.FINISH_TIME > param.StartTime && b.FINISH_TIME <= finishTime)) &&
                        (b.JETTY == param.Jetty || (b.JETTY != param.Jetty && b.DATE_BOOKING != param.DateBooking)));

                    if (conflictingBooking != null)
                    {
                        return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Terjadi bentrokan waktu booking!" });
                    }
                }


                // Create a new TBL_T_BARGING_ONLINE object and set its properties
                var create = new TBL_T_BARGING_ONLINE
                {
                    JETTY = param.Jetty,
                    TUG_BOAT = param.TugBoat,
                    BARGE = param.Barge,
                    CAPACITY = param.Capacity,
                    VESSEL = param.Vessel,
                    PROCESS_TIME = param.ProcessTime,
                    DATE_BOOKING = param.DateBooking,
                    START_TIME = param.StartTime,
                    FINISH_TIME = finishTime,
                    FINISH_BOOKING = finishBooking,
                    ID_USER = booking.ID, // Assign the ID_Booking value from the other table
                    STATUS = "Progress" // Set the STATUS property to "Progress"
                };

                db.TBL_T_BARGING_ONLINEs.InsertOnSubmit(create);
                db.SubmitChanges();

                return Ok(new { Data = create, Status = true, Message = "Data Berhasil dibuat!!!" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Terdapat data yang kosong!!!" });
            }
        }

        //mendapatkan bookingdate per id
        [Route("Get_BargingOnlineByStartTime/{id}")]
        [HttpGet]
        public IHttpActionResult GetBargingOnlineById(int id)
        {
            try
            {
                var startDate = DateTime.Now.Date;
                var endDate = startDate.AddDays(14);

                string jetty = Request.GetQueryNameValuePairs()
                                    .FirstOrDefault(q => string.Compare(q.Key, "jetty", true) == 0)
                                    .Value;

                var records = db.TBL_T_BARGING_ONLINEs
                    .Where(b => b.ID_USER == id && b.DATE_BOOKING.HasValue && b.DATE_BOOKING.Value.Date >= startDate && b.DATE_BOOKING.Value.Date <= endDate && b.JETTY == jetty)
                    .ToList();

                if (records.Count == 0)
                {
                    return Content(HttpStatusCode.OK, new { Data = new object[0], Status = true, Message = "Data berhasil diambil" });
                }

                var response = records.GroupBy(record => new { record.ID_USER, record.DATE_BOOKING.Value.Date })
                    .Select(group =>
                    {
                        var record = group.First();
                        int maksimalWaktuDalamSehari = 24;
                        int totalWaktuYangDidapatkan = (record.START_TIME ?? 0) + (record.PROCESS_TIME ?? 0);
                        int finishTime;
                        DateTime finishDate;

                        if (totalWaktuYangDidapatkan > maksimalWaktuDalamSehari)
                        {
                            finishTime = totalWaktuYangDidapatkan - maksimalWaktuDalamSehari;
                            finishDate = record.DATE_BOOKING.Value.AddDays(1);

                            var nextDayResponse = new
                            {
                                bookingDate = finishDate.Date,
                                bookingUserId = record.ID_USER,
                                jetty
                            };

                            return new[]
                            {
                        new
                        {
                            bookingDate = record.DATE_BOOKING.Value.Date,
                            bookingUserId = record.ID_USER,
                            jetty
                        },
                        nextDayResponse
                            };
                        }
                        else
                        {
                            finishTime = totalWaktuYangDidapatkan;
                            finishDate = record.DATE_BOOKING.Value;

                            return new[]
                            {
                        new
                        {
                            bookingDate = record.DATE_BOOKING.Value.Date,
                            bookingUserId = record.ID_USER,
                            jetty
                        }
                            };
                        }
                    })
                    .SelectMany(x => x)
                    .ToArray();

                return Ok(new { Data = response, Status = true, Message = "Data berhasil diambil" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = e.Message });
            }
        }



        [Route("Get_ValidasiBookingTime")]
        [HttpGet]
        public IHttpActionResult GetBookingTime(DateTime filterDate, string jetty)
        {
            try
            {
                DateTime filterNextDate = filterDate.AddDays(1);
                List<object> finalData = new List<object>();
                List<object> nextFinalData = new List<object>();

                var database = db.TBL_T_BARGING_ONLINEs.ToList();

                List<object> filterData = new List<object>();
                List<object> filterNextData = new List<object>();

                for (int i = 0; i < database.Count(); i++)
                {
                    if (database[i].DATE_BOOKING != null && database[i].FINISH_BOOKING != null)
                    {
                        if (database[i].DATE_BOOKING.Value.Date == filterDate.Date && database[i].JETTY == jetty)
                        {
                            filterData.Add(new { Data = database[i], typeDate = 1 });
                        }
                        else if (database[i].FINISH_BOOKING.Value.Date == filterDate.Date && database[i].JETTY == jetty)
                        {
                            filterData.Add(new { Data = database[i], typeDate = 2 });
                        }
                    }
                    else if (database[i].DATE_BOOKING != null && database[i].FINISH_BOOKING == null)
                    {
                        if (database[i].DATE_BOOKING.Value.Date == filterDate.Date && database[i].JETTY == jetty)
                        {
                            filterData.Add(new { Data = database[i], typeDate = 3 });
                        }
                    }
                    if (database[i].DATE_BOOKING != null && database[i].FINISH_BOOKING != null)
                    {
                        if (database[i].DATE_BOOKING.Value.Date == filterNextDate.Date && database[i].JETTY == jetty)
                        {
                            filterNextData.Add(new { Data = database[i], typeDate = 1 });
                        }
                        else if (database[i].FINISH_BOOKING.Value.Date == filterNextDate.Date && database[i].JETTY == jetty)
                        {
                            filterNextData.Add(new { Data = database[i], typeDate = 2 });
                        }
                    }
                    else if (database[i].DATE_BOOKING != null && database[i].FINISH_BOOKING == null)
                    {
                        if (database[i].DATE_BOOKING.Value.Date == filterNextDate.Date && database[i].JETTY == jetty)
                        {
                            filterNextData.Add(new { Data = database[i], typeDate = 3 });
                        }
                    }
                }

                for (int i = 1; i <= 24; i++)
                {
                    bool isBooked = false;
                    int? bookingUserId = null;

                    foreach (var data in filterData)
                    {
                        if ((int)data.GetType().GetProperty("typeDate").GetValue(data) == 1)
                        {
                            dynamic objData = data.GetType().GetProperty("Data").GetValue(data);
                            if (i >= objData.START_TIME && objData.START_TIME <= 24)
                            {
                                isBooked = true;
                                bookingUserId = objData.ID_USER;
                                break;
                            }
                        }

                        else if ((int)data.GetType().GetProperty("typeDate").GetValue(data) == 2)
                        {
                            dynamic objData = data.GetType().GetProperty("Data").GetValue(data);
                            if (i <= objData.FINISH_TIME)
                            {
                                isBooked = true;
                                bookingUserId = objData.ID_USER;
                                break;
                            }
                        }
                        else if ((int)data.GetType().GetProperty("typeDate").GetValue(data) == 3)
                        {
                            dynamic objData = data.GetType().GetProperty("Data").GetValue(data);
                            if (i >= objData.START_TIME && i <= objData.FINISH_TIME)
                            {
                                isBooked = true;
                                bookingUserId = objData.ID_USER;
                                break;
                            }
                        }
                    }

                    finalData.Add(new
                    {
                        time = i,
                        isBooked = isBooked,
                        BookingUserId = bookingUserId
                    });
                }

                for (int i = 1; i <= 24; i++)
                {
                    bool isBooked = false;
                    int? bookingUserId = null;

                    foreach (var data in filterNextData)
                    {
                        if ((int)data.GetType().GetProperty("typeDate").GetValue(data) == 1)
                        {
                            dynamic objData = data.GetType().GetProperty("Data").GetValue(data);
                            if (i >= objData.START_TIME && objData.START_TIME <= 24)
                            {
                                isBooked = true;
                                bookingUserId = objData.ID_USER;
                                break;
                            }
                        }

                        else if ((int)data.GetType().GetProperty("typeDate").GetValue(data) == 2)
                        {
                            dynamic objData = data.GetType().GetProperty("Data").GetValue(data);
                            if (i <= objData.FINISH_TIME)
                            {
                                isBooked = true;
                                bookingUserId = objData.ID_USER;
                                break;
                            }
                        }
                        else if ((int)data.GetType().GetProperty("typeDate").GetValue(data) == 3)
                        {
                            dynamic objData = data.GetType().GetProperty("Data").GetValue(data);
                            if (i >= objData.START_TIME && i <= objData.FINISH_TIME)
                            {
                                isBooked = true;
                                bookingUserId = objData.ID_USER;
                                break;
                            }
                        }
                    }

                    nextFinalData.Add(new
                    {
                        time = i,
                        isBooked = isBooked,
                        BookingUserId = bookingUserId
                    });
                }

                var newData = new
                {
                    Today = finalData,
                    NextDay = nextFinalData
                };

                return Ok(new { Data = newData, Status = true, Message = "Berhasil Menampilkan Data Booking!!!" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Error!!!" });
            }
        }
    }
}
