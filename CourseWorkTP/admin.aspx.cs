using System;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CourseWorkTP
{
    public partial class admin : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["isAdmin"] == null)
            {
                Response.Redirect("index.aspx");
            }
            if (!IsPostBack)
            {
                BindGoodsGrid();
            }
        }
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            Session.Abandon();
            Response.Redirect("index.aspx");
        }
        protected void BindGoodsGrid()
        {
            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(strSqlConnection))
            {
                sqlConnection.Open();

                string query = @"
            SELECT 
                g.id,
                g.title,
                g.description,
                g.amount,
                g.price,
                c.title AS category_name
            FROM dbo.goods g
            LEFT JOIN dbo.categories c ON g.category_id = c.id
            ORDER BY g.title";

                SqlDataAdapter adapter = new SqlDataAdapter(query, sqlConnection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                GridView1.DataSource = dt;
                GridView1.DataBind();
            }
        }
        protected string TruncateDescription(string desc)
        {
            if (desc.Length > 50)
                return desc.Substring(0, 47) + "...";
            return desc;
        }
        protected void GridView1_RowEditing(object sender, GridViewEditEventArgs e)
        {
            GridView1.EditIndex = e.NewEditIndex;
            BindGoodsGrid();
            UpdatePanel1.Update();
        }
        protected void GridView1_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            int productID = Convert.ToInt32(GridView1.DataKeys[e.RowIndex].Value);
            DeleteProduct(productID);
            BindGoodsGrid();
            UpdatePanel1.Update();
        }
        protected void DeleteProduct(int productID)
        {
            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(strSqlConnection))
            {
                sqlConnection.Open();

                // Можно добавить проверку на существование заказов для этого товара
                string checkOrdersQuery = "SELECT COUNT(*) FROM dbo.orderItems WHERE good_id = @Id";
                SqlCommand checkCmd = new SqlCommand(checkOrdersQuery, sqlConnection);
                checkCmd.Parameters.AddWithValue("@Id", productID);
                int ordersCount = (int)checkCmd.ExecuteScalar();

                if (ordersCount > 0)
                {
                    ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                            $"alert('Невозможно удалить товар, так как он присутствует в заказах.');", true);
                    return;
                }

                string deleteQuery = "DELETE FROM dbo.goods WHERE id = @Id";
                SqlCommand cmd = new SqlCommand(deleteQuery, sqlConnection);
                cmd.Parameters.AddWithValue("@Id", productID);
                cmd.ExecuteNonQuery();
            }
        }
        protected void GridView1_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            // Получаем данные из формы редактирования
            string id = GridView1.DataKeys[e.RowIndex].Value.ToString();
            string title = ((TextBox)GridView1.Rows[e.RowIndex].FindControl("txtTitle")).Text;
            string description = ((TextBox)GridView1.Rows[e.RowIndex].FindControl("txtDesc")).Text;
            string amount = ((TextBox)GridView1.Rows[e.RowIndex].FindControl("txtAmount")).Text;
            string price = ((TextBox)GridView1.Rows[e.RowIndex].FindControl("txtPrice")).Text;
            string categoryName = ((TextBox)GridView1.Rows[e.RowIndex].FindControl("txtCategory")).Text;

            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(strSqlConnection))
            {
                sqlConnection.Open();

                // Начинаем транзакцию для обеспечения целостности данных
                using (SqlTransaction transaction = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        int categoryId;

                        // 1. Проверяем существование категории
                        string checkCategoryQuery = "SELECT id FROM dbo.categories WHERE title = @Title";
                        SqlCommand checkCmd = new SqlCommand(checkCategoryQuery, sqlConnection, transaction);
                        checkCmd.Parameters.AddWithValue("@Title", categoryName);

                        object result = checkCmd.ExecuteScalar();

                        if (result == null)
                        {
                            // 2. Если категории нет - создаем новую
                            string insertCategoryQuery = "INSERT INTO dbo.categories (title) OUTPUT INSERTED.id VALUES (@Title)";
                            SqlCommand insertCmd = new SqlCommand(insertCategoryQuery, sqlConnection, transaction);
                            insertCmd.Parameters.AddWithValue("@Title", categoryName);

                            categoryId = (int)insertCmd.ExecuteScalar();
                        }
                        else
                        {
                            categoryId = (int)result;
                        }

                        // 3. Обновляем данные товара
                        string updateProductQuery = @"
                    UPDATE dbo.goods 
                    SET title = @Title, 
                        description = @Description, 
                        amount = @Amount, 
                        price = @Price,
                        category_id = @CategoryId
                    WHERE id = @Id";

                        SqlCommand updateCmd = new SqlCommand(updateProductQuery, sqlConnection, transaction);
                        updateCmd.Parameters.AddWithValue("@Title", title);
                        updateCmd.Parameters.AddWithValue("@Description", description);
                        updateCmd.Parameters.AddWithValue("@Amount", Convert.ToInt32(amount));
                        updateCmd.Parameters.AddWithValue("@Price", Convert.ToInt32(price));
                        updateCmd.Parameters.AddWithValue("@CategoryId", categoryId);
                        updateCmd.Parameters.AddWithValue("@Id", Convert.ToInt32(id));

                        updateCmd.ExecuteNonQuery();

                        // Фиксируем транзакцию
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        // Откатываем транзакцию при ошибке
                        transaction.Rollback();

                        // Показываем сообщение об ошибке
                        ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                            $"alert('Не удалось обновить товар: {ex.Message}');", true);
                        return;
                    }
                }
            }

            GridView1.EditIndex = -1;
            BindGoodsGrid();
            UpdatePanel1.Update();
        }
        protected void GridView1_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridView1.EditIndex = -1;
            BindGoodsGrid();
            UpdatePanel1.Update();
        }
        protected void Menu1_MenuItemClick(object sender, MenuEventArgs e)
        {
            // Устанавливаем активную вкладку
            MultiView1.ActiveViewIndex = Int32.Parse(e.Item.Value);

            // Загружаем данные для соответствующей таблицы
            switch (e.Item.Value)
            {
                case "0": 
                    BindGoodsGrid();
                    break;
                case "1": 
                    BindCategoriesGrid();
                    break;
                case "2": 
                    BindOrdersGrid();
                    break;
            }
        }
        protected void btnAddProduct_Click(object sender, EventArgs e)
        {
            string title = txtNewTitle.Text.Trim();
            string description = txtNewDesc.Text.Trim();
            string categoryName = txtNewCat.Text.Trim();
            string amount = txtNewAmount.Text.Trim();
            string price = txtNewPrice.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) || string.IsNullOrEmpty(categoryName)
                || string.IsNullOrEmpty(amount) || string.IsNullOrEmpty(price))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                            $"alert('Ошибка: все поля должны быть заполнены');", true);
                return;
            }
            if (Convert.ToInt32(amount) < 0 || Convert.ToInt32(price) <= 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                            $"alert('Ошибка: количество и цена должны быть положительными числами');", true);
                return;
            }

            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(strSqlConnection))
            {
                sqlConnection.Open();

                using (SqlTransaction transaction = sqlConnection.BeginTransaction())
                {
                    try
                    {
                        int? categoryId = null;

                        // Обработка категории (если указана)
                        if (!string.IsNullOrEmpty(categoryName))
                        {
                            // Проверяем существование категории
                            string checkCategoryQuery = "SELECT id FROM dbo.categories WHERE title = @Title";
                            SqlCommand checkCmd = new SqlCommand(checkCategoryQuery, sqlConnection, transaction);
                            checkCmd.Parameters.AddWithValue("@Title", categoryName);

                            object result = checkCmd.ExecuteScalar();

                            if (result == null)
                            {
                                // Создаем новую категорию
                                string insertCategoryQuery = "INSERT INTO dbo.categories (title) OUTPUT INSERTED.id VALUES (@Title)";
                                SqlCommand insertCmd = new SqlCommand(insertCategoryQuery, sqlConnection, transaction);
                                insertCmd.Parameters.AddWithValue("@Title", categoryName);

                                categoryId = (int)insertCmd.ExecuteScalar();
                            }
                            else
                            {
                                categoryId = (int)result;
                            }
                        }


                        // Добавляем новый товар
                        string insertProductQuery = @"
                        INSERT INTO dbo.goods 
                            (title, description, amount, price, category_id) 
                        VALUES 
                            (@Title, @Description, @Amount, @Price, @CategoryId)";

                        SqlCommand insertProductCmd = new SqlCommand(insertProductQuery, sqlConnection, transaction);
                        insertProductCmd.Parameters.AddWithValue("@Title", title);
                        insertProductCmd.Parameters.AddWithValue("@Description", description);
                        insertProductCmd.Parameters.AddWithValue("@Amount", Convert.ToInt32(amount));
                        insertProductCmd.Parameters.AddWithValue("@Price", Convert.ToDecimal(price));

                        if (categoryId.HasValue)
                            insertProductCmd.Parameters.AddWithValue("@CategoryId", categoryId.Value);
                        else
                            insertProductCmd.Parameters.AddWithValue("@CategoryId", DBNull.Value);

                        insertProductCmd.ExecuteNonQuery();

                        transaction.Commit();

                        // Очищаем форму
                        txtNewTitle.Text = "";
                        txtNewDesc.Text = "";
                        txtNewCat.Text = "";
                        txtNewAmount.Text = "";
                        txtNewPrice.Text = "";

                        // Обновляем GridView
                        BindGoodsGrid();

                        // Показываем сообщение об успехе
                        ScriptManager.RegisterStartupScript(this, GetType(), "showSuccess",
                            "alert('Товар успешно добавлен!');", true);
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                            $"alert('Ошибка при добавлении товара: {ex.Message}');", true);
                    }
                }
            }
            UpdatePanel1.Update();
        }
        protected void BindCategoriesGrid()
        {
            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(strSqlConnection))
            {
                sqlConnection.Open();

                string query = "SELECT id, title FROM dbo.categories ORDER BY id";

                SqlDataAdapter adapter = new SqlDataAdapter(query, sqlConnection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                GridViewCategories.DataSource = dt;
                GridViewCategories.DataBind();
            }
        }
        protected void GridViewCategories_RowEditing(object sender, GridViewEditEventArgs e)
        {
            GridViewCategories.EditIndex = e.NewEditIndex;
            BindCategoriesGrid();
            UpdatePanel1.Update();
        }
        protected void GridViewCategories_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            string id = GridViewCategories.DataKeys[e.RowIndex].Value.ToString();
            string newName = ((TextBox)GridViewCategories.Rows[e.RowIndex].FindControl("txtCategoryName")).Text;

            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(strSqlConnection))
            {
                sqlConnection.Open();

                // Проверяем существование категории с таким именем (кроме текущей)
                string checkQuery = "SELECT COUNT(*) FROM dbo.categories WHERE title = @Title AND id <> @Id";
                SqlCommand checkCmd = new SqlCommand(checkQuery, sqlConnection);
                checkCmd.Parameters.AddWithValue("@Title", newName);
                checkCmd.Parameters.AddWithValue("@Id", Convert.ToInt32(id));
                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    // Категория с таким именем уже существует
                    ScriptManager.RegisterStartupScript(this, GetType(), "showAlert",
                        "alert('Категория с таким именем уже существует');", true);
                    return;
                }

                // Обновляем категорию
                string updateQuery = "UPDATE dbo.categories SET title = @Title WHERE id = @Id";
                SqlCommand updateCmd = new SqlCommand(updateQuery, sqlConnection);
                updateCmd.Parameters.AddWithValue("@Title", newName);
                updateCmd.Parameters.AddWithValue("@Id", Convert.ToInt32(id));
                updateCmd.ExecuteNonQuery();
            }

            GridViewCategories.EditIndex = -1;
            BindCategoriesGrid();
            UpdatePanel1.Update();

        }
        protected void GridViewCategories_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridViewCategories.EditIndex = -1;
            BindCategoriesGrid();
            UpdatePanel1.Update();
        }
        protected void btnAddCategory_Click(object sender, EventArgs e)
        {
            string newCategoryName = txtNewCategory.Text.Trim();

            if (!string.IsNullOrEmpty(newCategoryName))
            {
                string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

                using (SqlConnection sqlConnection = new SqlConnection(strSqlConnection))
                {
                    sqlConnection.Open();

                    // Проверяем существование категории
                    string checkQuery = "SELECT COUNT(*) FROM dbo.categories WHERE title = @Title";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, sqlConnection);
                    checkCmd.Parameters.AddWithValue("@Title", newCategoryName);
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count > 0)
                    {
                        // Категория уже существует
                        ScriptManager.RegisterStartupScript(this, GetType(), "showAlert",
                            "alert('Категория уже существует');", true);
                        return;
                    }

                    // Добавляем новую категорию
                    string insertQuery = "INSERT INTO dbo.categories (title) VALUES (@Title)";
                    SqlCommand insertCmd = new SqlCommand(insertQuery, sqlConnection);
                    insertCmd.Parameters.AddWithValue("@Title", newCategoryName);
                    insertCmd.ExecuteNonQuery();
                }

                txtNewCategory.Text = "";
                BindCategoriesGrid();
                UpdatePanel1.Update();
            }
        }
        protected void GridView2_RowEditing(object sender, GridViewEditEventArgs e)
        {
            GridView2.EditIndex = e.NewEditIndex;
            BindOrdersGrid();

            // Проверяем, что индекс существует
            if (e.NewEditIndex >= 0 && e.NewEditIndex < GridView2.Rows.Count)
            {
                // Находим DropDownList
                DropDownList ddlStatus = GridView2.Rows[e.NewEditIndex].FindControl("ddlStatus") as DropDownList;

                // Проверяем, что контрол найден и DataKey существует
                if (ddlStatus != null && GridView2.DataKeys[e.NewEditIndex] != null)
                {
                    // Безопасно получаем значение статуса
                    string currentStatus = GridView2.DataKeys[e.NewEditIndex].Values["status"]?.ToString();

                    if (!string.IsNullOrEmpty(currentStatus))
                    {
                        ddlStatus.SelectedValue = currentStatus;
                    }
                }
            }
            UpdatePanel3.Update();
        }   
        protected void GridView2_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            string orderId = GridView2.DataKeys[e.RowIndex].Value.ToString();
            DropDownList ddlStatus = (DropDownList)GridView2.Rows[e.RowIndex].FindControl("ddlStatus");
            string newStatus = ddlStatus.SelectedValue;

            // Обновляем статус заказа
            string result = ChangeOrderStatus(orderId, newStatus);

            GridView2.EditIndex = -1;
            BindOrdersGrid();
            UpdatePanel3.Update();

            if (!result.StartsWith("Order status changed"))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                    $"alert('{result}');", true);
            }
        }
        public string ChangeOrderStatus(string orderId, string newStatus)
        {
            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            try
            {
                // Проверяем валидность good_id
                if (!int.TryParse(orderId, out int orderId1))
                {
                    return "Неверный формат идентификатора";
                }
                if (newStatus != "Processing" && newStatus != "Delivering" && newStatus != "Finished")
                {
                    return "Неверный статус";
                }

                using (SqlConnection sqlConnection = new SqlConnection(strSqlConnection))
                {
                    sqlConnection.Open();
                    SqlTransaction transaction = sqlConnection.BeginTransaction();

                    try
                    {
                        string checkGoodQuery = "SELECT * FROM orders WHERE id = @id";

                        using (SqlCommand checkCmd = new SqlCommand(checkGoodQuery, sqlConnection, transaction))
                        {
                            checkCmd.Parameters.AddWithValue("@id", orderId1);

                            using (SqlDataReader reader = checkCmd.ExecuteReader())
                            {
                                if (!reader.HasRows)
                                {
                                    reader.Close();
                                    transaction.Rollback();
                                    return "Заказ не найден";
                                }
                                reader.Close();
                                string updateQuery = "UPDATE orders SET status = @newStatus WHERE id = @id";
                                using (SqlCommand updateCmd = new SqlCommand(updateQuery, sqlConnection, transaction))
                                {
                                    updateCmd.Parameters.AddWithValue("@id", orderId1);
                                    updateCmd.Parameters.AddWithValue("@newStatus", newStatus);
                                    updateCmd.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                        return "Order status changed successfully";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        return $"Ошибка при изменении заказа: {ex.Message}";
                    }
                }
            }
            catch (SqlException ex)
            {
                return $"Database error: {ex.Message}";
            }
        }
        protected void GridView2_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            GridView2.EditIndex = -1;
            BindOrdersGrid();
            UpdatePanel3.Update();
        }
        protected void GridView2_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ShowModal")
            {
                string orderId = e.CommandArgument.ToString();
                ShowOrderDetails(orderId);
            }
        }
        private void ShowOrderDetails(string orderId)
        {
            // Получаем детали заказа
            
            GetOrderDetails(orderId);
            ScriptManager.RegisterStartupScript(this, GetType(), "showModal",
                $"document.getElementById('{orderDetailsModal.ClientID}').style.display='block';", true);
        }
        private void GetOrderDetails(string orderId)
        {
            string connectionString = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            // Загрузка основной информации о заказе
            string orderQuery = @"SELECT 
                            o.id AS OrderId, 
                            o.date AS OrderDate, 
                            o.status AS Status, 
                            o.address AS DeliveryAddress,
                            SUM(g.price * oi.amount) AS TotalAmount
                         FROM orders o
                         JOIN orderItems oi ON o.id = oi.order_id
                         JOIN goods g ON oi.good_id = g.id
                         WHERE o.id = @orderId
                         GROUP BY o.id, o.date, o.status, o.address";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(orderQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            orderIdHeader.InnerText = "#" + reader["OrderId"].ToString();
                            orderDateDetail.InnerText = Convert.ToDateTime(reader["OrderDate"]).ToString("dd.MM.yyyy HH:mm");
                            orderStatusDetail.InnerText = reader["Status"].ToString();
                            orderAddressDetail.InnerText = reader["DeliveryAddress"].ToString();
                            orderTotalDetail.InnerText = Convert.ToDecimal(reader["TotalAmount"]).ToString("C");
                        }
                    }
                }

                // Загрузка товаров в заказе
                string itemsQuery = @"SELECT 
                                g.title AS ProductName, 
                                oi.amount AS Quantity, 
                                g.price AS Price,
                                (g.price * oi.amount) AS Total
                             FROM orderItems oi
                             JOIN goods g ON oi.good_id = g.id
                             WHERE oi.order_id = @orderId";

                using (SqlCommand cmd = new SqlCommand(itemsQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@orderId", orderId);

                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    rptOrderItems.DataSource = dt;
                    rptOrderItems.DataBind();
                }
            }
        }
        private void BindOrdersGrid()
        {
            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(strSqlConnection))
            {
                sqlConnection.Open();

                string query = @"
SELECT 
    o.id,
    o.date,
    o.total,
    o.status,
    o.address,
    u.email
FROM orders o
JOIN users u ON o.user_id = u.id
ORDER BY o.date DESC";

                SqlDataAdapter adapter = new SqlDataAdapter(query, sqlConnection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                GridView2.DataSource = dt;
                GridView2.DataBind();
            }
        }
    }
    }