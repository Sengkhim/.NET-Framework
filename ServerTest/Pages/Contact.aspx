<%@ 
    Page Title="Contact"
    Language="C#" 
    MasterPageFile="~/Pages/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Contact.aspx.cs"
    Inherits="ServerTest.Pages.Contact"
%>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
        
        <h2 id="title"><%: DisplayEmail %>.</h2>
        
        <h3>Your contact page.</h3>
        
    </main>
</asp:Content>
