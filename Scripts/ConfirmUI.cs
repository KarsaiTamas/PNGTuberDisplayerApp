using Godot;
using System;

public partial class ConfirmUI : Control
{
    //Check if i can reuse this via having 2 action variables
    //1 for 'yes' and for the 'no' button
    public static ConfirmUI Instance { get; private set; }
    public Label confirmLabel;
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
        confirmLabel = GetNode<Label>("C/PC/VBC/MC/Label");
        yesButton = GetNode<Button>("C/PC/VBC/HBC/AcceptMC/AcceptButton");
        noButton = GetNode<Button>("C/PC/VBC/HBC/NoMC/NoButton");
        cancelButton = GetNode<Button>("C/PC/VBC/HBC/CancelMC/CancelButton");
        yesButton.Pressed += DestroyConfirm;
        noButton.Pressed += DestroyConfirm;
        cancelButton.Pressed += DestroyConfirm;
        
        isActive = false;
        Hide();
        canvasLayer.Visible = false;
    }

    public void SetConfirm(string labelText, Action yesPressed )
    {
        if (isActive) return;
        noButton.Hide();
        yesAction = yesPressed;
        confirmLabel.Text = labelText;
        yesButton.Pressed += yesAction; 
        Show();
        canvasLayer.Visible = true;
        isActive = true;
    }
    public void SetConfirm(string labelText, Action yesPressed, Action noPressed)
    {
        if (isActive) return;
        noButton.Show();
        yesAction = yesPressed;
        noAction = noPressed;
        confirmLabel.Text = labelText;
        yesButton.Pressed += yesAction;
        noButton.Pressed += noAction;
        Show();
        canvasLayer.Visible = true;
        isActive = true;
    }
    public void DestroyConfirm()
    {
        if (!isActive) return;
        yesButton.Pressed -= yesAction; 
        if(noAction != null)
        {
            noButton.Pressed -= noAction;
            noAction=null;
        }
        isActive = false;
        canvasLayer.Visible = false;
        //GameHandler.confirmUI = null;
        Hide();
        //QueueFree();
    }
}
