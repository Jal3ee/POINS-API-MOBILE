using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Point_Internal_API.Models;
using System.Configuration;
using System.IO;
using System.Web.UI.WebControls;
using System.Drawing.Imaging;
using Image = System.Drawing.Image;
using System.Drawing.Drawing2D;
using System.Drawing;


namespace Point_Internal_API.ViewModel
{
    public class ClsLogin
    {
        DataClassesDataContext db = new DataClassesDataContext();

        public int ID { get; set; }
        public string EMAIL { get; set; }
        public string PASSWORD { get; set; }
        public string TELEPON { get; set; }
        public string NAMA { get; set; }
        public string FOTO { get; set; }
        public string OTP { get; set; }
        public int? ID_COMPANY { get; set; }
        public string CUSTOMER { get; set; }

        public bool isVersionValid(string version, string osName)
        {
            //periksa pengaturan mengenai pengecekan full version
            bool isFullVer = false;
            string isFullVersion = ConfigurationManager.AppSettings["fullVersion"].ToString();
            if (isFullVersion == "Y")
            {
                isFullVer = true;
            }

            if (db.cufn_isVersionValid(version, isFullVer, osName) == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string generateOTP()
        {
            string alphabets = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string small_alphabets = "abcdefghijklmnopqrstuvwxyz";
            string numbers = "1234567890";

            string characters = numbers;

            characters += numbers;

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
            return otp;
        }

        public string getPhoneByEmail(string email)
        {
            //DateTime currentTime = DateTime.Now;
            //DateTime next5Minutes = currentTime.AddMinutes(5);


            return db.TBL_M_USERs.Where(a => a.EMAIL == email).FirstOrDefault().TELEPON;
        }

        public bool SaveImage(string ImgStr, string ImgName)
        {
            try
            {
                string pathUploadProfilePhoto = ConfigurationManager.AppSettings["pathUploadProfilePhoto"].ToString();
                String path = HttpContext.Current.Server.MapPath(pathUploadProfilePhoto); //Path

                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path); //Create directory if it doesn't exist
                }
                string imageName = ImgName + ".jpg";

                //set the image path
                string imgPath = Path.Combine(path, imageName);
                byte[] imageBytes = Convert.FromBase64String(ImgStr);
                //write file from base64 string
                File.WriteAllBytes(imgPath, imageBytes);

                //compress image to 50% quality
                //Encoder parameter for image quality 
                Image imgSource = Image.FromFile(imgPath, true);
                Image imgPhoto = null;
                int quality = 50;

                //EncoderParameter qualityParam = new EncoderParameter(Encoder.Quality, quality);
                ////JPEG image codec 
                //ImageCodecInfo jpegCodec = GetEncoderInfo("image/jpeg");
                //EncoderParameters encoderParams = new EncoderParameters(1);
                //encoderParams.Param[0] = qualityParam;
                //img.Save(path, jpegCodec, encoderParams);

                imgPhoto = ScaleByPercent(imgSource, quality);
                imgSource.Dispose();
                imgPhoto.Save(imgPath, ImageFormat.Jpeg);
                imgPhoto.Dispose();

                var query = db.TBL_M_USERs.Where(a => a.FOTO == ImgName).FirstOrDefault();
                TBL_M_USER foto = new TBL_M_USER();
                foto.FOTO = ImgName;
                db.TBL_M_USERs.InsertOnSubmit(foto);

                db.SubmitChanges();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private static Image ScaleByPercent(Image imgPhoto, int Percent)
        {
            float nPercent = ((float)Percent / 100);

            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;

            int destX = 0;
            int destY = 0;
            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(destWidth, destHeight,
                PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

    }

    public class validasiEmail
    {
        public string EMAIL { get; set; }
    }

    public class changePassword
    {
        public string EMAIL { get; set; }
        public string PASSWORD { get; set; }
    }

    //public bool Login()
    //{
    //    bool status = false;
    //    bool status_login = false;

    //    if(status_login == true)
    //    {
    //        var data_user = db
    //    }
    //}
}