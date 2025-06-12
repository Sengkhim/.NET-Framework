<%@ 
    Page Title="Contact"
    Language="C#" 
    MasterPageFile="~/Pages/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Contact.aspx.cs"
    Inherits="Web3.Pages.Contact"
%>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
        
        <h2 id="title">Email: <%: DisplayEmail %>.</h2>
        <h6>Connection String: <%: ConnectionString %></h6>        
        <h6>Level Log: <%: LogLevel %></h6>
        <h6>SiteName: <%: SiteName %></h6>
        
    </main>
</asp:Content>
