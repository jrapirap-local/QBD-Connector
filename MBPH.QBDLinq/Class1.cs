using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using MBPH.QBDLinq.Model;

namespace MBPH.QBDLinq
{
    public static class QBD
    {
        //QBXML << multiple pa laman.  Request kay API nila. Request API natin. QBD-> QBO.. QBD -> MBPH
        public static List<BillPaymentCheckRet> QueryBillPaymentCheckList(this string xml)
        {
            List<BillPaymentCheckRet> ret = new List<BillPaymentCheckRet>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml.Replace("<?xml version=\"1.0\" ?>\n", ""));
            var element = doc.GetElementsByTagName("BillPaymentCheckRet");
            for (int i = 0; i < element.Count; i++)
            {
                var json = JsonConvert.SerializeXmlNode(element[i]);
                QBObjects obj = JsonConvert.DeserializeObject<QBObjects>(json);
                ret.Add(obj.BillPaymentCheckRet);
            }
            return ret;
        }
        public static List<ClassRet> QueryClassList(this string xml)
        {
            List<ClassRet> ret = new List<ClassRet>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml.Replace("<?xml version=\"1.0\" ?>\n", ""));
            var element = doc.GetElementsByTagName("ClassRet");
            for (int i = 0; i < element.Count; i++)
            {
                var json = JsonConvert.SerializeXmlNode(element[i]);
                QBObjects obj = JsonConvert.DeserializeObject<QBObjects>(json);
                ret.Add(obj.ClassRet);
            }
            return ret;
        }
        public static List<CustomerRet> QueryProjectsList(this string xml) //Added by JRAPI
        {
            List<CustomerRet> ret = new List<CustomerRet>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml.Replace("<?xml version=\"1.0\" ?>\n", ""));
            var element = doc.GetElementsByTagName("CustomerRet");
            for (int i = 0; i < element.Count; i++)
            {
                var json = JsonConvert.SerializeXmlNode(element[i]);
                QBObjects obj = JsonConvert.DeserializeObject<QBObjects>(json);
                if (!obj.CustomerRet.SubLevel.Equals("0")) //Added validation to verify if object has subclass set to 1, this is required since customername and jobname (which is a subclass) is required together to form 1 projects
                {
                    ret.Add(obj.CustomerRet);
                }
            }
            return ret;
        }
        public static List<VendorRet> QueryVendorList(this string xml)
        {
            List<VendorRet> ret = new List<VendorRet>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml.Replace("<?xml version=\"1.0\" ?>\n", ""));
            var element = doc.GetElementsByTagName("VendorRet");
            for (int i = 0; i < element.Count; i++)
            {
                var json = JsonConvert.SerializeXmlNode(element[i]);
                QBObjects obj = JsonConvert.DeserializeObject<QBObjects>(json);
                ret.Add(obj.VendorRet);
            }
            return ret;
        }

        public static List<BillRet> QueryBillList(this string xml)
        {
            List<BillRet> ret = new List<BillRet>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml.Replace("<?xml version=\"1.0\" ?>\n", ""));
            var element = doc.GetElementsByTagName("BillRet");
            for (int i = 0; i < element.Count; i++)
            {
                var json = JsonConvert.SerializeXmlNode(element[i]);
                QBObjects obj = JsonConvert.DeserializeObject<QBObjects>(json);
                ret.Add(obj.BillRet);
            }
            return ret;

        }
        public static List<AccountRet> QueryAccountList(this string xml)
        {
            List<AccountRet> ret = new List<AccountRet>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml.Replace("<?xml version=\"1.0\" ?>\n", ""));
            var element = doc.GetElementsByTagName("AccountRet");
            for (int i = 0; i < element.Count; i++)
            {
                var json = JsonConvert.SerializeXmlNode(element[i]);
                QBObjects obj = JsonConvert.DeserializeObject<QBObjects>(json);
                ret.Add(obj.AccountRet);
            }
            return ret;
        }


        public static List<ListDeletedRet> QueryDeletedList(this string xml)
        {
            List<ListDeletedRet> ret = new List<ListDeletedRet>();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml.Replace("<?xml version=\"1.0\" ?>\n", ""));
            var element = doc.GetElementsByTagName("ListDeletedRet");
            for (int i = 0; i < element.Count; i++)
            {
                var json = JsonConvert.SerializeXmlNode(element[i]);
                QBObjects obj = JsonConvert.DeserializeObject<QBObjects>(json);
                ret.Add(obj.ListDeletedRet);
            }
            return ret;
        }

    }
}
