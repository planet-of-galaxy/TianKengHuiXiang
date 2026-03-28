using QFramework;

public class StartPanel : UIPanel, IController
{
    protected override void OnInit(IUIData uiData = null)
    {

    }

    protected override void OnClose()
    {

    }

    public IArchitecture GetArchitecture()
    {
        return TianArchitecture.Interface;
    }
}
