using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using Microsoft.Ajax.Utilities;
using System.Text;

namespace CourseWorkTP
{
    public partial class cart : System.Web.UI.Page
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
            LoadCartItems();
        }
        private void LoadCartItems()
        {
             // Получаем ID текущего пользователя
            int userId = Convert.ToInt32(Session["userId"]);
            GoodsContainer.Controls.Clear();
            string connectionString = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";
            string query = @"
                SELECT 
                    g.id,
                    g.image_url,
                    g.title,
                    g.price,
                    c.amount,
                    (g.price * c.amount) AS total_price
                FROM dbo.cart c
                JOIN dbo.goods g ON c.good_id = g.id
                WHERE c.user_id = @user_id
                ORDER BY g.title";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@user_id", userId);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                if (dt.Rows.Count == 0)
                {
                    lblEmptyCart.Visible = true;
                    cartSummary.Visible = false;
                    return;
                }

                decimal total = 0;
                HtmlGenericControl cardsContainer = new HtmlGenericControl("div");
                cardsContainer.Attributes["class"] = "cart-items";
                foreach (DataRow row in dt.Rows)
                {
                    total += Convert.ToDecimal(row["total_price"]);
                    CreateCartItem(row);

                }

                lblTotal.Text = total.ToString("N0") + " ₽";
                cartSummary.Visible = true;
            }

        }
        private void CreateCartItem(DataRow row)
        {
            HtmlGenericControl itemDiv = new HtmlGenericControl("div");
            itemDiv.Attributes["class"] = "cart-item";
            itemDiv.Attributes["data-id"] = row["id"].ToString();

            // Изображение товара
            HtmlGenericControl imgDiv = new HtmlGenericControl("div");
            imgDiv.Attributes["class"] = "cart-item-image";

            HtmlGenericControl img = new HtmlGenericControl("img");
            img.Attributes["src"] = !string.IsNullOrEmpty(row["image_url"].ToString()) ?
                ResolveUrl(row["image_url"].ToString()) : ResolveUrl("~/images/no-image.png");
            img.Attributes["alt"] = row["title"].ToString();

            imgDiv.Controls.Add(img);
            itemDiv.Controls.Add(imgDiv);

            // Информация о товаре
            HtmlGenericControl infoDiv = new HtmlGenericControl("div");
            infoDiv.Attributes["class"] = "cart-item-info";

            HtmlGenericControl title = new HtmlGenericControl("h3");
            title.InnerText = row["title"].ToString();
            infoDiv.Controls.Add(title);

            HtmlGenericControl price = new HtmlGenericControl("div");
            price.Attributes["class"] = "cart-item-price";
            price.InnerText = Convert.ToDecimal(row["price"]).ToString("N0") + " ₽";
            infoDiv.Controls.Add(price);

            itemDiv.Controls.Add(infoDiv);

            // Управление количеством
            HtmlGenericControl quantityDiv = new HtmlGenericControl("div");
            quantityDiv.Attributes["class"] = "cart-item-quantity";
            // Создаем элементы как в вашем коде, но с важными изменениями:

            Button btnDecrease = new Button();
            btnDecrease.ID = "btnDec_" + row["id"].ToString(); // Уникальный ID!
            btnDecrease.CssClass = "quantity-btn";
            btnDecrease.Text = "-";
            btnDecrease.CommandArgument = row["id"].ToString();
            btnDecrease.Click += BtnDecrease_Click;

            HtmlGenericControl amount = new HtmlGenericControl("span");
            amount.Attributes["class"] = "quantity-value";
            amount.InnerText = row["amount"].ToString();

            // Аналогично для других кнопок
            Button btnIncrease = new Button();
            btnIncrease.ID = "btnInc_" + row["id"].ToString();
            btnIncrease.CssClass = "quantity-btn";
            btnIncrease.Text = "+";
            btnIncrease.CommandArgument = row["id"].ToString();
            btnIncrease.Click += BtnIncrease_Click;

            quantityDiv.Controls.Add(btnDecrease);
            quantityDiv.Controls.Add(amount);
            quantityDiv.Controls.Add(btnIncrease);
            itemDiv.Controls.Add(quantityDiv);

            // Итоговая цена
            HtmlGenericControl totalDiv = new HtmlGenericControl("div");
            totalDiv.Attributes["class"] = "cart-item-total";
            totalDiv.InnerText = Convert.ToDecimal(row["total_price"]).ToString("N0") + " ₽";
            itemDiv.Controls.Add(totalDiv);

            // Кнопка удаления
            Button btnRemove = new Button();
            btnRemove.ID = "btnRem_" + row["id"].ToString();
            btnRemove.CssClass = "btn-remove";
            btnRemove.Text = "×";
            btnRemove.CommandArgument = row["id"].ToString();
            btnRemove.Click += BtnRemove_Click;

            itemDiv.Controls.Add(btnRemove);

            // Добавляем элементы в контейнер
            GoodsContainer.Controls.Add(itemDiv);
        }
        protected void BtnIncrease_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int itemId = Convert.ToInt32(btn.CommandArgument);
            UpdateCartItemAmount(itemId, 1);
            LoadCartItems();
        }
        protected void BtnDecrease_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int itemId = Convert.ToInt32(btn.CommandArgument);
            UpdateCartItemAmount(itemId, -1);
            LoadCartItems();
        }
        protected void BtnRemove_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            int itemId = Convert.ToInt32(btn.CommandArgument);
            RemoveCartItem(itemId);
            LoadCartItems();
        }
        private void UpdateCartItemAmount(int itemId, int change)
        {
            string connectionString = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Проверяем текущее количество в корзине
                        string getCurrentAmountQuery = @"
                    SELECT amount FROM dbo.cart 
                    WHERE good_id = @item_id AND user_id = @user_id";

                        int currentInCart = 0;
                        using (SqlCommand cmd = new SqlCommand(getCurrentAmountQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@item_id", itemId);
                            cmd.Parameters.AddWithValue("@user_id", Convert.ToInt32(Session["userId"]));
                            currentInCart = Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
                        }

                        // 2. Проверяем остатки на складе
                        string getStockQuery = "SELECT amount FROM dbo.goods WHERE id = @item_id";
                        int stockQuantity = 0;
                        using (SqlCommand cmd = new SqlCommand(getStockQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@item_id", itemId);
                            stockQuantity = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // 3. Проверяем, не превышаем ли доступное количество
                        if (change > 0 && (currentInCart + change) > stockQuantity)
                        {
                            ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Товара нет в наличии'
                                                        });", true);
                            return;
                        }

                        // 4. Обновляем корзину
                        string updateQuery = @"
                    UPDATE dbo.cart 
                    SET amount = amount + @change 
                    WHERE good_id = @item_id AND user_id = @user_id;
                    
                    DELETE FROM dbo.cart 
                    WHERE amount <= 0 AND good_id = @item_id AND user_id = @user_id;";

                        using (SqlCommand cmd = new SqlCommand(updateQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@change", change);
                            cmd.Parameters.AddWithValue("@item_id", itemId);
                            cmd.Parameters.AddWithValue("@user_id", Convert.ToInt32(Session["userId"]));
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                    }
                }
            }
        }
        private void RemoveCartItem(int itemId)
        {
            string connectionString = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";
            string query = "DELETE FROM dbo.cart WHERE good_id = @item_id AND user_id = @user_id";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@item_id", itemId);
                cmd.Parameters.AddWithValue("@user_id", Convert.ToInt32(Session["userId"]));

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        protected void btnCheckout_Click(object sender, EventArgs e)
        {
            // Реализация оформления заказа
 
            if (Session["userId"] == null)
            {
                Response.Redirect("index.aspx");
                return;
            }
            ScriptManager.RegisterStartupScript(this, GetType(), "showModal",
            $"document.getElementById('{orderModal.ClientID}').style.display='block';", true);
        }
        protected void btnOrder_Click(object sender, EventArgs e)
        {
            int userId = (int)Session["userId"];
            string address = Address.Text;
            if (string.IsNullOrWhiteSpace(address))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Адрес не может быть пустым'
                                                        });", true);
                return;
            }
            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";
            StringBuilder result = new StringBuilder();

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(strSqlConnection))
                {
                    sqlConnection.Open();
                    SqlTransaction transaction = sqlConnection.BeginTransaction();

                    try
                    {
                        // 1. Получаем содержимое корзины и считаем общую сумму
                        decimal totalAmount = 0;
                        var cartItems = new List<CartItem>();

                        string getCartQuery = @"
                    SELECT g.id, g.price, c.amount, g.amount as stock_amount 
                    FROM cart c 
                    JOIN goods g ON c.good_id = g.id 
                    WHERE c.user_id = @user_id";

                        using (var cmd = new SqlCommand(getCartQuery, sqlConnection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@user_id", userId);

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var item = new CartItem
                                    {
                                        GoodId = reader.GetInt32(0),
                                        Price = reader.GetInt32(1),
                                        Quantity = reader.GetInt32(2),
                                        StockAmount = reader.GetInt32(3)
                                    };

                                    // Проверка наличия товара на складе
                                    if (item.Quantity > item.StockAmount)
                                    {
                                        transaction.Rollback();
                                        ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Товара нет в наличии'
                                                        });", true);
                                        return;
                                    }

                                    cartItems.Add(item);
                                    totalAmount += item.Price * item.Quantity;
                                }
                            }
                        }

                        if (cartItems.Count == 0)
                        {
                            transaction.Rollback();
                            ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Корзина пуста'
                                                        });", true);
                            return;
                        }

                        // 2. Создаем запись заказа
                        string createOrderQuery = @"
                    INSERT INTO orders (user_id, date, total, status, address) 
                    VALUES (@user_id, @date, @total, 'Processing', @address);
                    SELECT SCOPE_IDENTITY();";

                        int orderId;
                        using (var cmd = new SqlCommand(createOrderQuery, sqlConnection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@user_id", userId);
                            cmd.Parameters.AddWithValue("@date", DateTime.Now);
                            cmd.Parameters.AddWithValue("@total", totalAmount);
                            cmd.Parameters.AddWithValue("@address", address);
                            orderId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        // 3. Добавляем товары в заказ и уменьшаем количество на складе
                        foreach (var item in cartItems)
                        {
                            // Добавляем в orderItems
                            string addItemQuery = @"
                        INSERT INTO orderItems (order_id, good_id, amount)
                        VALUES (@order_id, @good_id, @amount)";

                            using (var cmd = new SqlCommand(addItemQuery, sqlConnection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@order_id", orderId);
                                cmd.Parameters.AddWithValue("@good_id", item.GoodId);
                                cmd.Parameters.AddWithValue("@amount", item.Quantity);
                                cmd.ExecuteNonQuery();
                            }

                            // Уменьшаем количество на складе
                            string updateStockQuery = @"
                        UPDATE goods 
                        SET amount = amount - @amount 
                        WHERE id = @good_id";

                            using (var cmd = new SqlCommand(updateStockQuery, sqlConnection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@good_id", item.GoodId);
                                cmd.Parameters.AddWithValue("@amount", item.Quantity);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        // 4. Очищаем корзину
                        string clearCartQuery = "DELETE FROM cart WHERE user_id = @user_id";
                        using (var cmd = new SqlCommand(clearCartQuery, sqlConnection, transaction))
                        {
                            cmd.Parameters.AddWithValue("@user_id", userId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'success',
                                                      title: 'Success',
                                                    text: 'Заказ успешно создан'
                                                        });", true);
                        LoadCartItems();
                        return;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Ошибка при создании заказа'
                                                        });", true);
                        return;
                    }
                }
            }
            catch (SqlException ex)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                    @"Swal.fire({
                                                      icon: 'error',
                                                      title: 'Error',
                                                    text: 'Ошибка в базе данных'
                                                        });", true);
                return;
            }
        }
    }
    public class CartItem
    {
        public int GoodId { get; set; }
        public int Price { get; set; }
        public int Quantity { get; set; }
        public int StockAmount { get; set; }
    }
}