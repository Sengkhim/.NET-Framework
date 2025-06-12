<%@ 
    Page Title="About"
    Language="C#"
    MasterPageFile="~/Pages/Site.Master"
    AutoEventWireup="true"
    CodeBehind="About.aspx.cs"
    Inherits="ServerTest.About"
%>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <main aria-labelledby="title">
        <h2 id="title"><%: Message.GetMessage() %>.</h2>
        <h3>Your application description page.</h3>
        <p>Use this area to provide additional information.</p>
    </main>
</asp:Content>
