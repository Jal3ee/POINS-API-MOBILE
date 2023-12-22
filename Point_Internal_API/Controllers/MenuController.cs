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
    [RoutePrefix("api/menu")]
    public class MenuController : ApiController
    {
        DataClassesDataContext db = new DataClassesDataContext();

        //membuat menu
        [Route("Create_Menu")]
        [HttpPost]
        public IHttpActionResult CreateMenu(ClsMenu menu)
        {
            try
            {
                var requestUrl = HttpContext.Current.Request.Url;
                var uriBuilder = new UriBuilder(requestUrl)
                {
                    Scheme = Uri.UriSchemeHttps
                };
                var url = uriBuilder.Uri;

                string base64 = menu.ICON.Substring(menu.ICON.IndexOf(',') + 1);
                base64 = base64.Trim('\0');
                string strDateTime = DateTime.Now.ToString("ddMMyyyHHMMss");
                string fileName = menu.NAMA.Replace(" ", "_") + ".Jpeg";
                byte[] imageBytes = Convert.FromBase64String(base64);
                MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                ms.Write(imageBytes, 0, imageBytes.Length);
                System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                string physicalPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/Menu/" + fileName);

                var path = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/Menu/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                image.Save(physicalPath, System.Drawing.Imaging.ImageFormat.Png);

                //// add to database
                Guid i_guid_pid = System.Guid.NewGuid();

                TBL_M_MENU_APP table_data = new TBL_M_MENU_APP();
                table_data.ICON = $"{url.Scheme}://{url.Authority}/Images/Menu/{fileName}";
                table_data.NAMA = menu.NAMA;
                table_data.STATUS = menu.STATUS; // Mengambil nilai status dari properti JSON

                db.TBL_M_MENU_APPs.InsertOnSubmit(table_data);
                db.SubmitChanges();

                return Ok(new { Status = true, Message = "Data berhasil dimasukan!" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Status = false, Message = e.Message });
            }
        }

        //update menu
        [Route("Update_Menu/{id}")]
        [HttpPost]
        public IHttpActionResult UpdateMenu(ClsMenu menu, int id)
        {
            try
            {
                var url = new UriBuilder(HttpContext.Current.Request.Url)
                {
                    Scheme = Uri.UriSchemeHttps,
                }.Uri;

                TBL_M_MENU_APP table_data = db.TBL_M_MENU_APPs.FirstOrDefault(m => m.ID == id);

                if (table_data == null)
                {
                    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Data tidak ditemukan!!" });
                }

                if (!string.IsNullOrEmpty(menu.ICON))
                {
                    string base64 = menu.ICON.Substring(menu.ICON.IndexOf(',') + 1);
                    base64 = base64.Trim('\0');
                    string strDateTime = DateTime.Now.ToString("ddMMyyyHHMMss");
                    string fileName = "Update_Menu"+strDateTime + ".Jpeg";
                    byte[] imageBytes = Convert.FromBase64String(base64);
                    MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                    ms.Write(imageBytes, 0, imageBytes.Length);
                    System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                    string physicalPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/Menu/" + fileName);

                    var path = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/Menu/");

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    image.Save(physicalPath, System.Drawing.Imaging.ImageFormat.Png);

                    table_data.ICON = $"{url.Scheme}://{url.Authority}/Images/Menu/{fileName}";
                }

                table_data.NAMA = menu.NAMA;

                db.SubmitChanges();

                return Ok(new { Status = true, Message = "Data berhasil diupdate!" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Status = false, Message = e.Message });
            }
        }


        //menampilkan menu
        //[Route("Get_Menu")]
        //[HttpGet]
        //public IHttpActionResult GetMenu()
        //{
        //    try
        //    {
        //        var data = db.TBL_M_MENU_APPs.ToList();

        //        if (data == null)
        //        {
        //            return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Menu Tidak ada!!!" });
        //        }

                
        //        return Ok(new { Data = data, Status = true, Message = "Data Berhasil Diambil!", Total = data.Count() });
        //    }
        //    catch(Exception e)
        //    {
        //        return Content(HttpStatusCode.BadRequest, new { Message = e.Message });
        //    }
        //}

        [Route("Get_Menu/{id_user}")]
        [HttpGet]
        public IHttpActionResult GetMenu(int id_user)
        {
            try
            {
                var user = db.TBL_M_USERs.FirstOrDefault(u => u.ID == id_user);
                if (user == null)
                {
                    return Content(HttpStatusCode.NotFound, new { Status = false, Message = "User not found!" });
                }

                List<TBL_M_MENU_APP> data;

                // Jika user dengan ID = 4, maka hanya tampilkan menu dengan STATUS = 4
                if (user.ID == 4)
                {
                    data = db.TBL_M_MENU_APPs.Where(menu => menu.STATUS == "1").ToList();
                }
                else
                {
                    // Jika user bukan dengan ID = 4, tampilkan semua menu
                    data = db.TBL_M_MENU_APPs.ToList();
                }

                if (data == null || data.Count == 0)
                {
                    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Menu Tidak ada!!!" });
                }

                return Ok(new { Data = data, Status = true, Message = "Data Berhasil Diambil!", Total = data.Count() });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = e.Message });
            }
        }

    }
}
