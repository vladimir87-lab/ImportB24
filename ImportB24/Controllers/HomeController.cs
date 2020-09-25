using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Data.SqlClient;
using System.Configuration;



namespace ImportB24.Controllers
{
    public class HomeController : Controller
    {
        SQL sql = new SQL();
        Function func = new Function();

        [HttpGet]
        public ActionResult Index()
        {
            count = 0;
            object[] ob = sql.SelectUserB24();
            try
            {
                ViewBag.Domen = ob[1].ToString();
                ViewBag.Hesh = ob[2].ToString();
            }
            catch { }
            return View();
        }

        [HttpPost]
        public ActionResult Index(string Domen, string Hesh)
        {
            List<object[]> nametask = new List<object[]>();
            var data = new
            {
                filter = new
                {
                    TAG = "oktell"
                },
                select = new[] { "UF_CRM_TASK", "TITLE", "TAG" }
            };
            string contentText = JsonConvert.SerializeObject(data).ToString();
            string content;
            try
            {
                using (xNet.HttpRequest request = new xNet.HttpRequest())
                {
                    content = request.Post("https://" + Domen + "/rest/" + Hesh + "/tasks.task.list", contentText, "application/json").ToString();
                }
            }
            catch
            {
                nametask = null;
                return View(nametask);
            }
            var converter = new ExpandoObjectConverter();
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
            foreach (var item in obj.result.tasks)
            {

                nametask.Add(new object[] { item.id, item.title });
            }
            ViewBag.Domen = Domen;
            ViewBag.Hesh = Hesh;

            return View(nametask);
        }
        public static int count = 0;
        public ActionResult CountParse()
        {
            return Content(count.ToString());
        }

        public ActionResult ParsTask(int IdTaskB24, string Domen, string Hesh, string NameTbl)
        {
           
            var converter = new ExpandoObjectConverter();
            string content;
            using (xNet.HttpRequest request = new xNet.HttpRequest())
            {
                content = request.Get("https://"+ Domen + "/rest/"+ Hesh + "/tasks.task.get?id=" + IdTaskB24).ToString();
            }
            dynamic obj = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
            List<string> lstabon = new List<string>();
            foreach (var item in obj.result.task.ufCrmTask)
            {
                lstabon.Add(item);
            }
            List<object[]> lsttel = new List<object[]>();
            try
            {
                lsttel = sql.SelectUserTel(NameTbl);
            }catch
            {
                sql.CreateTable(NameTbl);
            }

           
            foreach (var item in lstabon)
            {
                DataAbonent abonent = new DataAbonent();
                string[] dataabon = item.Split('_');
                if (dataabon[0] == "C")
                {
                    using (xNet.HttpRequest request = new xNet.HttpRequest())
                    {
                        content = request.Get("https://" + Domen + "/rest/" + Hesh + "/crm.contact.get?id=" + dataabon[1]).ToString();
                    }
                    dynamic obj2 = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
                    content = System.Text.RegularExpressions.Regex.Unescape(content);
                    abonent.datajson = content;
                    try
                    {
                        abonent.telef = obj2.result.PHONE[0].VALUE;
                    }
                    catch { continue; }
                }
                else if (dataabon[0] == "CO")
                {
                    using (xNet.HttpRequest request = new xNet.HttpRequest())
                    {
                        content = request.Get("https://" + Domen + "/rest/" + Hesh + "/crm.company.get?id=" + dataabon[1]).ToString();
                    }
                    dynamic obj3 = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
                    content = System.Text.RegularExpressions.Regex.Unescape(content);
                    abonent.datajson = content;
                    try
                    {
                        abonent.telef = obj3.result.PHONE[0].VALUE;
                    }
                    catch { continue; }

                }
                else if (dataabon[0] == "L")
                {
                    using (xNet.HttpRequest request = new xNet.HttpRequest())
                    {
                        content = request.Get("https://" + Domen + "/rest/" + Hesh + "/crm.lead.get?id=" + dataabon[1]).ToString();
                    }
                    dynamic obj4 = JsonConvert.DeserializeObject<ExpandoObject>(content, converter);
                    content = System.Text.RegularExpressions.Regex.Unescape(content);
                    abonent.datajson = content;
                    try
                    {
                        abonent.telef = obj4.result.PHONE[0].VALUE;
                    }
                    catch { continue; }
                }
                string strtel = new string(abonent.telef.Where(t => char.IsDigit(t)).ToArray());
                strtel = func.FilterNumber(strtel);
                if (strtel == "") { continue; }
                if (!lsttel.Select(i => i[1]).Contains(strtel))
                {
                    sql.ImportDataTel(NameTbl, strtel, abonent.datajson, IdTaskB24);
                    count++;
                }
                System.Threading.Thread.Sleep(200);
            }


            return Content("<div style=\"text-align: center; padding: 20px; font-size: 22px; \"><div>Импорт завершен! Всего: " + count + " записей.</div><div><a href=\"/\">Назад</a></div></div>");
        }


    }
    public class DataAbonent
    {
        public string telef { get; set; }
        public string datajson { get; set; }
    }

    public class Function
    {
        public string FilterNumber(string tel)
        {
            string strtel = new string(tel.Where(t => char.IsDigit(t)).ToArray());
            if (strtel.Substring(0, 3) == "375")
            {
                if (strtel.Length != 12) { return ""; }
            }
            else if ((strtel.Substring(0, 2) == "80") && (strtel.Length == 11))
            {
                strtel = strtel.Replace("80", "375");
                return strtel;
            }
            else { return ""; }
            return strtel;
        }
    }
    public class SQL
    {
        public SqlConnection conn;
        public string connstr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public void ConnectSQLServer()
        {
            conn = new SqlConnection(connstr);
            conn.Open();
        }
        public List<object[]> SelectUserTel(string nametable)
        {
            ConnectSQLServer();
            SqlCommand cmd = new SqlCommand("SELECT * FROM [oktell].[dbo].["+ nametable + "]", conn);
            SqlDataReader reader = cmd.ExecuteReader();
            List<object[]> result = new List<object[]>();
            while (reader.Read())
            {
                object[] str = new Object[reader.FieldCount];
                int fieldCount = reader.GetValues(str);
                result.Add(str);
            }
            reader.Close();
            conn.Close();
            conn.Dispose();
            return result;
        }
        public bool ImportDataTel(string nametable, string tel, string datajson, int idtask)
        {
            ConnectSQLServer();

            string guid = Guid.NewGuid().ToString();
            SqlCommand cmd2 = new SqlCommand("INSERT INTO [oktell].[dbo].[" + nametable + "] (Telef, Json, IdTask, GuidTel) Values  ( '" + tel + "', N'" + datajson + "', "+ idtask + ", '" + guid + "' )", conn);
            cmd2.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();
            return true;
        }

        public bool CreateTable(string nametable)
        {
            ConnectSQLServer();           
            SqlCommand cmd2 = new SqlCommand("CREATE TABLE [oktell].[dbo].[" + nametable+"]([Id] [int] IDENTITY(1,1) NOT NULL,[Telef] [nvarchar](20) NULL,[Json] [nvarchar](max) NULL,[IdTask] [nvarchar](5) NULL,[GuidTel] [nvarchar](100) NULL, CONSTRAINT [PK_"+nametable+"] PRIMARY KEY CLUSTERED ([Id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]", conn);
            cmd2.ExecuteNonQuery();
            conn.Close();
            conn.Dispose();
            return true;
        }

        public object[] SelectUserB24()
        {
            ConnectSQLServer();
            SqlCommand cmd = new SqlCommand("SELECT * FROM [oktell].[dbo].[UserB24Import]", conn);
            SqlDataReader reader = cmd.ExecuteReader();
            object[] result = new object[3];
            while (reader.Read())
            {
                object[] str = new Object[reader.FieldCount];
                int fieldCount = reader.GetValues(str);
                result  = str;
            }
            reader.Close();
            conn.Close();
            conn.Dispose();
            return result;
        }


    }


}
