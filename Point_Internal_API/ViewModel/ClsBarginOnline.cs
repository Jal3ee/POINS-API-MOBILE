using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Point_Internal_API.ViewModel
{
    public class ClsBarginOnline
    {
        public int Id { get; set; }
        public string Company { get; set; }
        public string Barge { get; set; }
        public string TugBoat { get; set; }
    }

    public class ClsActiveBarging
    {
        public string Jetty { get; set; }
        public string Company { get; set; }
        public string Barge { get; set; }
        public string Tugboat { get; set; }
        public int CurrentCapacity { get; set; }
        public int TotalCapacity { get; set; }
        public string progress { get; set; }
    }

    public class ClsCreateBarginOnline
    {
        public int Id { get; set; }
        public string Company { get; set; }
        public string Barge { get; set; }
        public string TugBoat { get; set; }
        public string Jetty { get; set; }
        public int Capacity { get; set; }
        public int ProcessTime { get; set; }
        public DateTime DateBooking { get; set; }
        public int StartTime { get; set; }
        public int Id_User { get; set; }
        public int FinishTime { get { return StartTime + ProcessTime; } set { } } // calculate FinishTime as StartTime + ProcessTime
        public string Vessel { get; set; }
        public DateTime? FinishBooking
        {
            get
            {
                if (FinishTime > 24)
                {
                    return DateBooking.AddDays(1);
                }
                else
                {
                    return null;
                }
            }
            set { }
        }
        public string Status { get; set; }
    }

    public class RequestData
    {
        public List<RequestDataItem> Company { get; set; }
        public List<RequestDataItem> Barge { get; set; }
        public List<RequestDataItem> TugBoat { get; set; }
    }

    public class RequestDataItem
    {
        public int Id { get; set; }
        public string Company { get; set; }
        public string Barge { get; set; }
        public string TugBoat { get; set; }
    }
}