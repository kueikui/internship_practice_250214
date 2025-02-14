using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using System.IO;
using NPOI.XSSF.UserModel;
using System.Web.Services.Description;
using MathNet.Numerics;
using System.Globalization;
using NPOI.SS.Formula.Functions;
using System.Web.DynamicData;
using NPOI.XWPF.UserModel;
using Dapper;
using DB2;
using System.Security.Principal;
namespace test
{
    public partial class home : System.Web.UI.Page
    {
        string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["MyDBConnection"].ConnectionString;
        Account accountInfo = new Account(); // 創建 Account 物件
        #region Page_Load
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["login"] == null && Session["clogin"] == null)
                {
                    Response.Redirect("login.aspx");
                }
                if (Session["clogin"] != null)//是管理員登入
                {
                    Import.Visible = true;
                    Nav_Panel.Visible = true;
                    GridView1.Visible = true;
                    GridView2.Visible = false;
                }
                else//不是管理員
                {
                    Import.Visible = false;
                    Nav_Panel.Visible = false;
                    GridView2.Visible = true;
                    GridView1.Visible = false;
                }
                Add_Panel.Visible = false;
                AddName_text.Text = "";
                AddAccount_text.Text = "";
                AddPassword_text.Text = "";
                AddPhone_text.Text = "";
                message1.Visible = false;
                message.Visible = false;
                BindGridView();
            }

        }
        #endregion
        #region BindGridView
        private void BindGridView()
        {     
            if (Session["login"] != null)//一般
            {                  
                Account accountList = accountInfo.GetuAccount(Session["login"].ToString()); // 查詢資料
                GridView2.DataSource = new List<Account> { accountList };
                GridView2.DataBind();
            }
            else if (Session["clogin"] != null)//管理
            {
                IEnumerable<Account> accountList = accountInfo.GetAccount(); // 查詢資料
                GridView1.DataSource = accountList;
                GridView1.DataBind();
            }
        }

        #endregion BindGridView
        protected void Logout_Click(object sender, EventArgs e)
        {
            Session["login"] = null;
            Session["clogin"] = null;
            Response.Redirect("login.aspx");
        }
        #region Import/Export
        protected void Export_Click(object sender, EventArgs e)
        {
            if (ViewState["EditingRow"] != null)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "Alert", "alert('請先關閉編輯資料行');", true);
                return;
            }
            // 建立 Excel
            IWorkbook workbook = new XSSFWorkbook();
            ISheet sheet = workbook.CreateSheet("帳號列表");

            DataTable dt = new DataTable();

            GridView targetGridView = null;

            if (Session["login"] != null)
            {
                targetGridView = GridView2;
            }
            else if (Session["clogin"] != null)
            {
                targetGridView = GridView1;
            }

            if (targetGridView != null && targetGridView.HeaderRow != null)
            {
                foreach (TableCell cell in targetGridView.HeaderRow.Cells)
                {
                    if (cell.Text != "&nbsp;")
                    {
                        dt.Columns.Add(cell.Text);
                    }
                }

                // 取得資料列
                foreach (GridViewRow row in targetGridView.Rows)
                {
                    if (row.RowType == DataControlRowType.DataRow)
                    {
                        DataRow dr = dt.NewRow();
                        int columnIndex = 0;
                        dr[columnIndex++] = (row.FindControl("lblName") as Label)?.Text.Trim();
                        dr[columnIndex++] = (row.FindControl("lblAccount") as Label)?.Text.Trim();
                        dr[columnIndex++] = (row.FindControl("lblPassword") as Label)?.Text.Trim();
                        dr[columnIndex++] = (row.FindControl("lblPhone") as Label)?.Text.Trim();
                        dr[columnIndex++] = (row.FindControl("lblGender") as Label)?.Text.Trim();

                        // 將 DataRow 加入 DataTable
                        dt.Rows.Add(dr);
                    }
                }
            }

            IRow headerRow = sheet.CreateRow(0);//title
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                headerRow.CreateCell(i).SetCellValue(dt.Columns[i].ColumnName);
            }

            for (int i = 0; i < dt.Rows.Count; i++)//data
            {
                IRow row = sheet.CreateRow(i + 1);
                for (int j = 0; j < dt.Columns.Count; j++)
                {
                    row.CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
                }
            }

            // 將 Excel 寫入 MemoryStream 儲存 Excel 檔案的資料
            using (MemoryStream exportData = new MemoryStream())
            {
                workbook.Write(exportData);
                workbook.Close();

                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment;filename=ExportData.xlsx");
                Response.BinaryWrite(exportData.ToArray());
                Response.End();
            }
        }

        protected void Import_Click(object sender, EventArgs e)
        {
            Response.Redirect("import.aspx");
        }

        #endregion
        #region AddPanel
        protected void AddPanel_btn_Click(object sender, EventArgs e)
        {
            Add_Panel.Visible = true;
            Nav_Panel.Visible = false;
        }

        protected void Add_Click(object sender, EventArgs e)
        {
            string name = AddName_text.Text.Trim();
            string account = AddAccount_text.Text.Trim();
            string password = AddPassword_text.Text.Trim();
            string phone = AddPhone_text.Text.Trim();
            string gender = AddGender_list.Text;

            if (string.IsNullOrEmpty(name))
            {
                message.Text = "姓名為必填";
                return;
            }
            else if (string.IsNullOrEmpty(account))
            {
                message.Text = "帳號為必填";
                return;
            }
            else if (string.IsNullOrEmpty(password))
            {
                message.Text = "密碼為必填";
                return;
            }
            if (phone.Length != 10)
            {
                message.Visible = true;
                message.Text = "電話格式錯誤";
                return;
            }
            
            if (accountInfo.Check(account) == true)
            {
                message.Visible = true;
                message.Text = "帳號已存在";
            }
            else
            {
                message.Visible = false;
                message.Text = "";
                Account parameters = new Account()
                {
                    uName = name,
                    uAccount = account,
                    uPassword = password,
                    uPhone = phone,
                    uGender = gender
                };
                accountInfo.Add(parameters);
                string script = "alert('新增資料成功！'); window.location='home.aspx';";
                ClientScript.RegisterStartupScript(this.GetType(), "SuccessAlert", script, true);
            }
        }

        protected void Cancel_Click(object sender, EventArgs e)
        {
            Add_Panel.Visible = false;
            Nav_Panel.Visible = true;
            message.Visible = false;
            AddName_text.Text = "";
            AddAccount_text.Text = "";
            AddPassword_text.Text = "";
            AddPhone_text.Text = "";
            AddGender_list.SelectedIndex = 0;
        }

        #endregion
        #region Search
        protected void Search_Click(object sender, EventArgs e)
        {
            if (Search_text.Text == "")
            {
                message1.Visible = true;
                message1.Text = "請輸入搜尋內容";
                BindGridView();
                return;
            }
            if (Search_list.Text.ToString() == "Name")
            { 
                ShowAll_btn.Visible = true;
                
                var accountList = accountInfo.Search(new Account { uName = Search_text.Text.Trim()}); // 查詢資料
                if (accountList.Any()) // 如果有符合的資料
                {
                    message1.Visible = false;
                    GridView1.Visible = true;
                    GridView1.DataSource = accountList;
                    GridView1.DataBind();
                }
                else
                {
                    GridView1.Visible = false;
                    message1.Visible = true;
                    message1.Text = "找不到符合的資料";
                }   
            }
            else if (Search_list.Text.ToString() == "Account")
            {
                ShowAll_btn.Visible = true;
                //var accountList = accountInfo.Search(Search_text.Text).Where(a=>a.uName.Contains("125")); // 查詢資料
                var accountList = accountInfo.Search(new Account { uAccount = Search_text.Text.Trim() });
                if (accountList.Any()) // 如果有符合的資料
                {
                    message1.Visible = false;
                    GridView1.Visible = true;
                    GridView1.DataSource = accountList;
                    GridView1.DataBind();
                }
                else
                {
                    GridView1.Visible = false;
                    message1.Visible = true;
                    message1.Text = "找不到符合的資料";
                }               
            }
            else if (Search_list.Text.ToString() == "Select")
            {
                GridView1.Visible = false;
                message1.Visible = true;
                message1.Text = "請選搜尋類別";
            }
        }
        protected void ShowAll_Click(object sender, EventArgs e)
        {
            GridView1.Visible = true;
            GridView2.Visible = true;
            ShowAll_btn.Visible = false;
            Search_text.Text = "";
            message1.Visible = false;
            message1.Text = "";
            Search_list.SelectedIndex = 0;
            BindGridView();
        }

        #endregion
        #region Update
        protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
        {
            ViewState["EditingRow"] = e.NewEditIndex;
            GridView targetGridView = null;

            if (Session["login"] != null)
            {
                targetGridView = GridView2;
            }
            else if (Session["clogin"] != null)
            {
                targetGridView = GridView1;
            }
            targetGridView.EditIndex = e.NewEditIndex;
            BindGridView();

            GridViewRow row = targetGridView.Rows[e.NewEditIndex];

            TextBox txtAccount = row.FindControl("txtAccount") as TextBox;
            if (txtAccount != null)
            {
                txtAccount.Enabled = false;
            }
        }
        protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridView targetGridView = null;
            ViewState["EditingRow"] = null;
            if (Session["login"] != null)
            {
                targetGridView = GridView2;
            }
            else if (Session["clogin"] != null)
            {
                targetGridView = GridView1;
            }
            targetGridView.EditIndex = -1;
            BindGridView();
        }
        protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            GridView targetGridView = null;
            ViewState["EditingRow"] = null;
            if (Session["login"] != null)
            {
                targetGridView = GridView2;
            }
            else if (Session["clogin"] != null)
            {
                targetGridView = GridView1;
            }
            GridViewRow row = targetGridView.Rows[e.RowIndex];
            string name = (row.FindControl("txtName") as TextBox).Text;
            string account = (row.FindControl("txtAccount") as TextBox).Text;
            string password = (row.FindControl("txtPassword") as TextBox).Text;
            string phone = (row.FindControl("txtPhone") as TextBox).Text;
            string gender = (row.FindControl("ddlGender") as DropDownList).SelectedValue;

            // 驗證電話號碼格式
            string phonePattern = "(\\d{10})";
            if (phone != "" && phone.Length != 10)
            {
                ClientScript.RegisterStartupScript(this.GetType(), "Alert", "alert('電話號碼格式不正確');", true);
                e.Cancel = true;
                return;
            }
            if (string.IsNullOrWhiteSpace(phone))
            {
                phone = null;
            }
            if (!string.IsNullOrEmpty(phone) && !System.Text.RegularExpressions.Regex.IsMatch(phone, phonePattern))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "Alert", "alert('電話號碼格式不正確');", true);
                e.Cancel = true;
                return;
            }
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                
                Account parameters = new Account()
                {
                    uName = name,
                    uAccount = account,
                    uPassword = password,
                    uPhone = phone,
                    uGender = gender
                };
                accountInfo.Update(parameters); // 查詢資料
            }

            targetGridView.EditIndex = -1;
            BindGridView();
        }
        #endregion
        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            GridView targetGridView = null;

            if (Session["login"] != null)
            {
                targetGridView = GridView2;
            }
            else if (Session["clogin"] != null)
            {
                targetGridView = GridView1;
            }

            int rowIndex = e.RowIndex;
            GridViewRow row = targetGridView.Rows[rowIndex];

            string parameters = (row.FindControl("lblAccount") as Label).Text;

            accountInfo.Delete(parameters); 
            BindGridView();
        }   
    }
}