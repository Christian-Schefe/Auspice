using System.Collections.Generic;
using UnityEngine;

public class UIBuildMenu : MonoBehaviour
{
    public RectTransform layoutGroup;
    public PuzzleEditor editor;
    public UIBuildItem buildItemPrefab;
    public List<IconMapEntry> iconSprites;

    private readonly Dictionary<EntityType, (BuildItemData, UIBuildItem)> entityData = new();
    private UIBuildItem eraserItem;
    private UIBuildItem selectedItem;

    public void SetData(Dictionary<EntityType, int?> editableEntities)
    {
        var selectErase = new System.Action(() =>
        {
            editor.SetSelectedType(new(PuzzleEntityType.None));
            selectedItem.SetSelected(false);
            selectedItem = eraserItem;
            selectedItem.SetSelected(true);
        });

        var eraserData = new BuildItemData(null, iconSprites.Find(e => e.type.basicType == PuzzleEntityType.None).icon, selectErase);
        eraserItem = Instantiate(buildItemPrefab, layoutGroup);
        eraserItem.UpdateData(eraserData, false);
        eraserItem.SetSelected(true);
        selectedItem = eraserItem;

        foreach (var pair in editableEntities)
        {
            var key = pair.Key;
            var onSelect = new System.Action(() =>
            {
                editor.SetSelectedType(key);
                selectedItem.SetSelected(false);
                selectedItem = entityData[key].Item2;
                selectedItem.SetSelected(true);
            });
            var spriteEntry = iconSprites.Find(e => e.type == key);
            Debug.Assert(spriteEntry != null, $"No sprite found for {key}");
            var data = new BuildItemData(pair.Value, spriteEntry.icon, onSelect);
            var item = Instantiate(buildItemPrefab, layoutGroup);
            entityData[key] = (data, item);
            item.UpdateData(data);
            item.SetSelected(false);

        }
    }

    public bool CanConsumeEntity(EntityType type)
    {
        if (entityData.TryGetValue(type, out var data))
        {
            return data.Item1.number != 0;
        }

        return false;
    }

    public void ConsumeEntity(EntityType type)
    {
        if (entityData.TryGetValue(type, out var data))
        {
            if (data.Item1.number == 0) throw new System.InvalidOperationException();
            if (data.Item1.number != null) data.Item1.number--;
            data.Item2.UpdateData(data.Item1);
        }
    }

    public void ReturnEntity(EntityType type)
    {
        if (entityData.TryGetValue(type, out var data))
        {
            if (data.Item1.number != null) data.Item1.number++;
            data.Item2.UpdateData(data.Item1);
        }
    }

    public class BuildItemData
    {
        public int? number;
        public Sprite icon;
        public System.Action select;

        public BuildItemData() { }

        public BuildItemData(int? number, Sprite icon, System.Action select)
        {
            this.number = number;
            this.icon = icon;
            this.select = select;
        }
    }

    [System.Serializable]
    public class IconMapEntry
    {
        public EntityType type;
        public Sprite icon;

        public IconMapEntry(EntityType type, Sprite icon)
        {
            this.type = type;
            this.icon = icon;
        }
    }
}
