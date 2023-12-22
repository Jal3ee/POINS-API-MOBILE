using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Web.Http;
using Point_Internal_API.Models;
using Point_Internal_API.ViewModel;

namespace Point_Internal_API.Controllers
{
    [RoutePrefix("api/senselog")]
    public class SenselogController : ApiController
    {
        DataClassesDataContext db = new DataClassesDataContext();

        [Route("Get_Last_Senselog")]
        [HttpGet]
        public IHttpActionResult GetLastSenselog()
        {
            try
            {
                var senselog = db.cusp_get_last_senselog_realtime().ToList();

                if (senselog == null || !senselog.Any())
                {
                    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Data Last Senselog Tidak Ditemukan !!!" });
                }

                return Ok(new { Data = senselog, Status = true, Message = "Data Last Senselog Ditemukan!!!" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Terjadi kesalahan" });
            }
        }

        [Route("Get_Avg_Senselog")]
        [HttpGet]
        public IHttpActionResult GetAvgSenselog(DateTime startDate, DateTime endDate)
        {
            try
            {
                var senselogData = db.cusp_avg_range_senselog(startDate, endDate)
                    .Select(result => new
                    {
                        Jam = result.Jam,
                        Tanggal = result.Tanggal,
                        Rata_Rata_Surface = result.Rata_Rata_Surface
                    })
                    .ToList();

                if (senselogData == null || !senselogData.Any())
                {
                    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Data Average Senselog Tidak Ditemukan !!!" });
                }

                // Group the data by different dates
                var groupedData = senselogData
                    .GroupBy(item => item.Tanggal)
                    .Select(group => new
                    {
                        Tanggal = group.Key,
                        Rata_Rata_Surface = Math.Round(Convert.ToDouble(group.Average(item => item.Rata_Rata_Surface)), 2), // Convert and calculate the rounded average
                        Data = group.ToList()
            })
                    .ToList();

                return Ok(new { Data = groupedData, Status = true, Message = "Data Last Senselog Ditemukan!!!" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Terjadi kesalahan" });
            }
        }
    }
}
