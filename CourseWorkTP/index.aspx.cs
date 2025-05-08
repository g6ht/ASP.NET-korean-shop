using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace CourseWorkTP
{
    public partial class index : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["isAdmin"] == null)
            {
                if (!IsPostBack)
                {
                    BindCategoriesToDropDown();
                    BindAllGoodsAsCards();
                }

                string selectedCategory = DropDownList1.SelectedValue;
                if (selectedCategory == "")
                {
                    if (!string.IsNullOrEmpty(txtSearch.Text.Trim()))
                    {
                        // Если есть поисковый запрос, показываем результаты поиска
                        SearchProducts(txtSearch.Text.Trim());
                        DropDownList1.SelectedValue = "";
                    }
                    else
                    {
                        BindAllGoodsAsCards();
                    }
                }
                else
                {
                    FilterProductsByCategory(selectedCategory);
                }
            }
            else
            {
                Response.Redirect("admin.aspx");
            }
        }
        private void BindAllGoodsAsCards()
        {
            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            try
            {
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
                                    g.image_url,
                                    c.title AS category_name
                                    FROM dbo.goods g
                                    LEFT JOIN dbo.categories c ON g.category_id = c.id
                                    WHERE g.amount > 0
                                    ORDER BY g.title";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, sqlConnection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    HtmlGenericControl cardsContainer = new HtmlGenericControl("div");
                    cardsContainer.Attributes["class"] = "products-container";

                    foreach (DataRow row in dt.Rows)
                    {
                        HtmlGenericControl cardDiv = new HtmlGenericControl("div");
                        cardDiv.Attributes["class"] = "product-card";

                        // Изображение товара
                        HtmlGenericControl img = new HtmlGenericControl("img");
                        img.Attributes["src"] = !string.IsNullOrEmpty(row["image_url"].ToString())
                            ? ResolveUrl(row["image_url"].ToString())
                            : ResolveUrl("~/images/no-image.png");
                        img.Attributes["class"] = "product-image";
                        img.Attributes["alt"] = row["title"].ToString();
                        cardDiv.Controls.Add(img);

                        // Основной контент
                        HtmlGenericControl contentDiv = new HtmlGenericControl("div");
                        contentDiv.Attributes["class"] = "product-content";

                        // Название товара
                        HtmlGenericControl title = new HtmlGenericControl("div");
                        title.Attributes["class"] = "product-title";
                        title.InnerText = row["title"].ToString();
                        contentDiv.Controls.Add(title);

                        // Описание товара
                        HtmlGenericControl description = new HtmlGenericControl("div");
                        description.Attributes["class"] = "product-description";
                        description.InnerText = row["description"].ToString();
                        contentDiv.Controls.Add(description);

                        // Блок с ценой и кнопкой
                        HtmlGenericControl priceActionDiv = new HtmlGenericControl("div");
                        priceActionDiv.Attributes["class"] = "product-price-action";

                        // Цена товара
                        HtmlGenericControl price = new HtmlGenericControl("div");
                        price.Attributes["class"] = "product-price";
                        price.InnerText = $"{Convert.ToDecimal(row["price"]):C}";
                        priceActionDiv.Controls.Add(price);

                        // Кнопка "В корзину"
                        HtmlGenericControl actionsDiv = new HtmlGenericControl("div");
                        actionsDiv.Attributes["class"] = "product-actions";

                        Button addToCart = new Button();
                        addToCart.CssClass = "add-to-cart";
                        addToCart.Text = "Добавить в корзину";
                        addToCart.ID = "btnAdd_" + row["id"].ToString();
                        addToCart.CommandArgument = row["id"].ToString();
                        addToCart.Click += BtnAdd_Click;
                        actionsDiv.Controls.Add(addToCart);

                        priceActionDiv.Controls.Add(actionsDiv);
                        contentDiv.Controls.Add(priceActionDiv);
                        cardDiv.Controls.Add(contentDiv);
                        cardsContainer.Controls.Add(cardDiv);
                    }

                    GoodsContainer.Controls.Clear();
                    GoodsContainer.Controls.Add(cardsContainer);
                }
            }
            catch (SqlException ex)
            {
                Response.Write($"<script>alert('Database error: {ex.Message}');</script>");
            }
        }
        private void BindCategoriesToDropDown()
        {
            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(strSqlConnection))
                {
                    sqlConnection.Open();

                    string query = "SELECT title FROM dbo.categories ORDER BY title";
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        DropDownList1.Items.Clear(); // Очищаем список перед заполнением

                        // Добавляем элемент по умолчанию
                        DropDownList1.Items.Add(new ListItem("Выберите категорию", ""));

                        while (reader.Read())
                        {
                            string title = reader["title"].ToString();
                            DropDownList1.Items.Add(new ListItem(title, title));
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Response.Write($"<script>alert('Database error: {ex.Message}');</script>");
            }
        }
        protected void DropDownList1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Получаем выбранное значение
            string selectedCategory = DropDownList1.SelectedValue;
            if (selectedCategory == "")
            {
                BindAllGoodsAsCards();
            }
            else
            {
                // Фильтруем товары по выбранной категории
                FilterProductsByCategory(selectedCategory);
            }

        }
        private void FilterProductsByCategory(string category)
        {
            string connectionString = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT * FROM goods 
                                 WHERE category_id = (SELECT id FROM categories WHERE title = @Category) AND amount > 0
                                 ORDER BY title";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Category", category);

                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                // Создаем контейнер для карточек
                HtmlGenericControl cardsContainer = new HtmlGenericControl("div");
                cardsContainer.Attributes["class"] = "products-container";

                foreach (DataRow row in dt.Rows)
                {
                    // В методе BindAllGoodsAsCards() измените структуру карточки:
                    HtmlGenericControl cardDiv = new HtmlGenericControl("div");
                    cardDiv.Attributes["class"] = "product-card";

                    // Изображение (фиксированного размера)
                    HtmlGenericControl img = new HtmlGenericControl("img");
                    img.Attributes["src"] = !string.IsNullOrEmpty(row["image_url"].ToString()) ?
                        ResolveUrl(row["image_url"].ToString()) : ResolveUrl("~/images/no-image.png");
                    img.Attributes["class"] = "product-image";
                    img.Attributes["alt"] = row["title"].ToString();
                    cardDiv.Controls.Add(img);

                    // Основной контент
                    HtmlGenericControl contentDiv = new HtmlGenericControl("div");
                    contentDiv.Attributes["class"] = "product-content";

                    // Название товара
                    HtmlGenericControl title = new HtmlGenericControl("div");
                    title.Attributes["class"] = "product-title";
                    title.InnerText = row["title"].ToString();
                    contentDiv.Controls.Add(title);

                    // Описание товара
                    HtmlGenericControl description = new HtmlGenericControl("div");
                    description.Attributes["class"] = "product-description";
                    description.InnerText = row["description"].ToString();
                    contentDiv.Controls.Add(description);

                    // Блок с ценой и кнопкой
                    HtmlGenericControl priceActionDiv = new HtmlGenericControl("div");
                    priceActionDiv.Attributes["class"] = "product-price-action";

                    // Цена товара
                    HtmlGenericControl price = new HtmlGenericControl("div");
                    price.Attributes["class"] = "product-price";
                    price.InnerText = $"{Convert.ToDecimal(row["price"]):C}";
                    priceActionDiv.Controls.Add(price);

                    // Кнопка "В корзину"
                    HtmlGenericControl actionsDiv = new HtmlGenericControl("div");
                    actionsDiv.Attributes["class"] = "product-actions";

                    Button addToCart = new Button();
                    addToCart.CssClass = "add-to-cart";
                    addToCart.Text = "Добавить в корзину";
                    addToCart.ID = "btnAdd_" + row["id"].ToString();
                    addToCart.CommandArgument = row["id"].ToString();
                    addToCart.Click += BtnAdd_Click;
                    actionsDiv.Controls.Add(addToCart);

                    priceActionDiv.Controls.Add(actionsDiv);
                    contentDiv.Controls.Add(priceActionDiv);
                    cardDiv.Controls.Add(contentDiv);
                    cardsContainer.Controls.Add(cardDiv);
                }

                // Очищаем контейнер и добавляем новые элементы
                GoodsContainer.Controls.Clear();
                GoodsContainer.Controls.Add(cardsContainer);

            }
        }
        protected void ImageButton2_Clicked(object sender, EventArgs e)
        {
            if (Session["userId"] == null)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showModal",
            $"document.getElementById('{loginModal.ClientID}').style.display='block';", true);
            }
            else
            {
                Response.Redirect("UserProfile.aspx");
            }
        }
        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string email = loginEmail.Text;
            string password = loginPassword.Text;
            loginEmail.Text = "";
            loginPassword.Text = "";

            byte[] passw = sha256_hash(password);

            string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
            Match match = Regex.Match(email, pattern);
            if ((!match.Success || email.Length > 50) && email != "admin")
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                                    @"Swal.fire({
                                                                        icon: 'error',
                                                                        title: 'Error',
                                                                        text: 'Неверный формат почты'
                                                                    });", true);
                return;

            }
            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(strSqlConnection))
            {
                try
                {
                    sqlConnection.Open();

                    string query = "SELECT * FROM dbo.users WHERE email = @email";
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("email", email);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {

                                byte[] storedPassword = (byte[])reader["password"];
                                if (storedPassword.SequenceEqual(passw) == false)
                                {
                                    ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                                                        @"Swal.fire({
                                                                                            icon: 'error',
                                                                                            title: 'Error',
                                                                                            text: 'Неверный пароль'
                                                                                        });", true);
                                    return;
                                }
                                Session["userId"] = (int)reader["id"];
                                reader.Close();
                                if (email == "admin")
                                {
                                    Session["isAdmin"] = true;
                                    Response.Redirect("admin.aspx");
                                }
                                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                                                    @"Swal.fire({
                                                                                        icon: 'success',
                                                                                        title: 'Success',
                                                                                        text: 'Вход успешно выполнен'
                                                                                    });", true);
                                return;
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
        protected void btnRegister_Click(object sender, EventArgs e)
        {
            ScriptManager.RegisterStartupScript(this, GetType(), "showRegister",
            $"document.getElementById('{registerModal.ClientID}').style.display='block';" +
            $"document.getElementById('{loginModal.ClientID}').style.display='none';", true);
            registerEmail.Text = loginEmail.Text;
            loginEmail.Text = "";
            registerPassword.Text = loginPassword.Text;
            loginPassword.Text = "";
        }
        protected void btnRegister_Click2(object sender, EventArgs e)
        {
            string email = registerEmail.Text;
            string password = registerPassword.Text;
            string password2 = registerPassword2.Text;
            string name = registerName.Text;
            registerEmail.Text = "";
            registerPassword.Text = "";
            registerPassword2.Text = "";
            registerName.Text = "";

            if (password.Length == 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                                    @"Swal.fire({
                                                                        icon: 'error',
                                                                        title: 'Error',
                                                                        text: 'Пароль не должен быть пустым'
                                                                    });", true);
                return;
            }
            if (password != password2)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                                    @"Swal.fire({
                                                                        icon: 'error',
                                                                        title: 'Error',
                                                                        text: 'Пароли не совпадают'
                                                                    });", true);
                return;
            }

            if (name.Length == 0)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                                    @"Swal.fire({
                                                                        icon: 'error',
                                                                        title: 'Error',
                                                                        text: 'Введите имя'
                                                                    });", true);
                return;
            }

            string pattern = @"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$";
            Match match = Regex.Match(email, pattern);
            if ((!match.Success || email.Length > 50))
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                                    @"Swal.fire({
                                                                        icon: 'error',
                                                                        title: 'Error',
                                                                        text: 'Неверный формат почты'
                                                                    });", true);
                return;
            }

            byte[] passw = sha256_hash(password);

            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(strSqlConnection))
            {
                try
                {
                    sqlConnection.Open();

                    string query = "SELECT * FROM dbo.users WHERE email = @email";
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("email", email);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                                                    @"Swal.fire({
                                                                                        icon: 'error',
                                                                                        title: 'Error',
                                                                                        text: 'Пользователь с этой почтой уже существует'
                                                                                    });", true);
                                return;
                            }
                            else
                            {
                                reader.Close();

                                string query2 = "INSERT INTO users (email, password) VALUES (@email, @passw);" +
                                                "SELECT CAST(SCOPE_IDENTITY() AS INT);";
                                using (SqlCommand cmd2 = new SqlCommand(query2, sqlConnection))
                                {
                                    cmd2.Parameters.AddWithValue("@email", email);
                                    cmd2.Parameters.AddWithValue("@passw", passw);

                                    try
                                    {
                                        object result = cmd2.ExecuteScalar();

                                        if (result == null || result == DBNull.Value)
                                        {
                                            ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                                                                @"Swal.fire({
                                                                                                    icon: 'error',
                                                                                                    title: 'Error',
                                                                                                    text: 'Ошибка в создании пользователя'
                                                                                                });", true);
                                            return;
                                        }

                                        Session["userId"] = Convert.ToInt32(result);

                                        string query3 = "INSERT INTO userData (id, name) VALUES (@id, @name);";
                                        using (SqlCommand cmd3 = new SqlCommand(query3, sqlConnection))
                                        {
                                            cmd3.Parameters.AddWithValue("@id", ViewState["userId"]);
                                            cmd3.Parameters.AddWithValue("@name", name);

                                            int rowsAffected = cmd3.ExecuteNonQuery();

                                            if (rowsAffected > 0)
                                            {
                                                ScriptManager.RegisterStartupScript(this, GetType(), "showSuccess",
                                                                                                    @"Swal.fire({
                                                                                                        icon: 'success',
                                                                                                        title: 'Success',
                                                                                                        text: 'Аккаунт успешно создан'
                                                                                                    });", true);
                                                return;
                                            }
                                            else
                                            {
                                                ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                                                                    @"Swal.fire({
                                                                                                        icon: 'error',
                                                                                                        title: 'Error',
                                                                                                        text: 'Пользователь создан с ошибкой'
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
                                                                                                text: 'Ошибка с базой данных'
                                                                                            });", true);
                                        return;
                                    }
                                    catch (InvalidCastException ex)
                                    {
                                        ScriptManager.RegisterStartupScript(this, GetType(), "showError",
                                                                                            @"Swal.fire({
                                                                                                icon: 'error',
                                                                                                title: 'Error',
                                                                                                text: 'Error converting user ID'
                                                                                            });", true);
                                        return;
                                    }
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
        protected void ImageButton1_Clicked(object sender, EventArgs e)
        {
            if (Session["userId"] == null)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showModal",
                $"document.getElementById('{loginModal.ClientID}').style.display='block';", true);
                return;
            }
            Response.Redirect("cart.aspx");
        }
        protected void BtnAdd_Click(object sender, EventArgs e)
        {
            if (Session["userId"] == null)
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "showModal",
                $"document.getElementById('{loginModal.ClientID}').style.display='block';", true);
                return;
            }
            Button btn = (Button)sender;
            int itemId = Convert.ToInt32(btn.CommandArgument);
            if (UpdateCartItemAmount(itemId, 1))
            {
                string script = @"
                (function(){
                    const notification = document.createElement('div');
                    notification.className = 'cart-notification';
                    notification.innerHTML = '<i class=""fas fa-check-circle""></i> Товар добавлен в корзину!';
                    document.body.appendChild(notification);
                    setTimeout(() => notification.remove(), 1000);
                })();";

                ScriptManager.RegisterStartupScript(this, GetType(), "showNotif", script, true);
                GoodsUpdatePanel.Update();
            }

        }
        private bool UpdateCartItemAmount(int itemId, int change)
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
                                                                                    text: 'Товара нет на складе'
                                                                                });", true);
                            return false;
                        }
                        // 4. Если товара нет в корзине - INSERT, иначе - UPDATE
                        string query;
                        if (currentInCart == 0)
                        {
                            query = @"
                            INSERT INTO dbo.cart (user_id, good_id, amount)
                            VALUES (@user_id, @item_id, @change)";
                        }
                        else
                        {
                            query = @"
                            UPDATE dbo.cart 
                            SET amount = amount + @change 
                            WHERE good_id = @item_id AND user_id = @user_id";
                        }

                        using (SqlCommand cmd = new SqlCommand(query, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@change", change);
                            cmd.Parameters.AddWithValue("@item_id", itemId);
                            cmd.Parameters.AddWithValue("@user_id", Convert.ToInt32(Session["userId"]));
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        return false;
                    }
                }
            }
        }
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearch.Text.Trim();
            if (!string.IsNullOrEmpty(searchTerm))
            {
                SearchProducts(searchTerm);
            }
            else
            {
                // Если поисковая строка пустая, показываем все товары
                BindAllGoodsAsCards();
            }
            DropDownList1.SelectedValue = "";
        }
        private void SearchProducts(string searchTerm)
        {
            string strSqlConnection = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";

            try
            {
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
                                        g.image_url,
                                        c.title AS category_name
                                    FROM dbo.goods g
                                    LEFT JOIN dbo.categories c ON g.category_id = c.id
                                    WHERE (g.title LIKE @SearchTerm OR g.description LIKE @SearchTerm) AND g.amount > 0
                                    ORDER BY g.title";

                    SqlCommand cmd = new SqlCommand(query, sqlConnection);
                    cmd.Parameters.AddWithValue("@SearchTerm", $"%{searchTerm}%");

                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    HtmlGenericControl cardsContainer = new HtmlGenericControl("div");
                    cardsContainer.Attributes["class"] = "products-container";

                    if (dt.Rows.Count == 0)
                    {
                        // Если товары не найдены, показываем сообщение
                        HtmlGenericControl noResults = new HtmlGenericControl("div");
                        noResults.Attributes["class"] = "no-results";
                        noResults.InnerText = "No items found";
                        cardsContainer.Controls.Add(noResults);
                    }
                    else
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            HtmlGenericControl cardDiv = new HtmlGenericControl("div");
                            cardDiv.Attributes["class"] = "product-card";

                            // Изображение товара
                            HtmlGenericControl img = new HtmlGenericControl("img");
                            img.Attributes["src"] = !string.IsNullOrEmpty(row["image_url"].ToString())
                                ? ResolveUrl(row["image_url"].ToString())
                                : ResolveUrl("~/images/no-image.png");
                            img.Attributes["class"] = "product-image";
                            img.Attributes["alt"] = row["title"].ToString();
                            cardDiv.Controls.Add(img);

                            // Основной контент
                            HtmlGenericControl contentDiv = new HtmlGenericControl("div");
                            contentDiv.Attributes["class"] = "product-content";

                            // Название товара
                            HtmlGenericControl title = new HtmlGenericControl("div");
                            title.Attributes["class"] = "product-title";
                            title.InnerText = row["title"].ToString();
                            contentDiv.Controls.Add(title);

                            // Описание товара
                            HtmlGenericControl description = new HtmlGenericControl("div");
                            description.Attributes["class"] = "product-description";
                            description.InnerText = row["description"].ToString();
                            contentDiv.Controls.Add(description);

                            // Блок с ценой и кнопкой
                            HtmlGenericControl priceActionDiv = new HtmlGenericControl("div");
                            priceActionDiv.Attributes["class"] = "product-price-action";

                            // Цена товара
                            HtmlGenericControl price = new HtmlGenericControl("div");
                            price.Attributes["class"] = "product-price";
                            price.InnerText = $"{Convert.ToDecimal(row["price"]):C}";
                            priceActionDiv.Controls.Add(price);

                            // Кнопка "В корзину"
                            HtmlGenericControl actionsDiv = new HtmlGenericControl("div");
                            actionsDiv.Attributes["class"] = "product-actions";

                            Button addToCart = new Button();
                            addToCart.CssClass = "add-to-cart";
                            addToCart.Text = "Добавить в корзину";
                            addToCart.ID = "btnAdd_" + row["id"].ToString();
                            addToCart.CommandArgument = row["id"].ToString();
                            addToCart.Click += BtnAdd_Click;
                            actionsDiv.Controls.Add(addToCart);

                            priceActionDiv.Controls.Add(actionsDiv);
                            contentDiv.Controls.Add(priceActionDiv);
                            cardDiv.Controls.Add(contentDiv);
                            cardsContainer.Controls.Add(cardDiv);
                        }
                    }

                    GoodsContainer.Controls.Clear();
                    GoodsContainer.Controls.Add(cardsContainer);
                }
            }
            catch (SqlException ex)
            {
                Response.Write($"<script>alert('Database error: {ex.Message}');</script>");
            }
        }
    }
}