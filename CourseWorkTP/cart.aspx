<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="cart.aspx.cs" Inherits="CourseWorkTP.cart" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link href="cart.css" rel="stylesheet" />
    <title>Hot Pot Express</title>
    <link href="https://fonts.googleapis.com/css2?family=Bahnschrift&display=swap" rel="stylesheet">
</head>

<body>
    <form id="form1" runat="server">
        <!-- Добавляем ScriptManager в самое начало формы -->
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true"></asp:ScriptManager>
        
        <!-- Обертываем весь основной контент в UpdatePanel -->
        <asp:UpdatePanel ID="MainUpdatePanel" runat="server" UpdateMode="Conditional">
            <ContentTemplate>
                <div style="height: 130px; width: 100%; text-align: center; vertical-align: middle; display: block;">
                    <asp:ImageButton ID="ImageButton3" runat="server" CssClass="auto-style5" Height="107px" ImageUrl="~/images/logo.png" Width="443px" PostBackUrl="~/index.aspx" />
                </div>

                <div class="cart-container">
                    <div class="cart-header">
                        <h1>Корзина</h1>
                        <asp:Label ID="lblEmptyCart" runat="server" Text="Ваша корзина пуста" Visible="false" CssClass="empty-cart-message"></asp:Label>
                    </div>
                    
                    <asp:Panel ID="GoodsContainer" runat="server"></asp:Panel>
                    
                    <div class="cart-summary" id="cartSummary" runat="server" visible="false">
                        <div class="summary-row">
                            <span>Итого:</span>
                            <asp:Label ID="lblTotal" runat="server" CssClass="total-price"></asp:Label>
                        </div>
                        <asp:Button ID="btnCheckout" runat="server" Text="Сделать заказ" CssClass="btn-checkout" OnClick="btnCheckout_Click" />
                    </div>
                </div>
            </ContentTemplate>
        </asp:UpdatePanel>


                            <div id="orderModal" runat="server" class="modal" style="display:none;">
    <div class="modal-content">
        <span class="close" onclick="closePassModal()">&times;</span>
        <h2>Создание заказа</h2>
        <div class="form-group2">
            <label for="OldPass">Адрес доставки:</label>
            <asp:TextBox ID="Address" runat="server" CssClass="form-control2"></asp:TextBox>
        </div>
        
        
        <div class="buttons-container">
        <asp:Button ID="btnConfirm" runat="server" Text="Заказать" CssClass="btn-order" OnClick="btnOrder_Click" />
             </div>
    </div>
</div>

    <script>

function closePassModal() {
    document.getElementById('<%= orderModal.ClientID %>').style.display = 'none';
    }
  
    </script>

    </form>
</body>
</html>
