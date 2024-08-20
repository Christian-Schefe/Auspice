using System.Collections.Generic;
using System.Linq;
using TMPro;
using Tweenables;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleEditor : MonoBehaviour
{
    public LevelRegistry levelRegistry;

    public LevelVisuals visuals;
    public UIBuildMenu buildMenu;
    public UIRunMenu runMenu;
    public Main main;

    public TextMeshProUGUI impossibleText;
    public List<TMP_InputField> starThresholdInputs;
    public GameObject starThresholdContainer;
    public GameObject sizeButtonContainer;
    public Button incWidth, incHeight, decWidth, decHeight;
    private Vector2Int size = new(10, 10);

    private EntityType selectedType;

    private PuzzleData data;
    private HashSet<Vector2Int> usedPositions;

    private Vector2Int? lastPortalPosition;

    private bool isEditMode;

    private void Awake()
    {
        var selectedLevelIndex = main.GetSelectedLevelIndex();
        if (selectedLevelIndex is not int index)
        {
            Init(LevelGenerator.GenerateData(new(10, 10)));
            buildMenu.SetEditMode();
            isEditMode = true;
        }
        else
        {
            Init(levelRegistry.GetPuzzleDataInstance(index));
            buildMenu.SetData(data.buildableEntityCounts);
            isEditMode = false;
        }

        foreach (var input in starThresholdInputs)
        {
            if (isEditMode) input.onValueChanged.AddListener((value) => UpdateEditedStarThresholds());
        }
        starThresholdContainer.SetActive(isEditMode);
        sizeButtonContainer.SetActive(isEditMode);

        if (isEditMode) UpdateEditedStarThresholds();
        else runMenu.SetStepBounds(data.starTresholds);

        impossibleText.gameObject.SetActive(false);

        if (isEditMode)
        {
            incWidth.onClick.AddListener(() => ChangeSize(new Vector2Int(1, 0)));
            incHeight.onClick.AddListener(() => ChangeSize(new Vector2Int(0, 1)));
            decWidth.onClick.AddListener(() => ChangeSize(new Vector2Int(-1, 0)));
            decHeight.onClick.AddListener(() => ChangeSize(new Vector2Int(0, -1)));
        }
    }

    private void Init(PuzzleData data)
    {
        this.data = data;
        usedPositions = data.GetUsedPositions();
        visuals.SetData(data);
    }

    private void ChangeSize(Vector2Int delta)
    {
        if (!isEditMode) return;
        size += delta;
        size.Clamp(new Vector2Int(5, 5), new Vector2Int(20, 20));
        visuals.ClearAll();
        Init(LevelGenerator.GenerateData(size));
    }

    private void UpdateEditedStarThresholds()
    {
        if (!isEditMode) return;
        var thresholds = GetEditedStarThresholds();
        runMenu.SetStepBounds(thresholds);
    }

    public bool IsEditMode() => isEditMode;

    public List<int> GetEditedStarThresholds()
    {
        var thresholds = new List<int>();
        foreach (var input in starThresholdInputs)
        {
            if (string.IsNullOrEmpty(input.text)) thresholds.Add(0);
            else if (int.TryParse(input.text, out var threshold))
            {
                thresholds.Add(threshold);
            }
            else
            {
                throw new System.Exception("Invalid star threshold input");
            }
        }
        thresholds.Sort();
        return thresholds;
    }

    public PuzzleData GetEditedPuzzleDataClone()
    {
        var clone = data.Clone(true);
        clone.SetBuildableEntityCounts(buildMenu.GetCurrentBuildEntityCounts());
        clone.SetStarThresholds(GetEditedStarThresholds());
        return clone;
    }

    public Puzzle BuildPuzzle()
    {
        return new Puzzle(data);
    }

    public void ShowImpossible()
    {
        impossibleText.gameObject.SetActive(true);
        var runner = impossibleText.TweenScale().From(Vector3.zero).To(Vector3.one).Duration(0.3f).Ease(Easing.CubicOut).RunNew();
        impossibleText.TweenScale().Delay(2f).From(Vector3.one).To(Vector3.zero).Duration(0.3f).Ease(Easing.CubicIn).OnFinally(() =>
        {
            impossibleText.gameObject.SetActive(false);
        }).RunQueued(ref runner);
    }

    public void SetSelectedType(EntityType type)
    {
        selectedType = type;
    }

    private void UpdateInteractability()
    {
        bool interactable = main.CurrentState == MainState.Editing && !main.IsPaused && data != null;
        foreach (var field in starThresholdInputs) field.interactable = interactable;
        incWidth.interactable = incHeight.interactable = decWidth.interactable = decHeight.interactable = interactable;
    }

    private void Update()
    {
        UpdateInteractability();

        if (main.CurrentState != MainState.Editing || main.IsPaused || data == null)
        {
            visuals.PreviewEntity(Vector2Int.zero, selectedType, false);
            return;
        }

        var cellPosition = visuals.MouseCellPos();
        if (!data.positions.Contains(cellPosition))
        {
            visuals.PreviewEntity(cellPosition, selectedType, false);
            return;
        }

        if (Input.GetMouseButton(1))
        {
            OnPlace(cellPosition, EntityType.None);
            visuals.PreviewEntity(cellPosition, EntityType.None, true);
            return;
        }

        if (Input.GetMouseButton(0))
        {
            OnPlace(cellPosition, selectedType);
        }

        bool showPreview = !usedPositions.Contains(cellPosition) || selectedType == EntityType.None;
        visuals.PreviewEntity(cellPosition, selectedType, showPreview);
    }

    private void OnPlace(Vector2Int position, EntityType type)
    {
        if (type.basicType == PuzzleEntityType.None)
        {
            RemoveAt(position);
            return;
        }

        if (type.basicType == PuzzleEntityType.Portal)
        {
            if (lastPortalPosition is Vector2Int portalPos && portalPos != position)
            {
                if (TryAddPortalPair(portalPos, position)) lastPortalPosition = null;
                else lastPortalPosition = position;
            }
            else
            {
                lastPortalPosition = position;
            }
            return;
        }
        lastPortalPosition = null;

        PuzzleEntity entity = type.basicType switch
        {
            PuzzleEntityType.Player => new PlayerEntity(type.playerType, position),
            PuzzleEntityType.Button => new ButtonEntity(type.buttonColor, position),
            PuzzleEntityType.PressurePlate => new PressurePlateEntity(type.buttonColor, position),
            PuzzleEntityType.Portal => throw new System.Exception("Unreachable"),
            _ => new GenericEntity(position, type)
        };

        TryAddEntity(entity);
    }

    private bool TryAddEntity(PuzzleEntity entity)
    {
        var position = entity.position;
        var type = entity.GetEntityType();

        if (usedPositions.Contains(position)) return false;
        if (!buildMenu.CanConsumeEntity(type)) return false;

        if (!data.CanAddEntity(entity)) return false;

        data.AddEntity(entity);
        visuals.AddEntity(entity, true);

        usedPositions.Add(position);
        buildMenu.ConsumeEntity(type);

        return true;
    }

    private bool TryAddPortalPair(Vector2Int pos1, Vector2Int pos2)
    {
        if (usedPositions.Contains(pos1) || usedPositions.Contains(pos2)) return false;
        if (!buildMenu.CanConsumeEntity(EntityType.Portal)) return false;

        var portal1 = new PortalEntity(pos1, pos2);
        var portal2 = new PortalEntity(pos2, pos1);

        if (!data.CanAddEntity(portal1) || !data.CanAddEntity(portal2)) return false;

        data.AddEntity(portal1);
        data.AddEntity(portal2);

        visuals.AddEntity(portal1, true);
        visuals.AddEntity(portal2, false);

        usedPositions.Add(pos1);
        usedPositions.Add(pos2);

        buildMenu.ConsumeEntity(EntityType.Portal);

        return true;
    }

    private void RemoveAt(Vector2Int position)
    {
        var removedEntities = data.Remove(position);
        if (removedEntities.Count == 0) return;

        if (position == lastPortalPosition)
        {
            lastPortalPosition = null;
        }

        visuals.Remove(position);
        usedPositions.Remove(position);

        foreach (var removedEntity in removedEntities)
        {
            buildMenu.ReturnEntity(removedEntity.GetEntityType());

            if (removedEntity is PortalEntity portal && portal.destination is Vector2Int otherPortalPos)
            {
                data.RemoveEntity(otherPortalPos, EntityType.Portal);
                visuals.RemoveEntity(otherPortalPos, EntityType.Portal);
                usedPositions.Remove(otherPortalPos);
            }
        }
    }
}
