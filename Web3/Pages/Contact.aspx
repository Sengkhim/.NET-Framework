<%@ 
    Page Title="Contact"
    Language="C#" 
    MasterPageFile="~/Pages/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Contact.aspx.cs"
    Inherits="Web3.Pages.Contact"
    Async="true"
%>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
        
        <h5><%: ConnectionString %></h5>
        
        <asp:Repeater ID="ProductRepeater" runat="server">

            <ItemTemplate>
                <tr>
                    <td><%# Eval("Name") %></td>
                    <td><%# Eval("Description") %></td>
                    <td><%# Eval("Price", "{0:C}") %></td>
                </tr>
            </ItemTemplate>

        </asp:Repeater>
        
    </main>
</asp:Content>
