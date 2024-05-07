using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Point_Internal_API.Models;
using Point_Internal_API.ViewModel;

namespace Point_Internal_API.Controllers
{
    [RoutePrefix("api/notifikasi")]
    public class NotifikasiController : ApiController
    {
        DataClassesDataContext db = new DataClassesDataContext();

        [Route("Change_Notifikasi_Status")]
        [HttpPost]
        public IHttpActionResult ChangeNotifikasiStatus(Guid id_notifikasi)
        {
            try
            {
                // Cari notifikasi berdasarkan id_user dan id_notifikasi
                var notifikasi = db.VW_NOTIF_APPROVAL_BARGINGs.FirstOrDefault(x => x.id == id_notifikasi);

                // Jika notifikasi ditemukan, update nilai isHasBeenRead menjadi true
                if (notifikasi != null)
                {
                    db.cusp_update_status_notif(notifikasi.id); // Simpan perubahan ke database
                    return Ok(new { Status = true, Message = "Status notifikasi berhasil diubah!" });
                }
                else
                {
                    return Content(HttpStatusCode.NotFound, new { Message = "Notifikasi tidak ditemukan!" });
                }
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.InternalServerError, new { Message = e.Message });
            }
        }


        [Route("Get_Notifikasi/{id_user}")]
        [HttpGet]
        public IHttpActionResult GetNotifikasi(int id_user)
        {
            try
            {
                var data = db.VW_NOTIF_APPROVAL_BARGINGs
                    .Where(x => x.ID_USER == id_user)
                    .Select(x => new {
                        x.title,
                        x.body,
                        x.date,
                        x.isHasBeenRead,
                        x.ID_USER,
                        x.id
                    })
                    .ToList();

                // Return the data with HTTP 200 (OK) status, even if it's an empty array.
                return Ok(new { Data = data, Status = true, Message = "Data Berhasil Diambil!" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = e.Message });
            }
        }


        [Route("Get_NotifikasiThnBlnTgl/{id_user}")]
        [HttpGet]
        public IHttpActionResult GetNotifikasiThnBlnTgl(int id_user)
        {
            try
            {
                var data = db.VW_NOTIF_APPROVAL_BARGINGs
                    .Where(x => x.ID_USER == id_user && x.isHasBeenRead != true) // Menambahkan kondisi WHERE untuk ID_USER dan isHasBeenRead
                    .Select(x => new {
                        x.title,
                        x.body,
                        x.date,
                        x.isHasBeenRead,
                        x.ID_USER
                    })
                    .ToList();

                if (data == null || data.Count == 0)
                {
                    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Notif Tidak ada!!!" });
                }

                // Membuat dictionary untuk mengelompokkan notifikasi berdasarkan tanggal, bulan, dan tahun
                var notifikasiGroupedByDate = new Dictionary<string, List<object>>();

                foreach (var notifikasi in data)
                {
                    // Mengambil informasi tanggal, bulan, dan tahun dari properti 'date'
                    var tanggal = notifikasi.date?.Date; // Handle nullable DateTime
                    if (tanggal == null)
                    {
                        continue; // Skip jika tanggal null
                    }

                    var key = tanggal.Value.ToString("dd/MM/yyyy");

                    if (!notifikasiGroupedByDate.ContainsKey(key))
                    {
                        // Jika tanggal belum ada dalam dictionary, buat list baru untuk tanggal tersebut
                        notifikasiGroupedByDate[key] = new List<object>();
                    }

                    // Tambahkan notifikasi ke list tanggal yang sesuai
                    notifikasiGroupedByDate[key].Add(new
                    {
                        notifikasi.title,
                        notifikasi.body,
                        notifikasi.date,
                        notifikasi.isHasBeenRead,
                        notifikasi.ID_USER
                    });
                }

                return Ok(new { Data = notifikasiGroupedByDate, Status = true, Message = "Data Berhasil Diambil!" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = e.Message });
            }
        }


    }
}