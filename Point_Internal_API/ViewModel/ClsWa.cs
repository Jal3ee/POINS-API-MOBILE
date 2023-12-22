using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Point_Internal_API.Models;

namespace Point_Internal_API.ViewModel
{
    public class ClsWa
    {
        DataClassesWADataContext wa = new DataClassesWADataContext();

        public void sendMessage(string destinationNo, string message, string senderID)
        {
            tbl_outbox b = new tbl_outbox();
            b.destination = destinationNo;
            b.isContact = 0;
            b.messageType = 0;
            b.messageText = message;
            b.senderId = senderID;
            b.dateInsert = DateTime.Now;
            b.sendAfter = DateTime.Now;
            wa.tbl_outboxes.InsertOnSubmit(b);
            wa.SubmitChanges();
        }
    }
}