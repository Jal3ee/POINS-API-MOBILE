using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Point_Internal_API.Models;
using System.Configuration;

namespace Point_Internal_API.ViewModel
{
    public class ClsAppVersion
    {
        DataClassesDataContext db = new DataClassesDataContext();
        public string OS_NAME { get; set; }
        public string APP_VERSION { get; set; }
    }
}