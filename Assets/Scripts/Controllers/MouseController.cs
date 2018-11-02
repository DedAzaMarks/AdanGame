using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class MouseController : MonoBehaviour
{

    public GameObject CircleCursorPrefab;

    // The world-position of the mouse last frame.
    Vector3 LastFramePosition;
    Vector3 CurrFramePosition;

    // The world-position start of our left-mouse drag operation
    Vector3 dragStartPosition;
    List<GameObject> dragPreviewGameObjects;


    enum MouseMode {
		SELECT,
		BUILD
	}
	MouseMode currentMode = MouseMode.SELECT;
    // Use this for initialization
    void Start()
    {
        dragPreviewGameObjects = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        CurrFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CurrFramePosition.z = 0;

        //UpdateCursor();
        UpdateDragging();
        UpdateCameraMovement();

        // Save the mouse position from this frame
        // We don't use currFramePosition because we may have moved the camera.
        LastFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        LastFramePosition.z = 0;
    }

    void UpdateDragging()
    {
        // If we're over a UI element, then bail out from this.
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Start Drag
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPosition = CurrFramePosition;
        }

        int StartX = Mathf.FloorToInt(dragStartPosition.x);
        int EndX = Mathf.FloorToInt(CurrFramePosition.x);
        int StartY = Mathf.FloorToInt(dragStartPosition.y);
        int EndY = Mathf.FloorToInt(CurrFramePosition.y);

        // We may be dragging in the "wrong" direction, so flip things if needed.
        if (EndX < StartX)
        {
            int tmp = EndX;
            EndX = StartX;
            StartX = tmp;
        }
        if (EndY < StartY)
        {
            int tmp = EndY;
            EndY = StartY;
            StartY = tmp;
        }

        // Clean up old drag previews
        while (dragPreviewGameObjects.Count > 0)
        {
            GameObject go = dragPreviewGameObjects[0];
            dragPreviewGameObjects.RemoveAt(0);
            SimplePool.Despawn(go);
        }

        if (Input.GetMouseButton(0))
        {
            // Display a preview of the drag area
            for (int x = StartX; x <= EndX; x++)
            {
                for (int y = StartY; y <= EndY; y++)
                {
                    Tile t = WorldController.Instance.World.GetTileAt(x, y);
                    if (t != null)
                    {
                        // Display the building hint on top of this tile position
                        GameObject GO = SimplePool.Spawn(CircleCursorPrefab, new Vector3(x, y, 0), Quaternion.identity);
                        GO.transform.SetParent(this.transform, true);
                        dragPreviewGameObjects.Add(GO);
                    }
                }
            }
        }

        // End Drag
        if (Input.GetMouseButtonUp(0))
        {

            BuildModeController BMC = GameObject.FindObjectOfType<BuildModeController>();

            // Loop through all the tiles
            for (int x = StartX; x <= EndX; x++)
            {
                for (int y = StartY; y <= EndY; y++)
                {
                    Tile t = WorldController.Instance.World.GetTileAt(x, y);

                    if (t != null)
                    {
                        // Call BuildModeController::DoBuild()
                        BMC.DoBuild(t);
                    }
                }
            }
        }
    }

    void UpdateCameraMovement()
    {
        // Handle screen panning
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {   // Right or Middle Mouse Button

            Vector3 diff = LastFramePosition - CurrFramePosition;
            Camera.main.transform.Translate(diff);

        }

        Camera.main.orthographicSize = Camera.main.orthographicSize - (Camera.main.orthographicSize * Input.GetAxis("Mouse ScrollWheel"));

        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, 3f, 25f);
    }

    public void StartBuildMode() {
		currentMode = MouseMode.BUILD;
	}
}
