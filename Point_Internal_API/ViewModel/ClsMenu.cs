using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Point_Internal_API.Models;
using System.Configuration;

namespace Point_Internal_API.ViewModel
{
    public class ClsMenu
    {
        DataClassesDataContext db = new DataClassesDataContext();
        public string ICON { get; set; }
        public string NAMA { get; set; }
        public string URL_ANDROID { get; set; }
        public string URL_IOS { get; set; }
        public string STATUS { get; set; }
    }
}