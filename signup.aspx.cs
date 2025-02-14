using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Dapper;
using DB2;
using static Org.BouncyCastle.Crypto.Digests.SkeinEngine;

namespace test
{
    public partial class signup : System.Web.UI.Page
    {
        string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["MyDBConnection"].ConnectionString;
        Account accountInfo = new Account(); // 創建 Account 物件
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        protected void Button1_Click(object sender, EventArgs e)
        {
            string name = TextBox1.Text.Trim();
            string account = TextBox2.Text.Trim();
            string password = TextBox3.Text.Trim();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(account) || string.IsNullOrEmpty(password))
            {
                message.Text = "所有欄位都必須填寫";
                return;
            }

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                if (accountInfo.Check(account) == true)
                {
                    message.Text = "帳號已存在";
                }
                else
                {
                    message.Text = "";
                    Account parameters = new Account()
                    {
                        uName = name,
                        uAccount = account,
                        uPassword = password,
                    };
                    accountInfo.Add(parameters);
                    string script = "alert('新增資料成功！'); window.location='home.aspx';";
                    ClientScript.RegisterStartupScript(this.GetType(), "SuccessAlert", script, true);
                }
            }
        }
        protected void Button2_Click(object sender, EventArgs e)
        {
            Response.Redirect("login.aspx");
        }
    }
}