using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace CourseWorkTP
{
    public partial class orders : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["isAdmin"] != null)
            {
                Response.Redirect("admin.aspx");
            }
            if (!IsPostBack)
            {
                if (Session["UserId"] == null)
                {
                    Response.Redirect("~/index.aspx");
                    return;
                }
                LoadOrders();
            }
        }
        private void LoadOrders()
        {
            string connectionString = "Server=localhost;Database=db_tp_cw;Trusted_Connection=True;";
            string query = @"SELECT 
                            o.id,
                            o.date,
                            o.status,
                            o.total,
                            o.address,
                            COUNT(oi.good_id) AS ItemsCount
                        FROM orders o
                        JOIN orderItems oi ON o.id = oi.order_id
                        WHERE o.user_id = @userId
                        GROUP BY o.id, o.date, o.status, o.total, o.address
                        ORDER BY o.date DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", Session["userId"]);
                    conn.Open();

                    DataTable dt = new DataTable();
                    dt.Load(cmd.ExecuteReader());

                    rptOrders.DataSource = dt;
                    rptOrders.DataBind();
                }
            }
        }
        protected void rptOrders_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                DataRowView row = (DataRowView)e.Item.DataItem;
                string status = row["status"].ToString();

                Label lblStatus = (Label)e.Item.FindControl("lblStatus");
                if (lblStatus != null)
                {
                    lblStatus.CssClass = "order-status " + GetStatusClass(status);
                }
            }
        }
        public string GetStatusClass(string status)
        {
            switch (status.ToLower())
            {
                case "Finished": return "status-completed";
                case "Processing": return "status-processing";
                case "Delivering": return "status-shipping";
                default: return "status-default";
            }
        }
        protected void LoadOrderDetails(int orderId)
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
        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            if (IsPostBack && Request["__EVENTTARGET"] == "LoadOrderDetails")
            {
                int orderId = Convert.ToInt32(Request["__EVENTARGUMENT"]);
                LoadOrderDetails(orderId);
                ScriptManager.RegisterStartupScript(this, GetType(), "showModal",
                    $"document.getElementById('{orderDetailsModal.ClientID}').style.display='block';", true);
            }
        }
    }
}