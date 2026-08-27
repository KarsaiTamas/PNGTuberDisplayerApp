using Godot;
using System;

public partial class ConfirmUI : Control
{
    //Check if i can reuse this via having 2 action variables
    //1 for 'yes' and for the 'no' button
    public static ConfirmUI Instance { get; private set; }
    public Label confirmLabel;
    public LineEdit confirmLE;
    public Button yesButton;
    public Button noButton;
    public Button cancelButton;
    CanvasLayer canvasLayer;
    Action yesAction;
    Action noAction;
    public static bool isActive;
    //Panel/VBoxContainer/MessageLabel
    //Panel/VBoxContainer/HBoxContainer/YesMC/Button
    //Panel/VBoxContainer/HBoxContainer/NoMC/Button
    public override void _EnterTree()
    {
        Instance = this;
        canvasLayer = GetNode<CanvasLayer>("C");
        confirmLabel = GetNode<Label>("C/PC/VBC/Label");
        yesButton = GetNode<Button>("C/PC/VBC/HBC/AcceptButton");
        noButton = GetNode<Button>("C/PC/VBC/HBC/NoButton");
        cancelButton = GetNode<Button>("C/PC/VBC/HBC/CancelButton");
        confirmLE = GetNode<LineEdit>("C/PC/VBC/LineEdit");
        yesButton.Pressed += DestroyConfirm;
        noButton.Pressed += DestroyConfirm;
        cancelButton.Pressed += DestroyConfirm;

        isActive = false;
        Hide();
        canvasLayer.Visible = false;
    }

    public void ShowConfirm(string labelText)
    {
        if (isActive) return;
        noButton.Hide();
        yesButton.Hide();
        cancelButton.Show();
        confirmLE.Hide(); 
        cancelButton.Text = "OK";
        ShowUI(labelText);

    }
    public void ShowConfirm(string labelText, Action yesPressed)
    {
        if (isActive) return;
        noButton.Hide();
        confirmLE.Hide();
        cancelButton.Text = "Cancel";
        yesAction = yesPressed; 
        yesButton.Pressed += yesAction;
        ShowUI(labelText);

    }
    public void ShowConfirm(string labelText, Action yesPressed, Action noPressed)
    {
        if (isActive) return;
        noButton.Show();
        confirmLE.Hide();
        cancelButton.Text = "Cancel";
        yesAction = yesPressed;
        noAction = noPressed; 
        yesButton.Pressed += yesAction;
        noButton.Pressed += noAction;
        ShowUI(labelText);
    }
    public void ShowTextConfirm(string labelText,string yesText, Action yesPressed)
    {
        if (isActive) return;
        noButton.Hide();
        yesAction = yesPressed;
        confirmLE.Show();
        yesButton.Pressed += yesAction;
        yesButton.Text = yesText;
        cancelButton.Text = "Cancel";
        ShowUI(labelText);
    }
    void ShowUI(string labelText)
    {
        confirmLabel.Text = labelText;
        Show();
        canvasLayer.Visible = true;
        isActive = true;
    }
    public void DestroyConfirm()
    {
        if (!isActive) return;
        if (yesAction != null)
        {
            yesButton.Pressed -= yesAction;
            yesAction = null;
        }
        if (noAction != null)
        {
            noButton.Pressed -= noAction;
            noAction = null;
        }
        isActive = false;
        canvasLayer.Visible = false;
        //GameHandler.confirmUI = null;
        Hide();
        //QueueFree();
    }
}
