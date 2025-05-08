<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="UserProfile.aspx.cs" Inherits="CourseWorkTP.UserProfile" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link href="profile.css" rel="stylesheet" />
    <title>Hot Pot Express</title>
    <link href="https://fonts.googleapis.com/css2?family=Bahnschrift&display=swap" rel="stylesheet">
</head>
<body>
    <form id="form1" runat="server">
                <div style="height: 130px; width: 100%; text-align: center; vertical-align: middle; display: block;">
                <asp:ImageButton ID="ImageButton3" runat="server" CssClass="auto-style5" Height="107px" ImageUrl="~/images/logo.png" Width="443px" PostBackUrl="~/index.aspx" />
                </div>

        <div class="profile-container">
            <!-- Шапка профиля -->
            <div class="profile-header">
                <h1>Профиль</h1>
                <asp:Button ID="btnLogout" runat="server" Text="Выход" CssClass="btn-logout" OnClick="btnLogout_Click" />
            </div>
            
            <!-- Основная информация -->
            <div class="profile-section">
                <h2>Информация о пользователе</h2>
                <div class="form-group">
                    <label>Имя:</label>
                    <asp:TextBox ID="txtFirstName" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Фамилия:</label>
                    <asp:TextBox ID="txtLastName" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label>Отчество:</label>
                    <asp:TextBox ID="txtMiddleName" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <asp:Button ID="btnSave" runat="server" Text="Сохранить изменения" CssClass="btn-save" OnClick="btnSave_Click" />
            </div>
            
            <!-- Навигация -->
            <div class="profile-actions">
                <asp:Button ID="btnOrders" runat="server" Text="Мои заказы" CssClass="btn-action" 
                    PostBackUrl="~/orders.aspx" OnClick="btnOrders_Click" />
                <asp:Button ID="btnCart" runat="server" Text="Моя корзина" CssClass="btn-action" 
                    PostBackUrl="~/cart.aspx" OnClick="btnCart_Click" />
                <asp:Button ID="btnChangePassword" runat="server" Text="Изменить пароль" CssClass="btn-action" 
                    OnClick="btnChangePassword_Click" />
            </div>


                    <div id="NewPassModal" runat="server" class="modal" style="display:none;">
    <div class="modal-content">
        <span class="close" onclick="closePassModal()">&times;</span>
        <h2>Изменить пароль</h2>
        <div class="form-group2">
            <label for="OldPass">Старый пароль:</label>
            <asp:TextBox ID="OldPass" runat="server" TextMode="Password" CssClass="form-control2"></asp:TextBox>
        </div>
        <div class="form-group2">
            <label for="NewPass">Новый пароль:</label>
            <asp:TextBox ID="NewPass" runat="server" TextMode="Password" CssClass="form-control2"></asp:TextBox>
        </div>
        <div class="form-group2">
    <label for="NewPass2">Повторите новый пароль:</label>
    <asp:TextBox ID="NewPass2" runat="server" TextMode="Password" CssClass="form-control2"></asp:TextBox>
</div>
        <div class="buttons-container">
        <asp:Button ID="btnChange" runat="server" Text="Сохранить изменения" CssClass="btn-changepass" OnClick="btnChangePass_Click" />
             </div>
    </div>
</div>

        </div>

    </form>

    <script>

function closePassModal() {
    document.getElementById('<%= NewPassModal.ClientID %>').style.display = 'none';
    }
  
    </script>

</body>
</html>
