using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Point_Internal_API.Models;

namespace Point_Internal_API.ViewModel
{
    public class ClsCompany
    {
        DataClassesDataContext db = new DataClassesDataContext();
        public int ID { get; set; }
        public string CUSTOMER { get; set; }
    }

    public class ClsBarge
    {
        public int ID { get; set; }
        public string BARGE { get; set; }
    }

    public class ClsTugBoat
    {
        public int ID { get; set; }
        public string TUG_BOAT { get; set; }
    }
}