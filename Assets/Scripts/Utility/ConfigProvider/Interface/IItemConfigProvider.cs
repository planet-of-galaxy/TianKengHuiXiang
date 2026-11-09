using QFramework;

public interface IItemConfigProvider : IUtility
{
    ItemConfig GetItemConfig(int itemId);
    bool HasItem(int itemId);
    int GetItemCount();
}
