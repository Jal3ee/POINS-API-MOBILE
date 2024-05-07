using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Configuration;
using System.Web;
//using System.Web.Mvc;
using System.Web.Http;
using Point_Internal_API.Models;
using Point_Internal_API.ViewModel;
using MySql.Data.MySqlClient;
using System.IO;
using System.Drawing;
using Image = System.Drawing.Image;
using System.Threading.Tasks;

namespace Point_Internal_API.Controllers
{

    //login
    [RoutePrefix("api/login")]
    public class UserController : ApiController
    {
        ClsLogin newLogin = new ClsLogin();
        ClsWa newWa = new ClsWa();
        ClsSMSGateway sms = new ClsSMSGateway();
        DataClassesDataContext db = new DataClassesDataContext();
        DataClassesWADataContext wa = new DataClassesWADataContext();
        HttpContext context = HttpContext.Current;
        private readonly List<ClsLogin> _users = new List<ClsLogin>();
        private MySqlConnection con;


        //Buat akun
        [Route("Create_Register")]
        [HttpPost]
        public IHttpActionResult CreateUser(ClsLogin user)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingAccount = db.TBL_M_USERs.FirstOrDefault(a => a.EMAIL == user.EMAIL);
                if (existingAccount != null)
                {
                    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Email pada akun sudah dipakai!!!" });
                }

                var newAccount = new TBL_M_USER
                {
                    EMAIL = user.EMAIL,
                    TELEPON = user.TELEPON,
                    NAMA = user.NAMA,
                    PASSWORD = user.PASSWORD,
                    ID_COMPANY = user.ID_COMPANY,
                    VERIFICATION_STATUS = null // Set Null for account goes to waiti
                };

                db.TBL_M_USERs.InsertOnSubmit(newAccount);
                db.SubmitChanges();

                return Ok(new { Data = newAccount, Status = true, Message = "Akun berhasil dibuat" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = e.Message });
            }
        }


        //Hapus akun
        [Route("Delete_User/{id}")]
        [HttpPost]
        public IHttpActionResult DeleteUser(int id)
        {
            try
            {
                var user = db.TBL_M_USERs.FirstOrDefault(a => a.ID == id);

                if (user == null)
                {
                    return Content(HttpStatusCode.NotFound, new { Status = false, Message = "Akun tidak ditemukan" });
                }

                db.TBL_M_USERs.DeleteOnSubmit(user);
                db.SubmitChanges();

                return Ok(new { Status = true, Message = "Akun Berhasil dihapus" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = e.Message });
            }
        }

        //Create Version
        [Route("CreateVersion")]
        [HttpPost]
        public IHttpActionResult CreateVersion(ClsAppVersion ver)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var newVersion = new TBL_M_APP_VERSION
                {
                    os_name = ver.OS_NAME,
                    app_version = ver.APP_VERSION
                };

                db.TBL_M_APP_VERSIONs.InsertOnSubmit(newVersion);
                db.SubmitChanges();

                return Ok(new { Data = newVersion, Status = true, Message = "Version Baru" });
            }
            catch(Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = e.Message });
            }
        }

        //Merubah verifikasi user
        [Route("update_verification_user/{id}")]
        [HttpPost]
        public IHttpActionResult UpdateVarificationUser(int id)
        {
            try
            {
                var user = db.TBL_M_USERs.FirstOrDefault(m => m.ID == id);
                if(user == null)
                {
                    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "User tidak ditemukan!!!" });
                }

                db.cusp_update_user_verification_poins(id);
                db.SubmitChanges();
                return Ok(new {Status = true, Message = "Berhasil Update User Verification!" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = e.Message });
            }
        }

        // Update Profile
        [Route("Update_Profile/{id}")]
        [HttpPost]
        public IHttpActionResult UpdateProfile(int id, ClsLogin user)
        {
            try
            {
                var existingUser = db.TBL_M_USERs.FirstOrDefault(m => m.ID == id);
                if (existingUser == null)
                {
                    return Ok(new { Status = false, Message = "Error!!!" });
                }
                existingUser.EMAIL = user.EMAIL;
                existingUser.NAMA = user.NAMA;
                existingUser.ID_COMPANY = user.ID_COMPANY;
                //existingUser.TELEPON = user.TELEPON;
                //existingUser.PASSWORD = user.PASSWORD;
                //existingUser.FOTO = user.FOTO;
                db.SubmitChanges();
                return Ok(new { Data = existingUser, Status = true, Message = "Berhasil Update Profile!" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = e.Message });
            }
        }

        // Upload Image
        [Route("Upload_Image/{id}")]
        [HttpPost]
        public IHttpActionResult Upload_Image(int id, ClsLogin poto)
        {
            try
            {
                var url = HttpContext.Current.Request.Url;
                var user = db.TBL_M_USERs.FirstOrDefault(u => u.ID == id);
                if (user == null)
                {
                    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "User tidak ditemukan!!!" });
                }
                string base64 = poto.FOTO.Substring(poto.FOTO.IndexOf(',') + 1);
                base64 = base64.Trim('\0');
                string strDateTime = DateTime.Now.ToString("ddMMyyyHHMMss");
                string fileName = user.NAMA.Replace(" ", "_") + ".Jpeg";
                byte[] imageBytes = Convert.FromBase64String(base64);
                MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
                ms.Write(imageBytes, 0, imageBytes.Length);
                System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
                string physicalPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/Profile/" + fileName);

                var path = System.Web.Hosting.HostingEnvironment.MapPath("~/Images/Profile/");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                image.Save(physicalPath, System.Drawing.Imaging.ImageFormat.Png);

                //// update user image
                user.FOTO = $"https://{url.Authority}/Images/Profile/{fileName}";
                db.SubmitChanges();

                return Ok(new { Status = true, Message = "Data berhasil diupdate!" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Status = false, Message = e.Message });
            }
        }

        // Menampilkan akun
        [Route("Get_Register")]
        [HttpGet]
        public IHttpActionResult GetUser()
        {
            try
            {
                var data = db.TBL_M_USERs.ToList();

                return Ok(new { Data = data, Status = true, Message = "Data Berhasil Diambil!", Total = data.Count() });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = e.Message });
            }
        }

        [Route("Get_Profile")]
        [HttpGet]
        public IHttpActionResult GetProfile()
        {
            try
            {
                var profile = db.VW_USERs
                .Select(c => new ClsLogin
                {
                    ID = c.ID,
                    NAMA = c.NAMA,
                    EMAIL = c.EMAIL,
                    TELEPON = c.TELEPON,
                    FOTO = c.FOTO,
                    CUSTOMER = c.CUSTOMER
                })
                .ToList();

                return Ok(new { Data = profile, Status = true, Message = "Data Profile!!" });
            }
            catch(Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = e.Message });
            }
        }

        //Menampilkan akun per id
        [Route("Get_Profile/{id}")]
        [HttpGet]
        public IHttpActionResult GetProfile(int id)
        {
            try
            {
                var profile = db.VW_USERs
                    .Where(c => c.ID == id)
                    .Select(c => new ClsLogin
                    {
                        ID = c.ID,
                        NAMA = c.NAMA,
                        EMAIL = c.EMAIL,
                        TELEPON = c.TELEPON,
                        FOTO = c.FOTO,
                        ID_COMPANY = c.ID_COMPANY,
                        CUSTOMER = c.CUSTOMER
                    })
                    .FirstOrDefault();

                if (profile == null)
                {
                    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Id Profile Tidak ditemukan!!!" });
                }

                return Ok(new { Data = profile, Status = true, Message = "Data Profile!" });
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = e.Message });
            }
        }

        //Login OTP WA
        [Route("Login")]
        [HttpPost]
        public IHttpActionResult LoginUser(ClsLogin param)
        {
            try
            {
                if (!ModelState.IsValid)
                    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Incorrect Email or Password!!!!" });


                using (var db = new DataClassesDataContext())
                {
                    var user = (from u in db.TBL_M_USERs
                                where u.EMAIL.ToLower() == param.EMAIL.ToLower() && u.PASSWORD == param.PASSWORD
                                select u).FirstOrDefault();

                    if (user == null)
                        return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Incorrect Email or Password!!!!" });

                    if (user.VERIFICATION_STATUS == null)
                        return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Akun ada belum terverifikasi" });

                    if (user.VERIFICATION_STATUS == "false")
                        return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Akun ada tidak terverifikasi, silahkan hubungi admin" });
                    //var random = new Random();
                    //var otp = random.Next(100000, 999999).ToString();

                    string numbers = "1234567890";
                    

                    string characters = numbers;

                    int length = 6;
                    string otp = string.Empty;
                    for (int i = 0; i < length; i++)
                    {
                        string character = string.Empty;
                        do
                        {
                            int index = new Random().Next(0, characters.Length);
                            character = characters.ToCharArray()[index].ToString();
                        } while (otp.IndexOf(character) != -1);

                        otp += character;
                    }
                    user.OTP = otp; // Set the generated OTP for the user
                    db.SubmitChanges(); // Save the changes to the database



                    //sms.OpenConnection(ref con);

                    //var query = newLogin.getPhoneByEmail(param.EMAIL);
                    //sms.Insert(query, "PT. Kalimantan Prima Persada: " + Environment.NewLine + "Kode otorisasi anda untuk login adalah:" + Environment.NewLine +
                    //                        otp + Environment.NewLine + " ", "PT. Kalimantan Prima Persada", "", con);
                    //sms.CloseConnection(ref con);

                    var query = newLogin.getPhoneByEmail(param.EMAIL);
                    newWa.sendMessage(query, "PT. Kalimantan Prima Persada: " + Environment.NewLine + "Kode otoritas anda untuk login POINS adalah: " + Environment.NewLine +
                                            otp + Environment.NewLine + " ", "POINS");

                    var result = newLogin.NotifOtp(param.EMAIL, "PT. Kalimantan Prima Persada: " + Environment.NewLine + "Kode otoritas anda untuk login POINS adalah: " + Environment.NewLine +
                                            otp + Environment.NewLine + " ");

                    return Ok(new { Status = true, Message = "Success Login here is your otp = "+otp });
                }
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Incorrect Email or Password!!!!" });
            }
        }

        //validasi otp saat login
        [Route("ValidateOtp")]
        [HttpPost]
        public IHttpActionResult ValidateOtp(ClsOtp param)
        {
            try
            {
                using (var db = new DataClassesDataContext())
                {
                    var user = db.TBL_M_USERs.FirstOrDefault(u => u.EMAIL == param.Email && u.OTP == param.Otp);
                    string status_login = "true";
                    user.STATUS_LOGIN = status_login; // Set the generated OTP for the user
                    db.SubmitChanges(); // Save the changes to the database

                    if (user == null)
                    return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Incorrect OTP!!!" });

                    //// Update the user's OTP to null after successful validation
                    //user.OTP = null;
                    //db.SubmitChanges(); // Save the changes to the database

                    return Ok(new { Data = new { ID = user.ID, STATUS_LOGIN = user.STATUS_LOGIN}, Status = true, Message = "OTP validation successful" });
                }
            }
            catch (Exception e)
            {
                return BadRequest("Incorrect OTP!!!");
            }
        }

        //langkah 1 forget pass
        [Route("ForgetPassword")]
        [HttpPost]
        public IHttpActionResult ForgetPassword(validasiEmail email)
        {
            try
            {
                using (var db = new DataClassesDataContext())
                { 
                    var user = db.TBL_M_USERs.FirstOrDefault(u => u.EMAIL == email.EMAIL);

                    string numbers = "1234567890";


                    string characters = numbers;

                    int length = 6;
                    string otp = string.Empty;
                    for (int i = 0; i < length; i++)
                    {
                        string character = string.Empty;
                        do
                        {
                            int index = new Random().Next(0, characters.Length);
                            character = characters.ToCharArray()[index].ToString();
                        } while (otp.IndexOf(character) != -1);

                        otp += character;
                    }
                    user.OTP = otp; // Set the generated OTP for the user
                    db.SubmitChanges(); // Save the changes to the database

                    var query = newLogin.getPhoneByEmail(email.EMAIL);
                    newWa.sendMessage(query, "PT. Kalimantan Prima Persada: " + Environment.NewLine + "Kode otoritas anda untuk merubah password baru Aplikasi POINS adalah: " + Environment.NewLine +
                                            otp + Environment.NewLine + " ", "M-OK");

                    if (user == null)
                        return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Incorrect EMAIL!!!" });

                    return Ok(new { Data = new { TELEPON = user.TELEPON, OTP = user.OTP}, Status = true, Message = "EMAIL validation successful" });
                }
            }
            catch(Exception e)
            {
                return BadRequest("Incorrect EMAIL!!!");
            }
        }

        //langkah 2 validasi otp forget pass
        [Route("ValidateOtp_ForgetPassword")]
        [HttpPost]
        public IHttpActionResult ValidateOtp_ForgetPassword(ClsOtp param)
        {
            try
            {
                using (var db = new DataClassesDataContext())
                {
                    var user = db.TBL_M_USERs.FirstOrDefault(u => u.EMAIL == param.Email && u.OTP == param.Otp);

                    if (user == null)
                        return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Incorrect OTP!!!" });

                    //// Update the user's OTP to null after successful validation
                    user.OTP = null;
                    db.SubmitChanges(); // Save the changes to the database

                    return Ok(new {Status = true, Message = "OTP validation successful" });
                }
            }
            catch (Exception e)
            {
                return BadRequest("Incorrect OTP!!!");
            }
        }

        //langkah 3 merubah pass saat forget pass
        [Route("ChangePassword")]
        [HttpPost]
        public IHttpActionResult ChangePassword(changePassword param)
        {
            try
            {
                using (var db = new DataClassesDataContext())
                {
                    var user = db.TBL_M_USERs.FirstOrDefault(u => u.EMAIL == param.EMAIL);
                    if (user == null)
                        return Content(HttpStatusCode.BadRequest, new { Status = false, Message = "Incorrect User Id or Old Password!!!" });

                    // Update the user's password with the new password
                    user.PASSWORD = param.PASSWORD;
                    db.SubmitChanges(); // Save the changes to the database

                    return Ok(new { Status = true, Message = "Password changed successfully" });
                }
            }
            catch (Exception e)
            {
                return BadRequest("Failed to change password");
            }
        }


        //logout
        [Route("Logout")]
        [HttpPost]
        public IHttpActionResult LogoutUser(ClsLogin param)
        {
            try
            {
                using (var db = new DataClassesDataContext())
                {
                    var user = db.TBL_M_USERs.FirstOrDefault(u => u.EMAIL == param.EMAIL && u.PASSWORD == param.PASSWORD);

                    if (user == null)
                        return BadRequest("Invalid Login Details");

                    user.OTP = null; // Set the OTP to null to logout the user
                    user.STATUS_LOGIN = null;
                    db.SubmitChanges(); // Save the changes to the database

                    return Ok(new { Status = true, Message = "Success Logout" });
                }
            }
            catch (Exception e)
            {
                return Content(HttpStatusCode.BadRequest, new { Message = e.Message });
            }
        }

    }
}