<%@ 
    Page Language="C#" 
    MasterPageFile="~/Pages/Site.Master"
    AutoEventWireup="true"
    CodeBehind="Panel.aspx.cs"
    Inherits="Web3.Pages.Panel"
%>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:ScriptManager ID="ScriptManager1" runat="server" /> --%>
    
    <style>
        .overlay {
            position: fixed;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            background-color: rgba(0,0,0,0.4);
            display: none;
            z-index: 1000;
            pointer-events: auto;
        }

        .popup-panel {
            width: 400px;
            margin: 0 auto;
            background-color: white;
            padding: 20px;
            position: relative;
            top: -300px;
            animation: slideDown 0.5s forwards;
            border-radius: 8px;
            box-shadow: 0 0 10px gray;
            pointer-events: auto;
        }

        @keyframes slideDown {
            to {
                top: 100px;
            }
        }
    </style>

    <!-- Button to show popup -->
    <asp:Button ID="btnShowPopup" runat="server" Text="Show Form" OnClick="btnShowPopup_Click" />

    <!-- Success message -->
    <asp:Label ID="lblSuccess" runat="server" ForeColor="Green" Visible="false" Font-Bold="true" />

    <%-- <asp:ScriptManager ID="ScriptManager1" runat="server" /> --%>
    
    <!-- Overlay panel -->
    <asp:Panel ID="popupOverlay" runat="server" CssClass="overlay" ViewStateMode="Enabled">
        <div class="popup-panel">
            <h3>Enter Your Name</h3>

            <!-- Input -->
            <asp:TextBox ID="txtName" runat="server" Width="100%" />
            
            <br />
            <asp:RequiredFieldValidator
                ID="rfvName" runat="server"
                ControlToValidate="txtName"
                ErrorMessage="Name is required."
                ForeColor="Red"
                Display="Dynamic"
                EnableClientScript="true" />
            <br /><br />

            <!-- Buttons -->
            <asp:Button ID="btnSave" runat="server" Text="Save" OnClick="btnSave_Click" />
            &nbsp;
            <asp:Button ID="btnClose" runat="server" Text="Close" OnClick="btnClose_Click" />
        </div>
    </asp:Panel>

</asp:Content>
