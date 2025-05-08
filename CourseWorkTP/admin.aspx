<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="admin.aspx.cs" Inherits="CourseWorkTP.admin" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <link href="admin.css" rel="stylesheet" />
    <title>Hot Pot Express</title>
    <link href="https://fonts.googleapis.com/css2?family=Bahnschrift&display=swap" rel="stylesheet">
</head>
<body>
    <form id="form1" runat="server">
        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>
        <div style="height: 130px; width: 100%; display: flex; align-items: center; justify-content: center; position: relative;">
    <!-- Логотип по центру -->
    <asp:ImageButton ID="ImageButton3" runat="server" CssClass="auto-style5" Height="107px" 
        ImageUrl="~/images/logo.png" Width="443px" PostBackUrl="~/admin.aspx" />
    
    <!-- Кнопка выхода справа -->
    <asp:Button ID="btnLogout" runat="server" Text="Выход" 
        style="position: absolute; right: 20px; height: 40px; width: 100px;"
        OnClick="btnLogout_Click" CssClass="logout-button" />
</div>

        <div class="menu-container">
    <asp:Menu ID="Menu1" runat="server" Orientation="Horizontal" 
        OnMenuItemClick="Menu1_MenuItemClick" CssClass="tab-menu"
        StaticSelectedStyle-CssClass="selected">
        <Items>
            <asp:MenuItem Text="Товары" Value="0" Selected="true" />
            <asp:MenuItem Text="Категории" Value="1" />
            <asp:MenuItem Text="Заказы" Value="2" />
        </Items>
    </asp:Menu>
</div>

<asp:MultiView ID="MultiView1" runat="server" ActiveViewIndex="0">
    
    <asp:View ID="View1" runat="server">
        <asp:UpdatePanel ID="UpdatePanel1" runat="server" UpdateMode="Conditional">
<ContentTemplate>
    <div class="add-product-container">
    <div class="add-product-form">
    <h3>Добавить новый товар</h3>
    <div class="form-group">
        <asp:Label runat="server" Text="Название:" AssociatedControlID="txtNewTitle" />
        <asp:TextBox ID="txtNewTitle" runat="server" CssClass="form-control" />
    </div>
    <div class="form-group">
        <asp:Label runat="server" Text="Описание:" AssociatedControlID="txtNewDesc" />
        <asp:TextBox ID="txtNewDesc" runat="server" TextMode="MultiLine" Rows="3" CssClass="form-control" />
    </div>
    <div class="form-group">
        <asp:Label runat="server" Text="Категория:" AssociatedControlID="txtNewCategory" />
        <asp:TextBox ID="txtNewCat" runat="server" CssClass="form-control" />
    </div>

    <div class="form-row"> 
        <div class="form-group">
            <asp:Label runat="server" Text="Количество:" AssociatedControlID="txtNewAmount"  />
            <asp:TextBox ID="txtNewAmount" runat="server" TextMode="Number" CssClass="form-control" />
        </div>
        <div class="form-group">
            <asp:Label runat="server" Text="Цена:" AssociatedControlID="txtNewPrice" ID="lblNewPrice" />
            <asp:TextBox ID="txtNewPrice" runat="server" TextMode="Number" CssClass="form-control" />
        </div>
    </div>

    <asp:Button ID="btnAddProduct" runat="server" Text="Добавить товар" 
        CssClass="btn-add-product" OnClick="btnAddProduct_Click" />
</div>
    </div>


    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false"
        OnRowEditing="GridView1_RowEditing" OnRowDeleting="GridView1_RowDeleting" OnRowUpdating="GridView1_RowUpdating" 
        OnRowCancelingEdit="GridView1_RowCancelingEdit" DataKeyNames="id"
        CssClass="goods-grid" GridLines="None">
        <Columns>
            
            <asp:BoundField DataField="id" HeaderText="ID" ReadOnly="true" ItemStyle-CssClass="grid-id" />
            
            <asp:TemplateField HeaderText="Название" ItemStyle-CssClass="grid-title">
                <ItemTemplate>
                    <%# Eval("title") %>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtTitle" runat="server" Text='<%# Bind("title") %>' CssClass="grid-edit-field" />
                </EditItemTemplate>
            </asp:TemplateField>
            
            
            <asp:TemplateField HeaderText="Описание" ItemStyle-CssClass="grid-desc">
                <ItemTemplate>
                    <%# TruncateDescription(Eval("description").ToString()) %>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtDesc" runat="server" Text='<%# Bind("description") %>' 
                        TextMode="MultiLine" Rows="2" CssClass="grid-edit-field" />
                </EditItemTemplate>
            </asp:TemplateField>
            
            
            <asp:TemplateField HeaderText="Категория" ItemStyle-CssClass="grid-category">
    <ItemTemplate>
        <%# Eval("category_name") %>
    </ItemTemplate>
    <EditItemTemplate>
        <asp:TextBox ID="txtCategory" runat="server" Text='<%# Bind("category_name") %>' CssClass="grid-edit-field" />
    </EditItemTemplate>
</asp:TemplateField>
            
            
            <asp:TemplateField HeaderText="Количество" ItemStyle-CssClass="grid-amount">
                <ItemTemplate>
                    <%# Eval("amount") %>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtAmount" runat="server" Text='<%# Bind("amount") %>' 
                        TextMode="Number" CssClass="grid-edit-field" />
                </EditItemTemplate>
            </asp:TemplateField>
            
            
            <asp:TemplateField HeaderText="Цена" ItemStyle-CssClass="grid-price">
                <ItemTemplate>
                    <%# string.Format("{0:C}", Eval("price")) %>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtPrice" runat="server" Text='<%# Bind("price") %>' 
                        TextMode="Number" CssClass="grid-edit-field" />
                </EditItemTemplate>
            </asp:TemplateField>
            
            
            <asp:CommandField ShowEditButton="true" showDeleteButton="true" ButtonType="Button"
    EditText="Редактировать" DeleteText="Удалить" UpdateText="Сохранить" CancelText="Отмена"
    ControlStyle-CssClass="grid-action-btn" />
               

        </Columns>
    </asp:GridView>
                </ContentTemplate>
</asp:UpdatePanel>
</asp:View>
        


    
    <asp:View ID="ViewCategories" runat="server">
                <asp:UpdatePanel ID="UpdatePanel2" runat="server" UpdateMode="Conditional">
<ContentTemplate>
    <%-- Форма добавления новой категории --%>
<div class="add-category-panel">
    <div class="add-category-form">
        <div class="add-category-title">Добавить новую категорию</div>
        <div class="add-category-input-group">
            <asp:TextBox ID="txtNewCategory" runat="server" 
                CssClass="add-category-input" placeholder="Название категории" />
            <asp:Button ID="btnAddCategory" runat="server" Text="Добавить" 
                CssClass="add-category-btn" OnClick="btnAddCategory_Click" />
        </div>
    </div>
</div>
    <asp:GridView ID="GridViewCategories" runat="server" AutoGenerateColumns="false"
        OnRowEditing="GridViewCategories_RowEditing" OnRowUpdating="GridViewCategories_RowUpdating"
        OnRowCancelingEdit="GridViewCategories_RowCancelingEdit" DataKeyNames="id"
        CssClass="categories-grid" GridLines="None">
        <Columns>
            <%-- ID категории (только для чтения) --%>
            <asp:BoundField DataField="id" HeaderText="ID" ReadOnly="true" ItemStyle-CssClass="grid-id" />
            
            <%-- Название категории --%>
            <asp:TemplateField HeaderText="Название категории" ItemStyle-CssClass="grid-category-name">
                <ItemTemplate>
                    <%# Eval("title") %>
                </ItemTemplate>
                <EditItemTemplate>
                    <asp:TextBox ID="txtCategoryName" runat="server" Text='<%# Bind("title") %>' 
                        CssClass="grid-edit-field" MaxLength="50" />
                </EditItemTemplate>
            </asp:TemplateField>
            
            <%-- Кнопки действий --%>
            <asp:CommandField ShowEditButton="true" ButtonType="Button"
                EditText="Редактировать" UpdateText="Сохранить" CancelText="Отмена"
                ControlStyle-CssClass="grid-action-btn" />
                
            
        </Columns>
    </asp:GridView>
    
    
                    </ContentTemplate>
</asp:UpdatePanel>
</asp:View>

        <asp:View ID="View2" runat="server">
    <asp:UpdatePanel ID="UpdatePanel3" runat="server" UpdateMode="Conditional">
        <ContentTemplate>
            <asp:GridView ID="GridView2" runat="server" AutoGenerateColumns="false"
    OnRowEditing="GridView2_RowEditing" OnRowUpdating="GridView2_RowUpdating" 
    OnRowCancelingEdit="GridView2_RowCancelingEdit" DataKeyNames="id,status"
    CssClass="orders-grid" GridLines="None" OnRowCommand="GridView2_RowCommand">
                <Columns>
                    <asp:BoundField DataField="id" HeaderText="ID заказа" ReadOnly="true" ItemStyle-CssClass="grid-id" />
                    <asp:BoundField DataField="date" HeaderText="Дата" ReadOnly="true" DataFormatString="{0:yyyy-MM-dd HH:mm}" />
                    <asp:BoundField DataField="total" HeaderText="Сумма" ReadOnly="true" DataFormatString="{0:C}" />
                    
                    <asp:TemplateField HeaderText="Статус" ItemStyle-CssClass="grid-status">
                        <ItemTemplate>
                            <%# Eval("status") %>
                        </ItemTemplate>
                        <EditItemTemplate>
                            <asp:DropDownList ID="ddlStatus" runat="server" CssClass="status-dropdown">
                                <asp:ListItem Text="Processing" Value="Processing" />
                                <asp:ListItem Text="Delivering" Value="Delivering" />
                                <asp:ListItem Text="Finished" Value="Finished" />
                            </asp:DropDownList>
                        </EditItemTemplate>
                    </asp:TemplateField>
                    
                    <asp:BoundField DataField="address" HeaderText="Адрес доставки" ReadOnly="true" ItemStyle-CssClass="grid-address" />
                    <asp:BoundField DataField="email" HeaderText="Email пользователя" ReadOnly="true" ItemStyle-CssClass="grid-email" />
                    
                    <asp:TemplateField HeaderText="Действия">
    <ItemTemplate>  
        <asp:Button ID="btnEdit" runat="server" Text="Редактировать" 
            CommandName="Edit" CssClass="grid-action-btn" />
        <asp:Button ID="btnDetails" runat="server" Text="Детали" 
            CommandName="ShowModal" CommandArgument='<%# Eval("id") %>'
            CssClass="grid-details-btn" />
    </ItemTemplate>
    <EditItemTemplate>
        <asp:Button ID="btnUpdate" runat="server" Text="Сохранить" CommandName="Update" 
            CssClass="grid-action-btn" />
        <asp:Button ID="btnCancel" runat="server" Text="Отмена" CommandName="Cancel" 
            CssClass="grid-action-btn" />
    </EditItemTemplate>
</asp:TemplateField>
                </Columns>
            </asp:GridView>
            
            <!-- Модальное окно с деталями заказа -->
<div id="orderDetailsModal" class="modal" runat="server" style="display:none;">
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
            
            <h3>Товары в заказе:</h3>
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
        </ContentTemplate>
    </asp:UpdatePanel>
</asp:View>


</asp:MultiView>


    </form>

    <script>
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
