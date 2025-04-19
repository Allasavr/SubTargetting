using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubTargetting
{
    internal class Sub
    { 
        public int id { get; set; }

        private string subName, subType, date, endDate, payType;

        public string SubName
        {
            get { return subName; }
            set { subName = value; }
        }

        public string SubType
        {
            get { return subType; }
            set { subType = value; }
        }

        public string Date
        {
            get { return date; }
            set { date = value; }
        }

        public string EndDate
        {
            get { return endDate; }
            set { endDate = value; }
        }

        private float payment;
        public float Payment
        {
            get { return payment; }
            set { payment = value; }
        }

        public string PayType
        {
            get { return payType; }
            set { payType = value; }
        }

        public Sub() { }

        public Sub(string subName, string subType, string date, string endDate, float payment,
             string payType)
        {
            this.subName = subName;
            this.subType = subType;
            this.date = date;
            this.endDate = endDate;
            this.payment = payment;
            this.payType = payType;

        }
    }
}
