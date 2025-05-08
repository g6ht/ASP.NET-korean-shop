<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="orders.aspx.cs" Inherits="CourseWorkTP.orders" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link href="orders.css" rel="stylesheet" />
    <title>Hot Pot Express</title>
    <link href="https://fonts.googleapis.com/css2?family=Bahnschrift&display=swap" rel="stylesheet">
</head>
<body>
    <form id="form1" runat="server">
                <div style="height: 130px; width: 100%; text-align: center; vertical-align: middle; display: block;">
                <asp:ImageButton ID="ImageButton3" runat="server" CssClass="auto-style5" Height="107px" ImageUrl="~/images/logo.png" Width="443px" PostBackUrl="~/index.aspx" />
                </div>

        <div class="orders-container">
            <h1>Мои заказы</h1>
            
            <asp:Repeater ID="rptOrders" runat="server" OnItemDataBound="rptOrders_ItemDataBound">
                <ItemTemplate>
                    <div class="order-card" onclick='showOrderDetails(<%# Eval("id") %>)'>
                        <div class="order-header">
                            <span class="order-date"><%# Eval("date", "{0:dd.MM.yyyy HH:mm}") %></span>
                            <span class="order-status <%# GetStatusClass(Eval("status").ToString()) %>">
                                <%# Eval("status") %>
                            </span>
                        </div>
                        <div class="order-body">
                            <div class="order-info">
                                <span><i class="fas fa-box-open"></i> <%# Eval("ItemsCount") %> товаров</span>
                                <span><i class="fas fa-map-marker-alt"></i> <%# Eval("address") %></span>
                            </div>
                            <div class="order-total">
                                <strong><%# Eval("total", "{0:C}") %></strong>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>

        <!-- Модальное окно с деталями заказа -->
        <div id="orderDetailsModal" class="modal" runat="server">
            <div class="modal-content">
                <span class="close" onclick="closeModal()">&times;</span>
                <h2>Детали заказа <span id="orderIdHeader" runat="server"></span></h2>
                <div class="order-details">
                    <div class="details-row">
                        <span>Дата:</span>
                        <span id="orderDateDetail" runat="server"></span>
                    </div>
                    <div class="details-row">
                        <span>Статус:</span>
                        <span id="orderStatusDetail" runat="server"></span>
                    </div>
                    <div class="details-row">
                        <span>Адрес:</span>
                        <span id="orderAddressDetail" runat="server"></span>
                    </div>
                    
                    <h3>Товары:</h3>
                    <asp:Repeater ID="rptOrderItems" runat="server">
                        <HeaderTemplate>
                            <div class="order-items-header">
                                <span>Название</span>
                                <span>Количество</span>
                                <span>Цена</span>
                                <span>Итого</span>
                            </div>
                        </HeaderTemplate>
                        <ItemTemplate>
                            <div class="order-item">
                                <span class="item-name"><%# Eval("ProductName") %></span>
                                <span class="item-quantity"><%# Eval("Quantity") %></span>
                                <span class="item-price"><%# Eval("Price", "{0:C}") %></span>
                                <span class="item-total"><%# (Convert.ToDecimal(Eval("Price")) * Convert.ToInt32(Eval("Quantity"))).ToString("C") %></span>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                    
                    <div class="order-summary">
                        <span>Общая сумма:</span>
                        <span id="orderTotalDetail" runat="server"></span>
                    </div>
                </div>
            </div>
        </div>
        
    </form>

    <script>
        function showOrderDetails(orderId) {
            __doPostBack('LoadOrderDetails', orderId);
        }

        function closeModal() {
            document.getElementById('<%= orderDetailsModal.ClientID %>').style.display = 'none';
        }

        // Закрытие при клике вне окна
        window.onclick = function(event) {
            var modal = document.getElementById('<%= orderDetailsModal.ClientID %>');
            if (event.target == modal) {
                closeModal();
            }
        }
    </script>

</body>
</html>
