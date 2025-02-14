using Dapper;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DB2
{
    public class Account
    {
        public string uName { get; set; }
        public string uAccount { get; set; }
        public string uPassword { get; set; }
        public string uPhone { get; set; }
        public string uGender { get; set; }

        string connStr = @"Server=DESKTOP-T5DFIIB; Database = test; Integrated Security = True; ";

        public Account() { }
        public IEnumerable<Account> GetAccount()
        {
            using (var conn = new SqlConnection(connStr))
            {
                return conn.Query<Account>("SELECT * FROM Account");
            }
        }
        public Account GetuAccount(string uAccount)//binding user
        {
            using (var conn = new SqlConnection(connStr))
            {
                string query = "SELECT * FROM Account WHERE uAccount = @uAccount";
                return conn.QueryFirstOrDefault<Account>(query, new { uAccount });
            }
        }
        public void Add(Account account)
        {
            using (var conn = new SqlConnection(connStr))
            {
                var InsertQuery = @"INSERT INTO account (uName, uAccount, uPassword,uPhone,uGender) 
                VALUES (@uname, @uaccount, @upassword,@uphone,@ugender)";
                conn.Execute(InsertQuery, account);  
            }
        }
        public void Delete(string account)
        {
            using (var conn = new SqlConnection(connStr))
            {
                string DeleteQuery = "DELETE FROM account WHERE uAccount = @uAccount";
                conn.Execute(DeleteQuery, new { uAccount = account });
            }
        }
        public IEnumerable<Account> Search(Account accout)//一次傳入account裡面的參數
        {
            string cmd = string.Empty;
            if (!string.IsNullOrEmpty(accout.uName))//條件
                cmd += @"uName LIKE '%'+@uName+'%' ";
            else if(!string.IsNullOrEmpty(accout.uAccount))
                cmd += @"uAccount LIKE '%'+@uAccount+'%' ";
            using (var conn = new SqlConnection(connStr))
            {
                string query = $"SELECT * FROM account WHERE {cmd}";
                return conn.Query<Account>(query, accout);
            }
        }
        //public IEnumerable<Account> SearchByAccount(string searchText="")
        //{
        //    using (var conn = new SqlConnection(connStr))
        //    {
        //        string query = "SELECT * FROM account WHERE uAccount LIKE @searchText";
        //        return conn.Query<Account>(query, new { searchText = "%" + searchText + "%" });
        //    }
        //}
        public void Update(Account account)
        {
            using (var conn = new SqlConnection(connStr))
            {
                string UpdateQuery = "UPDATE account SET uName = @uName, uPassword=@uPassword,  uPhone = @uPhone, uGender = @uGender WHERE uAccount = @uAccount";
                conn.Execute(UpdateQuery, account);
            }
        }

        public bool Check(string account)
        {
            using (var conn = new SqlConnection(connStr))
            {
                string CheckQuery = "SELECT COUNT(*) FROM account WHERE uAccount = @account";
                int count = conn.ExecuteScalar<int>(CheckQuery, new { account });
                return count > 0;
            }
        }

        public bool uLogin(string account,string password)//user login
        {
            using (var conn = new SqlConnection(connStr))
            {
                string query = "SELECT COUNT(*) FROM account WHERE uAccount = @account AND uPassword = @password";
                int count = conn.ExecuteScalar<int>(query, new { account,password });
                return count > 0;
            }
        }
        public bool cLogin(string account, string password)//admini login
        {
            using (var conn = new SqlConnection(connStr))
            {
                string query = "SELECT COUNT(*) FROM Caccount WHERE cAccount = @account AND cPassword = @password";
                int count = conn.ExecuteScalar<char>(query, new { account, password });
                return count > 0;
            }
        }

        public bool Login(Account account,string type)
        {
            string cmd = string.Empty;
            if (type == "admini")
            {  
                cmd += @"Caccount WHERE cAccount = @account AND cPassword = @password";
            }
            else if (type == "user")
            {
                cmd += @"account WHERE uAccount = @account AND uPassword = @password";
            }
            using (var conn = new SqlConnection(connStr))
            {
                string query = $"SELECT COUNT(*) FROM {cmd}";
                int count = conn.ExecuteScalar<int>(query, new { account=account.uAccount, password=account.uPassword });
                return count > 0;
            }
        }
    }
}
