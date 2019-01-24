using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using Model;
using System.Net;
using System.Drawing;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using ICSharpCode.SharpZipLib;
using NPOI;
using Newtonsoft.Json;

namespace CookieTest.Controllers
{
    public class TTController : Controller
    {
        // GET: TT
        
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult two(string username)
        {
            ViewBag.name = Session["name"];
            return View();
        }
        public ActionResult SecurityCode()
        {

            string oldcode = Session["SecurityCode"] as string;
            string code = CreateRandomCode(5);
            Session["SecurityCode"] = code;
            return File(CreateValidateGraphic(code), "image/Jpeg");
        }


        private byte[] CreateImage(string checkCode)
        {
            int iwidth = (int)(checkCode.Length * 12);
            System.Drawing.Bitmap image = new System.Drawing.Bitmap(iwidth, 20);
            Graphics g = Graphics.FromImage(image);
            Font f = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            Brush b = new System.Drawing.SolidBrush(Color.White);
            g.Clear(Color.Blue);
            g.DrawString(checkCode, f, b, 3, 3);
            Pen blackPen = new Pen(Color.Black, 0);
            Random rand = new Random();
            for (int i = 0; i < 5; i++)
            {
                int x1 = rand.Next(image.Width);
                int x2 = rand.Next(image.Width);
                int y1 = rand.Next(image.Height);
                int y2 = rand.Next(image.Height);
                g.DrawLine(new Pen(Color.Silver, 1), x1, y1, x2, y2);
            }
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }

        private string CreateRandomCode(int codeCount)
        {
            string allChar = "0,1,2,3,4,5,6,7,8,9,A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,W,X,Y,Z";
            string[] allCharArray = allChar.Split(',');
            string randomCode = "";
            int temp = -1;
            Random rand = new Random();
            for (int i = 0; i < codeCount; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(i * temp * ((int)DateTime.Now.Ticks));
                }
                int t = rand.Next(35);
                if (temp == t)
                {
                    return CreateRandomCode(codeCount);
                }
                temp = t;
                randomCode += allCharArray[t];
            }
            return randomCode;
        }
        /// <summary>
        /// 创建验证码的图片
        /// </summary>
        public byte[] CreateValidateGraphic(string validateCode)
        {
            Bitmap image = new Bitmap((int)Math.Ceiling(validateCode.Length * 16.0), 27);
            Graphics g = Graphics.FromImage(image);
            try
            {
                //生成随机生成器
                Random random = new Random();
                //清空图片背景色
                g.Clear(Color.White);
                //画图片的干扰线
                for (int i = 0; i < 25; i++)
                {
                    int x1 = random.Next(image.Width);
                    int x2 = random.Next(image.Width);
                    int y1 = random.Next(image.Height);
                    int y2 = random.Next(image.Height);
                    g.DrawLine(new Pen(Color.Silver), x1, y1, x2, y2);
                }
                Font font = new Font("Arial", 13, (FontStyle.Bold | FontStyle.Italic));
                LinearGradientBrush brush = new LinearGradientBrush(new Rectangle(0, 0, image.Width, image.Height),
                 Color.Blue, Color.DarkRed, 1.2f, true);
                g.DrawString(validateCode, font, brush, 3, 2);
                //画图片的前景干扰点
                for (int i = 0; i < 100; i++)
                {
                    int x = random.Next(image.Width);
                    int y = random.Next(image.Height);
                    image.SetPixel(x, y, Color.FromArgb(random.Next()));
                }
                //画图片的边框线
                g.DrawRectangle(new Pen(Color.Silver), 0, 0, image.Width - 1, image.Height - 1);
                //保存图片数据
                MemoryStream stream = new MemoryStream();
                image.Save(stream, ImageFormat.Jpeg);
                //输出图片流
                return stream.ToArray();
            }
            finally
            {
                g.Dispose();
                image.Dispose();
            }
        }
        static int num=0;
        public ActionResult islogin(string username,string pwd)
        {
            
            SqlConnection conn = new SqlConnection("Data Source=DESKTOP-3SRQAO3;Initial Catalog=ks1122;Integrated Security=True");
            conn.Open();
            string sql = string.Format("select count(*) from loginin where uname ='{0}' and pwd='{1}'", username, pwd);
            SqlCommand cmd = new SqlCommand(sql, conn);
            int i = Convert.ToInt32(cmd.ExecuteScalar());
            string ck="";
            if(i>0)
            {
                Session["username"] = username;
                //ViewBag.username = Session["username"];
                //cookies
                //登陆成功后写
                Request.Cookies.Add(new HttpCookie("uname", username));
                ////判断是否登陆成功
                var cookie = Request.Cookies.Get("uname");
                ck = cookie.Value;
                Session["name"] = ck;
                return Json(new { ckName = ck }, JsonRequestBehavior.AllowGet);
                //写法2
                //Request.Cookies["uname"].Value = "fantao";
                //Request.Cookies["uname"].Expires =DateTime.Now.AddDays(2);
            }
            else
            {
                
                num++;
                Session["error"] = num;
                if(num>=3)
                {
                    return Json(new { ckName = "3" }, JsonRequestBehavior.AllowGet);
                }
            };
            conn.Close();
            return Json(new { ckName = "登录失败" }, JsonRequestBehavior.AllowGet);
        }
        public string Daochu()
        {
            Dictionary<string, string> dir = new Dictionary<string, string>()
            {
                {"lid","lid"},
                {"uname","uname"},
                {"pwd","pwd"},
            };
           
            SqlConnection conn = new SqlConnection("Data Source=DESKTOP-3SRQAO3;Initial Catalog=ks1122;Integrated Security=True");
            conn.Open();
            SqlDataAdapter see = new SqlDataAdapter("select * from loginin", conn);
            DataTable dt = new DataTable();
            see.Fill(dt);
            List<um> list = JsonConvert.DeserializeObject<List<um>>(JsonConvert.SerializeObject(dt)).ToList();
            conn.Close();
            return ExcelHelper.EntityListToExcel2003(dir, list, "用户表"); ;
        }
    }
}