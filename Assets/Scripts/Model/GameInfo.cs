using QFramework;
using UnityEngine;

public class GameInfo : AbstractModel
{
    public BindableProperty<int> killNum = new BindableProperty<int>(); // 总击杀数
    protected override void OnInit()
    {
        killNum.SetValueWithoutEvent(0);
    }
}
