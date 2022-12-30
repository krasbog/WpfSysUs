using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Net;
using ExcelStreamLateBinding;
using System.Windows;
using System.Xml.Linq;
using System.Data;



namespace WpfSysUs.Models
{
    class SystemUserModel
    {


        private readonly ICollection<string> _strings;
        private readonly ICollection<SystemUser> _systemUsers;
        private readonly ICollection<string> _errorStrings;

        [Obsolete]
        public SystemUserModel(ICollection<string> strings,
                               ICollection<SystemUser> systemUsers,
                               ICollection<string> errorStrings)
        {
            _systemUsers = systemUsers;
            _errorStrings = errorStrings;
            _strings = strings;
            ParseLogFile();
            SaveErrorStringsToFile();
        }

        void getStrings()
        {
            string fullPath = Environment.CurrentDirectory + "\\LOG.txt";
            StreamReader reader = new StreamReader(fullPath);
            if (File.Exists(fullPath))
            {
                while (true)
                {
                    string s = reader.ReadLine();
                    if (s == null) break;
                    _strings.Add(s);
                }


            }

        }
        void SaveErrorStringsToFile()
        {
            string fullPath = Environment.CurrentDirectory + "\\ERROR.txt";
            StreamWriter sw = File.CreateText(fullPath);
            foreach (var errorString in _errorStrings)
            {
                sw.WriteLine(errorString);
            }
            sw.Close();

        }

        [Obsolete]
        void ParseLogFile()
        {
            getStrings();
            foreach (var item in _strings)
            {
                string[] subs = item.Split(';');
                SystemUser su = new SystemUser();

                if (
                    !IsValidIP(subs[3])
                    ||
                    !IsValidDateTimeSpan(subs[5], subs[6])
                    )

                    _errorStrings.Add(item);
                else
                {
                    su.ID = int.Parse(subs[0]);
                    su.Name = subs[1];
                    su.Organization = subs[2];
                    su.IP = IPAddress.Parse(subs[3]);
                    su.longIP = IPAddress.Parse(subs[3]).Address;
                    su.SessionID = subs[4];
                    su.DateTimeLog = DateTime.Parse(subs[5]);
                    su.DateTimeLogOut = DateTime.Parse(subs[6]);
                    su.TerminationCode = subs[7];

                    _systemUsers.Add(su);

                }


            }

        }

       

       
        bool isValidString(string str, string pattern)
        {
            Regex check = new Regex(pattern);
            bool valid = false;
            if (str == "")
            {
                valid = false;
            }
            else
            {
                valid = check.IsMatch(str, 0);
            }
            return valid;

        }
        bool IsValidIP(string ipStr)
        {
            string pattern = @"^(0[0-7]{10,11}|0(x|X)[0-9a-fA-F]{8}|(\b4\d{8}[0-5]\b|\b[1-3]?\d{8}\d?\b)|((2[0-5][0-5]|1\d{2}|[1-9]\d?)|(0(x|X)[0-9a-fA-F]{2})|(0[0-7]{3}))(\.((2[0-5][0-5]|1\d{2}|\d\d?)|(0(x|X)[0-9a-fA-F]{2})|(0[0-7]{3}))){3})$";
            return isValidString(ipStr, pattern);
        }
        bool IsValidDateTime(string dtStr)
        {
            string pattern = @"^([0]?[1-9]|[1|2][0-9]|[3][0|1])[.]([0]?[1-9]|[1][0-2])[.]([0-9]{4}|[0-9]{2})[ ](([0-1]?[0-9])|([2][0-3])):([0-5]?[0-9])(:([0-5]?[0-9]))?$";
            return isValidString(dtStr, pattern);
        }
        bool IsValidDateTimeSpan(string dtStrLog, string dtStrLogOut)
        {
            bool isValid = false;
            if (IsValidDateTime(dtStrLogOut) && IsValidDateTime(dtStrLog))
                isValid = DateTime.Parse(dtStrLogOut) > DateTime.Parse(dtStrLog);
            return isValid;

        }
       

        public void MakeReport(
            string filePath,
            bool isPeriod,
            DateTime dateTimeFrom,
            DateTime dateTimeTo,
            bool param1,
            bool param2,
            bool param3,
            bool param4,
            bool param5)
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            string fileExtension = Path.GetExtension(filePath);
            if(fileExtension==".xls")
            {
                if (param1) MakeReport1_xls(filePath, dateTimeFrom);
                if (param2 && isPeriod) MakeReport2_xls(filePath, dateTimeFrom, dateTimeTo);
                if (param3) MakeReport3_xls(filePath);
                if (param4 && isPeriod) MakeReport4_xls(filePath, dateTimeFrom, dateTimeTo);
                if (param5 && isPeriod) MakeReport5_xls(filePath, dateTimeFrom, dateTimeTo);
            }
            if (fileExtension == ".xml")
            {
                string fp = Path.GetFileNameWithoutExtension(filePath);
                if (param1) MakeReport1_xml(fp, dateTimeFrom);
                if (param2 && isPeriod) MakeReport2_xml(fp, dateTimeFrom, dateTimeTo);
                if (param3) MakeReport3_xml(fp);
                if (param4 && isPeriod) MakeReport4_xml(fp, dateTimeFrom, dateTimeTo);
                if (param5 && isPeriod) MakeReport5_xml(fp, dateTimeFrom, dateTimeTo);

            }


            }

        void MakeReport1_xls (string filePath, DateTime dateTimeFrom)
        {
            ExcelWriter excelWriter = new ExcelWriter();
            excelWriter.Open(filePath, "Отчет1");
            StringBuilder sb = new StringBuilder();

            List<string> HeaderNames = new List<string> { "№", "Пользователь", "Организация", "IP адрес",
        "Уникальный идентификатор сессии", "Дата и время входа в систему",
            "Дата и время выхода из системы", "Код завершения сессии"};
            foreach (var i in HeaderNames)
            {
                sb.Append(i);
                sb.Append("\t");
            }
            sb.Append("\n");

            var subset = from i in _systemUsers
                         where (i.DateTimeLog.Date).ToString("d") == dateTimeFrom.Date.ToString("d")
                         select i;
            foreach (var user in subset)
            {
                sb.Append(user.ID.ToString() + "\t");
                sb.Append(user.Name.ToString() + "\t");
                sb.Append(user.Organization.ToString() + "\t");
                sb.Append(user.IP.ToString() + "\t");
                sb.Append(user.SessionID.ToString() + "\t");
                sb.Append(user.DateTimeLog.ToString() + "\t");
                sb.Append(user.DateTimeLogOut.ToString() + "\t");
                sb.Append(user.TerminationCode.ToString() + "\n");

            }

            Clipboard.SetText(sb.ToString());
            excelWriter.PasteFromClipboard();
            excelWriter.Close();

        }
        void MakeReport2_xls(string filePath, DateTime dateTimeFrom, DateTime dateTimeTo)
        {
            ExcelWriter excelWriter = new ExcelWriter();
            excelWriter.Open(filePath, "Отчет2");
            StringBuilder sb = new StringBuilder();

            List<string> HeaderNames = new List<string> { "№", "Пользователь", "IP адрес",
         "Дата и время входа в систему", "Дата и время выхода из системы", "Количество подключений"};
            foreach (var i in HeaderNames)
            {
                sb.Append(i);
                sb.Append("\t");
            }
            sb.Append("\n");


            var userIP_Groups = from i in _systemUsers
                          where i.DateTimeLog >= dateTimeFrom && i.DateTimeLog <= dateTimeTo
                          group i by i.IP into g
                          select new
                          {
                              Count = g.Count(),
                              Users = from i in g select i
                          };

            foreach (var userIP_Group in userIP_Groups)
            {
                foreach (var user in userIP_Group.Users)
                {
                    sb.Append(user.ID.ToString() + "\t");
                    sb.Append(user.Name.ToString() + "\t");
                   
                    sb.Append(user.IP.ToString() + "\t");
                   
                    sb.Append(user.DateTimeLog.ToString() + "\t");
                    sb.Append(user.DateTimeLogOut.ToString() + "\t");
                    sb.Append(userIP_Group.Count.ToString() + "\n");

                }

            }

            Clipboard.SetText(sb.ToString());
            excelWriter.PasteFromClipboard();
            excelWriter.Close();
        }

        void MakeReport3_xls(string filePath)
        {
            ExcelWriter excelWriter = new ExcelWriter();
            excelWriter.Open(filePath, "Отчет3");
            StringBuilder sb = new StringBuilder();

            List<string> HeaderNames = new List<string> { "№", "Пользователь", "IP адрес",
          "Время работы"};
            foreach (var i in HeaderNames)
            {
                sb.Append(i);
                sb.Append("\t");
            }
            sb.Append("\n");


            var user_Groups = from i in _systemUsers
                                group i by i.Organization into g
                                select new
                                {
                                    Count = g.Count(),
                                    Users = from i in g select i
                                };

            foreach (var user_Group in user_Groups)
            {
                double workTimes = 0;
                foreach (var user in user_Group.Users)
                {
                    sb.Append(user.ID.ToString() + "\t");
                    sb.Append(user.Name.ToString() + "\t");

                    sb.Append(user.IP.ToString() + "\t");

                   double workTime = (user.DateTimeLogOut - user.DateTimeLog).TotalSeconds;
                    workTimes += workTime;

                    sb.Append(workTime.ToString() + "\n");
                   

                }
                sb.Append("Всего" + "\t");
                sb.Append(user_Group.Users.First().Organization + "\t");

                sb.Append("-" + "\t");

                sb.Append(workTimes.ToString() + "\n");
                

            }

            Clipboard.SetText(sb.ToString());
            excelWriter.PasteFromClipboard();
            excelWriter.Close();
        }

        void MakeReport4_xls(string filePath, DateTime dateTimeFrom, DateTime dateTimeTo)
        {
            ExcelWriter excelWriter = new ExcelWriter();
            excelWriter.Open(filePath, "Отчет4");
            StringBuilder sb = new StringBuilder();

            List<string> HeaderNames = new List<string> { "№", "Организация", "Количество пользователей"};
            
            foreach (var i in HeaderNames)
            {
                sb.Append(i);
                sb.Append("\t");
            }
            sb.Append("\n");


            var user_Groups = from i in _systemUsers
                              where i.DateTimeLog >= dateTimeFrom && i.DateTimeLog <= dateTimeTo
                              group i by i.Organization into g
                              select new
                              {
                                  Organization = g.First().Organization,
                                  Count = (from i in g select i.Name).Distinct().Count()
                              };
            int num = 0;
            foreach (var user_Group in user_Groups)
            {
                num++;                
                sb.Append(num.ToString() + "\t");
                sb.Append(user_Group.Organization + "\t");
                sb.Append(user_Group.Count + "\n");              
            }

            Clipboard.SetText(sb.ToString());
            excelWriter.PasteFromClipboard();
            excelWriter.Close();
        }

        void MakeReport5_xls(string filePath, DateTime dateTimeFrom, DateTime dateTimeTo)
        {
            ExcelWriter excelWriter = new ExcelWriter();
            excelWriter.Open(filePath, "Отчет5");
            StringBuilder sb = new StringBuilder();

            List<string> HeaderNames = new List<string> { "№", "Пользователь", "Организация", "IP адрес",
        "Уникальный идентификатор сессии", "Дата и время входа в систему",
            "Дата и время выхода из системы"};
            foreach (var i in HeaderNames)
            {
                sb.Append(i);
                sb.Append("\t");
            }
            sb.Append("\n");

            var subset = from i in _systemUsers
                         where i.DateTimeLog >= dateTimeFrom && i.DateTimeLog <= dateTimeTo
                               && i.TerminationCode=="1"
                         select i;
            foreach (var user in subset)
            {
                sb.Append(user.ID.ToString() + "\t");
                sb.Append(user.Name.ToString() + "\t");
                sb.Append(user.Organization.ToString() + "\t");
                sb.Append(user.IP.ToString() + "\t");
                sb.Append(user.SessionID.ToString() + "\t");
                sb.Append(user.DateTimeLog.ToString() + "\t");
                sb.Append(user.DateTimeLogOut.ToString() + "\n");
                

            }

            Clipboard.SetText(sb.ToString());
            excelWriter.PasteFromClipboard();
            excelWriter.Close();

        }

        void MakeReport1_xml(string filePath, DateTime dateTimeFrom)
        {
            List<string> HeaderNames = new List<string> { "№", "Пользователь", "Организация", "IP адрес",
        "Уникальный идентификатор сессии", "Дата и время входа в систему",
            "Дата и время выхода из системы", "Код завершения сессии"};

            DataTable dt = new DataTable("Отчет1");
            foreach (var i in HeaderNames)
            {
                dt.Columns.Add(i, typeof(string));

            }

            var subset = from i in _systemUsers
                         where (i.DateTimeLog.Date).ToString("d") == dateTimeFrom.Date.ToString("d")
                         select i;

            foreach (var user in subset)
            {
                dt.Rows.Add(
                    user.ID.ToString(),
                    user.Name.ToString(),
                    user.Organization.ToString(),
                    user.IP.ToString(),
                    user.SessionID.ToString(),
                    user.DateTimeLog.ToString(),
                    user.DateTimeLogOut.ToString(),
                    user.TerminationCode.ToString()
                    );
            }

            dt.WriteXml(filePath + "_Отчет1.xml");




        }
        void MakeReport2_xml(string filePath, DateTime dateTimeFrom, DateTime dateTimeTo)
        {
           

            List<string> HeaderNames = new List<string> { "№", "Пользователь", "IP адрес",
         "Дата и время входа в систему", "Дата и время выхода из системы", "Количество подключений"};
            DataTable dt = new DataTable("Отчет2");
            foreach (var i in HeaderNames)
            {
                dt.Columns.Add(i, typeof(string));

            }


            var userIP_Groups = from i in _systemUsers
                                where i.DateTimeLog >= dateTimeFrom && i.DateTimeLog <= dateTimeTo
                                group i by i.IP into g
                                select new
                                {
                                    Count = g.Count(),
                                    Users = from i in g select i
                                };

            foreach (var userIP_Group in userIP_Groups)
            {
                foreach (var user in userIP_Group.Users)
                {
                    dt.Rows.Add(
                    user.ID.ToString(),
                    user.Name.ToString(),
                    
                    user.IP.ToString(),
                   
                    user.DateTimeLog.ToString(),
                    user.DateTimeLogOut.ToString(),
                    userIP_Group.Count.ToString()
                    );
                }

            }
            dt.WriteXml(filePath + "_Отчет2.xml");
           
        }
        void MakeReport3_xml(string filePath)
        {
            

            List<string> HeaderNames = new List<string> { "№", "Пользователь", "IP адрес",
          "Время работы"};
            DataTable dt = new DataTable("Отчет3");
            foreach (var i in HeaderNames)
            {
                dt.Columns.Add(i, typeof(string));

            }


            var user_Groups = from i in _systemUsers
                              group i by i.Organization into g
                              select new
                              {
                                  Count = g.Count(),
                                  Users = from i in g select i
                              };

            foreach (var user_Group in user_Groups)
            {
                double workTimes = 0;
                foreach (var user in user_Group.Users)
                {
                    double workTime = (user.DateTimeLogOut - user.DateTimeLog).TotalSeconds;
                    workTimes += workTime;

                    dt.Rows.Add(
                   user.ID.ToString(),
                   user.Name.ToString(),
                   
                   user.IP.ToString(),
                   workTime.ToString()

                   );

                                       
                    
                }
               

                dt.Rows.Add(
                  "Всего",
                  user_Group.Users.First().Organization,
                  "-",
                  workTimes.ToString()
                  );


            }

            dt.WriteXml(filePath + "_Отчет3.xml");
        }
        void MakeReport4_xml(string filePath, DateTime dateTimeFrom, DateTime dateTimeTo)
        {
            

            List<string> HeaderNames = new List<string> { "№", "Организация", "Количество пользователей" };

            DataTable dt = new DataTable("Отчет4");
            foreach (var i in HeaderNames)
            {
                dt.Columns.Add(i, typeof(string));

            }


            var user_Groups = from i in _systemUsers
                              where i.DateTimeLog >= dateTimeFrom && i.DateTimeLog <= dateTimeTo
                              group i by i.Organization into g
                              select new
                              {
                                  Organization = g.First().Organization,
                                  Count = (from i in g select i.Name).Distinct().Count()
                              };
            int num = 0;
            foreach (var user_Group in user_Groups)
            {
                num++;
                
                dt.Rows.Add(
                  num.ToString(),
                 user_Group.Organization,

                  user_Group.Count

                  );
            }

            dt.WriteXml(filePath + "_Отчет4.xml");
        }
        void MakeReport5_xml(string filePath, DateTime dateTimeFrom, DateTime dateTimeTo)
        {
           

            List<string> HeaderNames = new List<string> { "№", "Пользователь", "Организация", "IP адрес",
        "Уникальный идентификатор сессии", "Дата и время входа в систему",
            "Дата и время выхода из системы"};
            DataTable dt = new DataTable("Отчет5");
            foreach (var i in HeaderNames)
            {
                dt.Columns.Add(i, typeof(string));

            }

            var subset = from i in _systemUsers
                         where i.DateTimeLog >= dateTimeFrom && i.DateTimeLog <= dateTimeTo
                               && i.TerminationCode == "1"
                         select i;
            foreach (var user in subset)
            {
              
                dt.Rows.Add(
                   user.ID.ToString(),
                   user.Name.ToString(),
                   user.Organization.ToString(),
                   user.IP.ToString(),
                   user.SessionID.ToString(),
                   user.DateTimeLog.ToString(),
                   user.DateTimeLogOut.ToString()
                   
                   );


            }
            dt.WriteXml(filePath + "_Отчет5.xml");

        }


    }
}
