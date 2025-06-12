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
        
        <h2 id="title"><%: ContactService.GetEmail() %>.</h2>
        
        <h3>Your contact page.</h3>
        
        <address>
            One Microsoft Way<br />
            Redmond, WA 98052-6399<br />
            <abbr title="Phone">P:</abbr>
            425.555.0100
        </address>
        
    </main>
</asp:Content>
