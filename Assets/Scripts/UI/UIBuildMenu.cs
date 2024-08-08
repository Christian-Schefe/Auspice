using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBuildMenu : MonoBehaviour
{
    public RectTransform layoutGroup;
    public UIBuildItem buildItemPrefab;
    public List<Sprite> iconSprites;

    private readonly Dictionary<PuzzleEntityType, (BuildItemData, UIBuildItem)> entityData = new();
    private UIBuildItem eraserItem;
    private UIBuildItem selectedItem;

    public void SetData(Dictionary<PuzzleEntityType, int?> editableEntities, System.Action onSelectErase, System.Action<PuzzleEntityType> onSelectObject)
    {
        var selectErase = new System.Action(() =>
        {
            onSelectErase();
            selectedItem.SetSelected(false);
            selectedItem = eraserItem;
            selectedItem.SetSelected(true);
        });

        var eraserData = new BuildItemData(null, iconSprites[0], selectErase);
        eraserItem = Instantiate(buildItemPrefab, layoutGroup);
        eraserItem.UpdateData(eraserData);
        eraserItem.SetSelected(true);
        selectedItem = eraserItem;

        foreach (var pair in editableEntities)
        {
            var key = pair.Key;
            var onSelect = new System.Action(() =>
            {
                onSelectObject(key);
                selectedItem.SetSelected(false);
                selectedItem = entityData[key].Item2;
                selectedItem.SetSelected(true);
            });
            var data = new BuildItemData(pair.Value, iconSprites[(int)key], onSelect);
            var item = Instantiate(buildItemPrefab, layoutGroup);
            entityData[key] = (data, item);
            item.UpdateData(data);
            item.SetSelected(false);

        }
    }

    public bool TryConsumeEntity(PuzzleEntityType type)
    {
        if (entityData.TryGetValue(type, out var data))
        {
            if (data.Item1.number == 0) return false;
            if (data.Item1.number != null) data.Item1.number--;
            data.Item2.UpdateData(data.Item1);
            return true;
        }

        return false;
    }

    public void ReturnEntity(PuzzleEntityType type)
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
}
