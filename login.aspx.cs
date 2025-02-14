using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using DB2;
using System.Web.Configuration;
namespace test
{
    public partial class login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                TextBox3.Text = "";
                TextBox4.Text = "";
            }
        }

        protected void login_Click(object sender, EventArgs e)
        {
            string inputAccount = TextBox3.Text;
            string inputPassword = TextBox4.Text;

            Account accountInfo = new Account();
            if (accountInfo.Login(new Account { uAccount = inputAccount, uPassword = inputPassword }, "user") == true)
            {
                Label1.Text = "";
                Session["login"] = inputAccount;
                Response.Redirect("home.aspx");
            }
            else
            {
                if (accountInfo.Login(new Account { uAccount = inputAccount, uPassword = inputPassword }, "admini") == true)
                {
                    Label1.Text = "";
                    Session["clogin"] = inputAccount;
                    Response.Redirect("home.aspx");
                }
                else
                {
                    Label1.Text = "輸入錯誤";
                }
            }
            Label1.Text = "輸入錯誤";
        }
        protected void sign_up_Click(object sender, EventArgs e)
        {
            Response.Redirect("signup.aspx");
        }
    }
}