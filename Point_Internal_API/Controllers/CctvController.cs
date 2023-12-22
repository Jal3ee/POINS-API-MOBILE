using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.Web.Http;
using Point_Internal_API.Models;
using Point_Internal_API.ViewModel;

namespace Point_Internal_API.Controllers
{
    [RoutePrefix("api/cctv")]
    public class CctvController : ApiController
    {
        DataClassesDataContext db = new DataClassesDataContext();

        [Route("Get_Cctv")]
        [HttpGet]
        public IHttpActionResult GetCctv()
        {
            try
            {
                // Ambil data dari tabel "TBL_R_CCTV" menggunakan LINQ to SQL
                var cctvData = db.TBL_R_CCTVs.ToList(); // Ubah "TBL_R_CCTVs" dengan nama tabel yang sesuai di database Anda

                // Periksa apakah data CCTV ditemukan
                if (cctvData == null || !cctvData.Any())
                {
                    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Data CCTV tidak ditemukan." });
                }

                // Jika ditemukan, kembalikan data CCTV sebagai respons
                return Ok(new { Data = cctvData ,Status = true, Message = "Data CCTV ditemukan !!!" });
            }
            catch (Exception e)
            {
                // Tangani kesalahan dengan mengembalikan pesan kesalahan
                return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Terjadi kesalahan saat mengambil data CCTV: " + e.Message });
            }
        }
    }
}