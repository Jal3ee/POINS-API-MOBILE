using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web;
using Point_Internal_API.Models;
using Point_Internal_API.ViewModel;

namespace Point_Internal_API.Controllers
{
    [RoutePrefix("api/version")]
    public class VersionController : ApiController
    {
        DataClassesDataContext db = new DataClassesDataContext();

        //get version
        [Route("Get_Version")]
        [HttpGet]
        public IHttpActionResult GetVersion()
        {
            try
            {
                var data = db.TBL_M_APP_VERSIONs.ToList();

                if (data == null)
                {
                    return Ok(new { Status = false, Message = "Version Tidak ada!!!" });
                }

                return Ok(new { Data = data, Status = true, Message = "Data Version Berhasil Diambil!!!", Total = data.Count() });
            }
            catch(Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = e.Message });
            }
        }
    }
}
