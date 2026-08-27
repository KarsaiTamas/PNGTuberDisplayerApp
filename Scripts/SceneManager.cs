using Godot;
using System;
using System.Collections.Generic;
using SLM=SaveLoadManager;

public partial class SceneManager : Node
{
    public static SceneManager instance;
    public static int IDLoadedScene=-1;
    public bool isEdited=false;
    public Node sceneCharacters;
    public PanelContainer SceneUIPanel;
    public LineEdit sceneNameLE;
    public Button CloseSceneButton;
    public SceneData data;
    public List<Character> charactersOnScene;
    public override void _EnterTree()
    {
        instance = this;
        Init();
    }
    void Init()
    {
        sceneCharacters = GetNode<Control>("SceneCharacters");
        SceneUIPanel = GetNode<PanelContainer>("TopPC");
        sceneNameLE = GetNode<LineEdit>("TopPC/HC/SceneNameLE");
        CloseSceneButton = GetNode<Button>("TopPC/HC/CloseSceneButton");
        charactersOnScene = new List<Character>();
        CloseSceneButton.ButtonDown += UIManager.instance.CloseScene;
        sceneNameLE.TextChanged += (e) => { isEdited = !data.sceneName.Equals(e); };
        SceneUIPanel.Hide();
    }
    public void RemoveCharacters()
    {
        foreach (var item in charactersOnScene)
        {
            ProgramManager.instance.spawnedCharacters.Remove(item);
            item.QueueFree();
        }
        charactersOnScene.Clear();
    } 
    public void PutUIDataToSceneData()
    {
        data.sceneName= sceneNameLE.Text;
    }

    public void CreateScene()
    {
        string sceneName = "Scene_" + SLM.DateTimeForSave();
        SLM.scenesToLoad.Add(SLM.SAVEDSCENESLOCATION + sceneName);
        GD.Print("Creating scene");
        SLM.Save(SLM.scenesToLoad, SLM.SAVEDSCENESFILE, SLM.SAVEDSCENESLOCATION);
        SLM.Save(new SceneData(sceneName), sceneName, SLM.SAVEDSCENESLOCATION);
        UIManager.instance.AddSceneToSceneOptions(sceneName);

    }
    public void DeleteScene()
    {
        if (!SLM.DeleteFile(SLM.scenesToLoad[IDLoadedScene])) return;
        UIManager.instance.RemoveSceneFromPopupMenu(IDLoadedScene);
        SLM.scenesToLoad.RemoveAt(IDLoadedScene);
        SLM.Save(SLM.scenesToLoad, SLM.SAVEDSCENESFILE, SLM.SAVEDSCENESLOCATION);
        isEdited = false;
        UIManager.instance.CloseScene();
        

    }

    public void SaveScene()
    {
        isEdited = false;
        PutUIDataToSceneData();

        if (!SLM.RenamePathInList(SLM.scenesToLoad,
            IDLoadedScene,
            data.sceneName,
            SLM.SAVEDSCENESFILE,
            SLM.SAVEDSCENESLOCATION))
        {
            ConfirmUI.Instance.ShowConfirm("Failed to save Scene.");
            return;
        }

        UIManager.instance.RenameSceneInPopupMenu(
            IDLoadedScene,
            data.sceneName);

        SLM.Save(data, data.sceneName, SLM.SAVEDSCENESLOCATION);

    }

    public void LoadScene(int id)
    {
        if (!isEdited)
        {
            SLM. LoadScene(id);
        }
        else
        {
            ConfirmUI.Instance.ShowConfirm("You have unsaved changes, would you like to save before loading new scene?",
                () => {
                    SaveScene();
                    SLM.LoadScene(id);
                },
                () =>
                {
                    SLM.LoadScene(id);
                    isEdited = false;

                });
        }
    }
}
