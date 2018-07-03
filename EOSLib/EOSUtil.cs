﻿using NLog;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EOSLib
{
    public static class EOSUtil
    {
        static Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime EOSTimeToUTC(Int64 slotTime)
        {
            Int64 interval = 500;
            Int64 epoch = 946684800000;

            var unixEpochTime = (slotTime * interval + epoch) / 1000;
            var UTCTime = FromUnixTime(unixEpochTime);
            return UTCTime;
        }

        public static DateTime FromUnixTime(long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }

        public static void updateProxyVotersWithProducerInfo(ref List<EOSVoter_row> voters)
        {

            //logger.Info("Correting proxy voter data");
            int proxyVoterCount = 0;
            // Loop through the full resultset and correct producer list on those that chose to vote via proxy
            foreach (var row in voters)
            {
                //If this voter voted by proxy
                if (!string.IsNullOrEmpty(row.proxy))
                {
                    proxyVoterCount++;
                    //Find the proxy that voted for this voter and link the same producers to this account. 
                    var proxyWhoVoted = voters.Find(x => x.owner.Equals(row.proxy));
                    row.producers = proxyWhoVoted.producers;
                }

            }

            logger.Info("{0} proxy votes updated", proxyVoterCount);

        }

        public static List<dynamic> filterFields<T>(List<string> properties, List<T> data) where T: IEOSTable
        {
            List<dynamic> objList = new List<dynamic>();

            foreach (var item in data)
            {
                dynamic cleanObj = new ExpandoObject();
                var obj = (IDictionary<string, object>)cleanObj;

                foreach (var property in properties)
                {
                    var value = item.GetType().GetProperty(property).GetValue(item).ToString();
                    obj.Add(property, value);
                }
                objList.Add(obj);
            }

            return objList;
        }
    }


    public static class StringUtil
    {
        public static string ToCsv<T>(string separator, IEnumerable<T> objectlist)
        {
            Type t = typeof(T);
            FieldInfo[] fields = t.GetFields();

            string header = String.Join(separator, fields.Select(f => f.Name).ToArray());

            StringBuilder csvdata = new StringBuilder();
            csvdata.AppendLine(header);

            foreach (var o in objectlist)
                csvdata.AppendLine(ToCsvFields(separator, fields, o));

            return csvdata.ToString();
        }

        public static string ToCsvFields(string separator, FieldInfo[] fields, object o)
        {
            StringBuilder linie = new StringBuilder();

            foreach (var f in fields)
            {
                if (linie.Length > 0)
                    linie.Append(separator);

                var x = f.GetValue(o);

                if (x != null)
                    linie.Append(x.ToString());
            }

            return linie.ToString();
        }
    }
}
