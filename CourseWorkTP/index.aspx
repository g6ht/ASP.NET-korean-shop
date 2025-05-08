<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="CourseWorkTP.index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link rel="stylesheet" href="style.css">
    <title>Hot Pot Express</title>
    <link href="https://fonts.googleapis.com/css2?family=Bahnschrift&display=swap" rel="stylesheet">
    <link rel="icon" href="~/favicon.ico" type="image/x-icon" />
    <link rel="shortcut icon" href="~/favicon.ico" type="image/x-icon" />
</head>
<body style="height: 100%">
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server" EnablePartialRendering="true"></asp:ScriptManager>

        <header style="background: white; padding: 15px 0; box-shadow: 0 2px 10px rgba(0,0,0,0.1); position: sticky; top: 0; z-index: 100;">
            <div class="header-container" style="max-width: 1200px; margin: 0 auto; display: flex; flex-wrap: wrap; align-items: center; justify-content: space-between;">
                
                <!-- Логотип -->
                <div class="logo-container" style="flex: 1; min-width: 200px; text-align: center;">
                    <asp:ImageButton ID="ImageButton3" runat="server" CssClass="logo" Height="80px" ImageUrl="~/images/logo.png" PostBackUrl="~/index.aspx" />
                </div>
                
                <!-- Поиск (новый элемент) -->
                <div class="search-container" style="flex: 2; min-width: 300px; padding: 0 20px;">
                    <div style="display: flex; align-items: center; background: #f5f5f5; border-radius: 30px; padding: 8px 15px;">
                        <asp:TextBox ID="txtSearch" runat="server" 
                            placeholder="Поиск товаров..." 
                            style="border: none; background: transparent; width: 100%; outline: none; font-family: 'Bahnschrift';">
                        </asp:TextBox>
                        <asp:LinkButton ID="btnSearch" runat="server" OnClick="btnSearch_Click" CssClass="search-btn">
                            <i class="fas fa-search"></i>
                        </asp:LinkButton>
                    </div>
                </div>
                
                <!-- Правая часть (иконки) -->
                <div class="icons-container" style="flex: 1; min-width: 200px; display: flex; justify-content: flex-end; gap: 20px;">
                    <asp:ImageButton ID="ImageButton1" runat="server" 
                        CssClass="header-icon" 
                        Height="40px" 
                        ImageUrl="~/images/cart.png" 
                        Width="40px" 
                        OnClick="ImageButton1_Clicked"
                        style="transition: transform 0.3s;"
                        onmouseover="this.style.transform='scale(1.1)'"
                        onmouseout="this.style.transform='scale(1)'"/>
                    
                    <asp:ImageButton ID="ImageButton2" runat="server" 
                        CssClass="header-icon" 
                        Height="40px" 
                        ImageUrl="~/images/user.png" 
                        Width="40px" 
                        OnClick="ImageButton2_Clicked"
                        style="transition: transform 0.3s;"
                        onmouseover="this.style.transform='scale(1.1)'"
                        onmouseout="this.style.transform='scale(1)'"/>
                </div>
            </div>
            
            <!-- Выбор категории -->
            <div class="category-container" style="max-width: 600px; margin: 3px auto 0; padding: 0 15px;">
                <asp:DropDownList ID="DropDownList1" runat="server" 
                    BackColor="#FF30C6" 
                    CssClass="category-dropdown" 
                    Font-Names="Bahnschrift" 
                    ForeColor="White" 
                    Height="50px" 
                    OnSelectedIndexChanged="DropDownList1_SelectedIndexChanged" 
                    Width="100%" 
                    AutoPostBack="True"
                    style="border-radius: 8px; border: none; padding: 0 15px;">
                </asp:DropDownList>
            </div>
        </header>

        <!-- Окно входа в аккаунт -->
        <div id="loginModal" runat="server" class="modal" style="display:none;">
            <div class="modal-content">
                <span class="close" onclick="closeAuthModal()">&times;</span>
                <h2>Авторизация</h2>
                <div class="form-group">
                    <label for="loginEmail">Email:</label>
                    <asp:TextBox ID="loginEmail" runat="server" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="form-group">
                    <label for="loginPassword">Пароль:</label>
                    <asp:TextBox ID="loginPassword" runat="server" TextMode="Password" CssClass="form-control"></asp:TextBox>
                </div>
                <div class="buttons-container">
                    <asp:Button ID="btnLogin" runat="server" Text="Войти" CssClass="btn-login" OnClick="btnLogin_Click" />
                    <asp:Button ID="btnRegister" runat="server" Text="Зарегистрироваться" CssClass="btn-register" OnClick="btnRegister_Click" />
                </div>
            </div>
        </div>

        <!-- Окно регистрации -->
                <div id="registerModal" runat="server" class="modal" style="display:none;">
                    <div class="modal-content">
                        <span class="close" onclick="closeRegModal()">&times;</span>
                        <h2>Регистрация</h2>
                        <div class="form-group">
                            <label for="loginEmail">Email:</label>
                            <asp:TextBox ID="registerEmail" runat="server" CssClass="form-control"></asp:TextBox>
                        </div>
                    <div class="form-group">
                            <label for="loginPassword">Пароль:</label>
                            <asp:TextBox ID="registerPassword" runat="server" TextMode="Password" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label for="loginPassword">Повторите пароль:</label>
                        <asp:TextBox ID="registerPassword2" runat="server" TextMode="Password" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="form-group">
                        <label for="loginName">Имя:</label>
                        <asp:TextBox ID="registerName" runat="server" CssClass="form-control"></asp:TextBox>
                    </div>
                    <div class="buttons-container">
                        <asp:Button ID="btnRegister2" runat="server" Text="Зарегистрироваться" CssClass="btn-register" OnClick="btnRegister_Click2" />
                    </div>
                </div>
            </div>

        <!-- Главная часть с товарами -->
        <div style="flex: 1;">
            <asp:UpdatePanel ID="GoodsUpdatePanel" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="main-content">
                        <asp:Panel ID="GoodsContainer" runat="server" CssClass="auto-style272" Height="100%"></asp:Panel>
                    </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="DropDownList1" EventName="SelectedIndexChanged" />
            </Triggers>
        </asp:UpdatePanel>
      </div>

        <!-- Футер -->
        <footer style="background: #005591; color: white; padding: 30px 0; margin-top: auto;">
            <div style="max-width: 1200px; margin: 0 auto; display: flex; flex-wrap: wrap; justify-content: space-between;">
                <div style="flex: 1; min-width: 250px; padding: 0 15px;">
                    <h3 style="color: #FF30C6; font-family: 'Bahnschrift';">Hot Pot Express</h3>
                    <p>Доставка вкуснейшей корейской еды прямо до вашей двери</p>
                </div>
                
                <div style="flex: 1; min-width: 250px; padding: 0 15px;">
                    <h4 style="font-family: 'Bahnschrift';">Контакты</h4>
                    <ul style="list-style: none; padding: 0;">
                        <li><i class="fas fa-phone"></i> +7 (123) 456-7890</li>
                        <li><i class="fas fa-envelope"></i> info@hotpotexpress.com</li>
                        <li><i class="fas fa-map-marker-alt"></i> г. Казань, ул. Баумана, 123</li>
                    </ul>
                </div>
                
                <div style="flex: 1; min-width: 250px; padding: 0 15px;">
                    <h4 style="font-family: 'Bahnschrift';">Мы в соцсетях</h4>
                    <div style="display: flex; gap: 15px; font-size: 20px;">
                        <a href="#" style="color: white;"><i class="fab fa-instagram"></i></a>
                        <a href="#" style="color: white;"><i class="fab fa-telegram"></i></a>
                        <a href="#" style="color: white;"><i class="fab fa-vk"></i></a>
                    </div>
                </div>
            </div>
            
            <div style="text-align: center; margin-top: 20px; padding-top: 15px; border-top: 1px solid #34495e;">
                <p>© 2025 Hot Pot Express. Все права защищены.</p>
            </div>
        </footer>
        
    </form>

    <script>
    
    function closeAuthModal() {
        document.getElementById('<%= loginModal.ClientID %>').style.display = 'none';
    }

    function closeRegModal() {
        document.getElementById('<%= registerModal.ClientID %>').style.display = 'none';
    }

    function handleEnterKey(event) {
        if (event.keyCode === 13) {
            event.preventDefault();
            document.getElementById('<%= btnSearch.ClientID %>').click();
        }
    }

    document.getElementById('<%= txtSearch.ClientID %>').addEventListener('keypress', handleEnterKey);

    </script>

</body>
</html>
