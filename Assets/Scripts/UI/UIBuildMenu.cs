using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class UIBuildMenu : MonoBehaviour
{
    public RectTransform layoutGroup;
    public PuzzleEditor editor;
    public UIBuildItem buildItemPrefab;
    public SpriteRegistry spriteRegistry;

    private readonly Dictionary<BuildEntityType, (NumberBox, UIBuildItem)> entityData = new();
    private UIBuildItem eraserItem;

    private BuildEntityType selectedType;
    private int selectedTypeIndex;

    private bool isEditMode = false;

    private void Update()
    {
        if (Input.mouseScrollDelta.y != 0)
        {
            IncreaseIndex(Input.mouseScrollDelta.y > 0 ? 1 : -1);
        }
    }

    public void SetEditMode()
    {
        var editableEntities = new Dictionary<BuildEntityType, int?>();
        foreach (var type in BuildEntityTypeRef.GetAllTypes())
        {
            if (type != BuildEntityType.Eraser) editableEntities.Add(type, 0);
        }
        isEditMode = true;
        SetData(editableEntities);
    }

    public Dictionary<BuildEntityType, int?> GetCurrentBuildEntityCounts()
    {
        var editableEntities = new Dictionary<BuildEntityType, int?>();
        foreach (var (type, (val, _)) in entityData)
        {
            if (type != BuildEntityType.Eraser && val.number != 0) editableEntities.Add(type, val.number);
        }
        return editableEntities;
    }

    public void SetData(Dictionary<BuildEntityType, int?> editableEntities)
    {
        eraserItem = Instantiate(buildItemPrefab, layoutGroup);

        eraserItem.SetType(BuildEntityType.Eraser);
        eraserItem.SetOnClick(OnSelect);

        eraserItem.SetNumber(null, false);
        eraserItem.SetSelected(true);
        eraserItem.SetOnEditButton(null);

        entityData[BuildEntityType.Eraser] = (new NumberBox(null), eraserItem);

        foreach (var (buildEntityType, number) in editableEntities)
        {
            var item = Instantiate(buildItemPrefab, layoutGroup);

            item.SetType(buildEntityType);
            item.SetOnClick(OnSelect);

            item.SetNumber(number);
            item.SetSelected(false);
            item.SetOnEditButton(isEditMode ? OnEditButton : null);

            entityData[buildEntityType] = (new NumberBox(number), item);
        }

        selectedType = BuildEntityType.Eraser;
    }

    private void OnEditButton(BuildEntityType type, bool increase)
    {
        if (!isEditMode) return;

        var (data, item) = entityData[type];
        if (increase)
        {
            if (data.number == null) data.number = 0;
            else data.number++;
        }
        else
        {
            if (data.number == 0) data.number = null;
            else data.number--;
        }
        item.SetNumber(data.number);
    }

    private void IncreaseIndex(int delta)
    {
        var m = BuildEntityTypeRef.GetEntityTypes(selectedType).Count;
        selectedTypeIndex = (((selectedTypeIndex + delta) % m) + m) % m;
        entityData[selectedType].Item2.SetIndex(selectedTypeIndex);
        editor.SetSelectedType(BuildEntityTypeRef.GetEntityTypes(selectedType)[selectedTypeIndex]);
    }

    private void OnSelect(BuildEntityType type)
    {
        if (selectedType == type)
        {
            IncreaseIndex(1);
        }
        else
        {
            var curSelected = entityData[selectedType].Item2;
            selectedType = type;
            selectedTypeIndex = 0;
            curSelected.SetSelected(false);
            curSelected.SetIndex(0);

            curSelected = entityData[selectedType].Item2;
            curSelected.SetSelected(true);
            editor.SetSelectedType(BuildEntityTypeRef.GetEntityTypes(selectedType)[selectedTypeIndex]);
        }
    }

    public bool CanConsumeEntity(EntityType type)
    {
        if (isEditMode) return true;

        var buildType = BuildEntityTypeRef.GetBuildType(type);
        if (entityData.TryGetValue(buildType, out var data))
        {
            return data.Item1.number != 0;
        }

        return false;
    }

    public void ConsumeEntity(EntityType type)
    {
        if (isEditMode) return;

        var buildType = BuildEntityTypeRef.GetBuildType(type);
        if (entityData.TryGetValue(buildType, out var data))
        {
            if (data.Item1.number == 0) throw new System.InvalidOperationException();
            if (data.Item1.number != null) data.Item1.number--;
            data.Item2.SetNumber(data.Item1.number);
        }
    }

    public void ReturnEntity(EntityType type)
    {
        if (isEditMode) return;

        var buildType = BuildEntityTypeRef.GetBuildType(type);
        if (entityData.TryGetValue(buildType, out var data))
        {
            if (data.Item1.number != null) data.Item1.number++;
            data.Item2.SetNumber(data.Item1.number);
        }
    }

    public class NumberBox
    {
        public int? number;

        public NumberBox(int? number)
        {
            this.number = number;
        }
    }
}
