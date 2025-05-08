using System;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;

namespace CourseWorkTP
{
    public partial class UserProfile : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["isAdmin"] != null)
            {
                Response.Redirect("admin.aspx");
            }
            if (Session["userId"] == null)
            {
                Response.Redirect("index.aspx");
                return;
            }
            if (!IsPostBack)
            {
                ShowPersonalData();
            }
        }
        protected void ShowPersonalData()
        {
            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(strSqlConnection))
                {
                    sqlConnection.Open();

                    string query = "SELECT name, [last name], [middle name] FROM userData WHERE id = @user_id";

                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@user_id", Session["userId"]);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string name = reader["name"].ToString();
                                string lastName = reader["last name"].ToString();
                                string middleName = reader["middle name"].ToString();

                                txtFirstName.Text = name;
                                txtLastName.Text = lastName;
                                txtMiddleName.Text = middleName;
                            }
                            else
                            {
                                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Профиль не найден'
                                                        });", true);
                                return;
                            }
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Database error'
                                                        });", true);
                return;
            }
        }
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Abandon();
            Response.Redirect("index.aspx");
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            string firstName = txtFirstName.Text;
            string lastName = txtLastName.Text;
            string middleName = txtMiddleName.Text;

            if (string.IsNullOrWhiteSpace(firstName))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Имя не может быть пустым'
                                                        });", true);
                return;
            }

            if (firstName.Length > 50)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Имя силишком длинное'
                                                        });", true);
                return;
            }

            if (lastName.Length > 50)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Фамилия слишком длинная'
                                                        });", true);
                return;
            }

            if (middleName.Length > 50)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Отчество слишком длинное'
                                                        });", true);
                return;
            }

            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(strSqlConnection))
                {
                    sqlConnection.Open();

                    string updateQuery = @"UPDATE userData 
                                 SET name = @firstName,
                                     [last name] = @lastName, 
                                     [middle name] = @middleName 
                                 WHERE id = @userId";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@firstName", firstName.Trim());
                        cmd.Parameters.AddWithValue("@lastName", lastName.Trim());
                        cmd.Parameters.AddWithValue("@middleName", middleName.Trim());
                        cmd.Parameters.AddWithValue("@userId", Convert.ToInt32(Session["userId"]));

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'success',
                                                      title: 'Success',
                                                    text: 'Данные успешно обновлены'
                                                        });", true);
                            ShowPersonalData();
                            return;
                        }
                        else
                        {
                            ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Профиль не найден'
                                                        });", true);
                            return;
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Database error'
                                                        });", true);
                return;
            }
        }
        protected void btnOrders_Click(object sender, EventArgs e)
        {
            Response.Redirect("orders.aspx");
        }
        protected void btnCart_Click(object sender, EventArgs e)
        {
            Response.Redirect("cart.aspx");
        }
        protected void btnChangePassword_Click(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "showModal",
            $"document.getElementById('{NewPassModal.ClientID}').style.display='block';", true);
        }
        protected void btnChangePass_Click(object sender, EventArgs e)
        {
            string oldPass = OldPass.Text;
            string newPass = NewPass.Text;
            string newPass2 = NewPass2.Text;

            if (string.IsNullOrWhiteSpace(oldPass) || string.IsNullOrEmpty(newPass) || string.IsNullOrEmpty(newPass2))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Поля не могут быть пустыми'
                                                        });", true);
                return;
            }

            if (newPass != newPass2)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Пароли не совпадают'
                                                        });", true);
                return;
            }

            byte[] oldPassB = sha256_hash(oldPass);
            byte[] newPassB = sha256_hash(newPass);

            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(strSqlConnection))
            {
                try
                {
                    sqlConnection.Open();

                    string query = "SELECT * FROM dbo.users WHERE id = @id";
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@id", Convert.ToInt32(Session["userId"]));

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {

                                byte[] storedPassword = (byte[])reader["password"];
                                if (storedPassword.SequenceEqual(oldPassB) == false)
                                {
                                    ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Неверный старый пароль'
                                                        });", true);
                                    return;
                                }
                                reader.Close();

                                string updateQuery = "UPDATE users SET password = @newPass WHERE id = @id";

                                using (SqlCommand updateCmd = new SqlCommand(updateQuery, sqlConnection))
                                {
                                    updateCmd.Parameters.AddWithValue("@newPass", newPassB);
                                    updateCmd.Parameters.AddWithValue("@id", Convert.ToInt32(Session["userId"]));

                                    int rowsAffected = updateCmd.ExecuteNonQuery();

                                    if (rowsAffected > 0)
                                    {
                                        ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                                @"Swal.fire({
                                                      icon: 'success',
                                                      title: 'Success',
                                                    text: 'Пароль успешно изменен'
                                                        });", true);
                                        return;
                                    }
                                    else
                                    {
                                        ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                                @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Профиль не найден'
                                                        });", true);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                reader.Close();
                                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Пользователь не найден'
                                                        });", true);
                                return;
                            }
                        }
                    }
                }
                catch (SqlException ex)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Database error'
                                                        });", true);
                    return;
                }
            }

        }
        private byte[] sha256_hash(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return hash;
            }
        }
    }

}