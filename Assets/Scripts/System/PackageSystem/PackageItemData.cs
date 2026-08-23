using System.Collections.Generic;

public class PackageItemData
{
    public int index;
    public int configId;
    public ItemType type;
    public int num;
}

public class PackageSaveData
{
    public List<PackageItemData> packageItems;
    public int capacity;
    public int heldIndex;
}