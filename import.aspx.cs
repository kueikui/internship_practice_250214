﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using System.Data;

using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.IO;
using System.Web.Services.Description;
using System.Web.DynamicData;
using DB2;
namespace test
{
    public partial class import : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["login"] == null && Session["clogin"] == null)
                {
                    Response.Redirect("login.aspx");
                }
                BindGridView();
                message.Visible = false;
            }
        }
        private void BindGridView()
        {
            Account accountInfo = new Account(); // 創建 Account 物件
            IEnumerable<Account> accountList = accountInfo.GetAccount(); // 查詢資料
            GridView1.DataSource = accountList;
            GridView1.DataBind();
        }
        protected void Button1_Click(object sender, EventArgs e)
        {
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["MyDBConnection"].ConnectionString;
            string errorMsg = "";

            // 取得伺服器上 Uploads 資料夾的路徑
            string uploadFolder = Server.MapPath("~/Uploads/");

            // 確保資料夾存在，如果不存在則創建
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            // 檔案路徑
            string filePath = Path.Combine(uploadFolder, formFile.FileName);

            if (Path.Combine(uploadFolder, formFile.FileName) == uploadFolder)
            {
                message.Visible = true;
                message.Text = "請輸入資料來源";
                return;
            }
            else
            {
                // 儲存檔案到伺服器
                formFile.SaveAs(filePath);
                message.Visible = false;
                DataTable dataTable;
                int result = ReadExcelToDataTable(filePath, out dataTable, out errorMsg); // 檢查資料

                if (result == 1) // 如果有空值(name account password)或格式錯誤(phone gender)
                {
                    Response.Write("<script>alert('" + errorMsg + "')</script>"); 
                    return;
                }

                int check=CheckDatabase(dataTable, connStr); // 保存資料到資料庫
                if (check == 0)
                {
                    Response.Write("<script>alert('存入成功')</script>");
                }
                BindGridView();
            }
        }


        static int ReadExcelToDataTable(string filePath, out DataTable dt, out string errorMsg)
        {
            dt = new DataTable();
            errorMsg = "";
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook;
                if (Path.GetExtension(filePath) == ".xls")
                    workbook = new HSSFWorkbook(fs);  // 讀取舊版 Excel (.xls)
                else
                    workbook = new XSSFWorkbook(fs);  // 讀取新版 Excel (.xlsx)

                ISheet sheet = workbook.GetSheetAt(0); // 讀取第一個工作表
                IRow headerRow = sheet.GetRow(0);
                int cellCount = headerRow.LastCellNum;//有多少欄位

                // 建立 DataTable 欄位
                for (int i = 0; i < cellCount; i++)
                    dt.Columns.Add(headerRow.GetCell(i).ToString());

                for (int i = 1; i <= sheet.LastRowNum; i++)//有幾筆資料
                {
                    IRow row = sheet.GetRow(i);

                    DataRow dataRow = dt.NewRow();
                    bool hasEmptyRequiredField = false;

                    for (int j = 0; j < cellCount; j++) // 讀取每一欄
                    {
                        ICell cell = row.GetCell(j);
                        string cellValue = cell?.ToString().Trim() ?? "";

                        // 只檢查前三欄是否為空
                        if (j < 3 && string.IsNullOrEmpty(cellValue))
                        {
                            errorMsg = $"第 {i + 1} 列，第 {j + 1} 欄，資料不得為空";
                            hasEmptyRequiredField = true;
                            break;
                        }
                        // 檢查電話欄位格式
                        if (j == 3)
                        {
                            if (!string.IsNullOrEmpty(cellValue) && !IsValidPhoneNumber(cellValue)) // 如果電話不為空且格式不正確
                            {
                                errorMsg = $"第 {i} 筆資料，第 {j + 1} 欄，電話格式不正確，必須是 09 開頭且後面是 8 個數字";
                                hasEmptyRequiredField = true;
                                break;
                            }
                        }
                        // 檢查性別欄位  return為1也有執行68~73要跳出警示框，但實際上並沒有跳出
                        if (j == 4)
                        {
                            if ( !(string.IsNullOrEmpty(cellValue) || cellValue == "Male" || cellValue == "Female"))
                            {
                                errorMsg = $"第 {i + 1} 列，第 {j + 1} 欄，性別輸入錯誤，必須是\"Male\"或\"Female\" ";
                                hasEmptyRequiredField = true;
                                break;
                            }
                        }
                        dataRow[j] = cellValue;
                    }

                    if (hasEmptyRequiredField)
                        return 1;

                    dt.Rows.Add(dataRow);
                }
            }
            return 0;
        }
        static int CheckDatabase(DataTable dt, string connectionString)//檢查是否有重複帳號存在
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                foreach (DataRow row in dt.Rows)
                {
                    Account accountInfo = new Account();
                    if (accountInfo.Check(row[1].ToString()) == true)
                    {
                        string errorMsg = $"帳號 {row[1]} 已存在";
                        HttpContext.Current.Response.Write("<script>alert('" + errorMsg + "');</script>");
                        return 1;
                    }
                }
                SaveToDatabase(dt, connectionString);
                return 0;
            }
        }

        static void SaveToDatabase(DataTable dt, string connectionString)
        {
            Account accountInfo= new Account();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                foreach (DataRow row in dt.Rows)
                {
                    Account parameters = new Account()
                    {
                        uName = row[0].ToString(),
                        uAccount = row[1].ToString(),
                        uPassword = row[2].ToString(),
                        uPhone = row[3].ToString(),
                        uGender = row[4].ToString()
                    };
                    accountInfo.Add(parameters);    
                }
            }
        }

        protected void Back_Click(object sender, EventArgs e)
        {
            Response.Redirect("home.aspx");
        }
        static bool IsValidPhoneNumber(string phoneNumber)
        {
            string pattern = @"^((\d{10})|(((\(\d{2}\))|(\d{2}-))?\d{4}(-)?\d{3}(\d)?))";
            return System.Text.RegularExpressions.Regex.IsMatch(phoneNumber, pattern);
        }
    }
}