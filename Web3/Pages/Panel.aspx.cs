using System;
// using System.Web.UI;

namespace Web3.Pages;

// using System;

public partial class Panel : System.Web.UI.Page
{
    
    // Track popup visibility using ViewState
    // private bool IsPopupVisible
    // {
    //     get => ViewState["PopupVisible"] != null && (bool)ViewState["PopupVisible"];
    //     set => ViewState["PopupVisible"] = value;
    // }
    
    protected void Page_PreRender(object sender, EventArgs e)
    {
        // Show or hide popup overlay based on flag
        // popupOverlay.Style["display"] = IsPopupVisible ? "block" : "none";
    }

    protected void btnShowPopup_Click(object sender, EventArgs e)
    {
        popupOverlay.Style["display"] = "block";
        lblSuccess.Visible = false;
    }

    protected void btnClose_Click(object sender, EventArgs e)
    {
        // IsPopupVisible = false;
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        // Page.Validators
        Page.Validate(); // Run validators

        if (Page.IsValid)
        {
            lblSuccess.Text = $"Hello, {txtName.Text}! Your data has been saved.";
            lblSuccess.Visible = true;
            // IsPopupVisible = false; // Close popup on success
            btnClose.Enabled = true;
        }
        else
        {
            Page.Visible = false;
            // popupOverlay.Style["display"] = "block";
            // popupOverlay.EnableViewState = false;
            // IsPopupVisible = true; // Keep popup visible
            // btnClose.Enabled = false; // Disable Close button to force fixing validation errors
        }
    }
}
