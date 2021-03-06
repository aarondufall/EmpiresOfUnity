﻿using UnityEngine;
using System.Collections.Generic;

[AddComponentMenu("Camera-Control/GUIScript")]
public class GUIScript : MonoBehaviour
{
    public static GUIScript main;

    public static MiniMapControll MiniMap;
    public static UnitGroup SelectedGroup;
    public int GroupCount;
    public RightClickMenu RightclickGUI;

    private SelectorScript SelectionSprite;
    private GroupRectangleScript GroupSprite;

    public GUITexture SellectionGUITexture;
    public GameObject lastClickedUnit;
 
    //public static bool DebugText = false;

    //private Scrolling scrolling;

    new public Camera camera;
 //   public MiniMapControll miniMapControll;

    public Rect MapViewArea;
    public Rect MainGuiArea;
    public Rect MiniMapArea
    {
        get { return MiniMap.Area; }
    }

    public GUIText secondGUIText;
    private Vector2? mousePosition;
    public Vector2 MousePosition
    {
        get 
        {
            if (mousePosition == null)
                return (mousePosition = (Vector2)MouseEvents.State.Position).Value;
            return mousePosition.Value;
        }
    }

    public static Vector2 ScreenSize = new Vector2(Screen.width, Screen.height);

    public Vector2 Scale;

    public List<GUIContent> MainGuiContent;

    /* Lifebar Prefab */
    public Transform PrefabLifebar;

    /* Selection 3D Rectangle */
    private Rect selectionRectangle;
    public bool snapingAlowed = false;
    public Rect SelectionRectangle
    {
        get { return selectionRectangle; }
        set
        {
            selectionRectangle = value;

            // TODO Width & Height isn't the size. The width/height position a value in world coordinates!
            snapingAlowed = ((selectionRectangle.width >= 10) || (selectionRectangle.height >= 10) || (selectionRectangle.width <= -10) || (selectionRectangle.height <= -10));

            Vector3 w1;
            /* Get Mouse Position At Start (Left/Top Corner of Rect) */
            if (value.width == 0 && value.height == 0)
            {
                w1 = MouseEvents.State.Position.AsWorldPointOnMap;
                w1.y = SelectionSprite.transform.position.y;
                SelectionSprite.transform.position = w1;
            }
            else
            {
                w1 = SelectionSprite.transform.position;
            }
            /* Get Mouse Position At End (Width/Height of Rect) */
            Vector3 w2 = MouseEvents.State.Position.AsWorldPointOnMap;
            Vector3 localScale = new Vector3((w2.x - w1.x), -(w2.z - w1.z), 1);
            SelectionSprite.transform.localScale = localScale;
        }
    }

    private bool UnitFocused
    {
        get 
        {
            if (Focus.masterGameObject)
                return (bool)Focus.masterGameObject.GetComponent<Focus>();
            return false;
        }
    }

    private bool UnitMenuIsOn
    {
        get { return RightClickMenu.showCommandPannel; }
        set { RightClickMenu.showCommandPannel = value; }
    }

    void Awake()
    {
        main = this.gameObject.GetComponent<GUIScript>();
    }

    void Start()
    {
        foreach (GameObject rectangle in GameObject.FindGameObjectsWithTag("Rectangles"))
        {
            //if (rectangle.gameObject.name == "FocusRectangle")
                //FocusSprite = rectangle.GetComponent<FocusRectangleObject>();
            if (rectangle.gameObject.name == "SelectionRectangle")
                SelectionSprite = rectangle.GetComponent<SelectorScript>();
            else if (rectangle.gameObject.name == "GroupRectangle")
                GroupSprite = rectangle.GetComponent<GroupRectangleScript>();
        }
        
        MouseEvents.Setup(gameObject);

        SelectedGroup = ScriptableObject.CreateInstance<UnitGroup>();
        SelectedGroup.startGroup();

        //scrolling = (GetComponent<Scrolling>()) ? GetComponent<Scrolling>() : null;
        
        //if (camera.name == null)
        //{
        camera = Camera.main;
        //}
        Scale = new Vector2((camera.pixelRect.width / gameObject.guiTexture.texture.width), (camera.pixelRect.height / gameObject.guiTexture.texture.height));


        //gameObject.guiTexture.pixelInset = new Rect(0, -camera.pixelHeight, camera.pixelWidth, camera.pixelHeight);
        //if (gameObject.GetComponent<GUIText>() == null) gameObject.AddComponent<GUIText>();

        gameObject.guiText.pixelOffset = new Vector2(-Camera.main.pixelWidth / 2 + 25 * Scale.x, Camera.main.pixelHeight/2 - 80 * Scale.y);
        MapViewArea = new Rect(20 * Scale.x, 20 * Scale.y, 1675 * Scale.x, 1047 * Scale.y);
        MainGuiArea = new Rect(1716 * Scale.x, 20 * Scale.y, 184 * Scale.x, 1047 * Scale.y);
        gameObject.guiText.fontSize = (int)(40f * Scale.x + 0.5f);

        //guiTexture.pixelInset = MapViewArea;
        //guiTexture.guiTexture.border.left = (int)(20f * Scale.x);
        //guiTexture.guiTexture.border.right = (int)(225f * Scale.x);
        //guiTexture.guiTexture.border.top = (int)(20f * Scale.y);
        //guiTexture.guiTexture.border.bottom = (int)(13f * Scale.y);


        //QamAcsess = Camera.main.GetComponent<camScript>();

        MouseEvents.LEFTCLICK += MouseEvents_LEFTCLICK;
        MouseEvents.RIGHTCLICK += MouseEvents_RIGHTCLICK;
        MouseEvents.LEFTRELEASE += MouseEvents_LEFTRELEASE;

        UpdateManager.GUIUPDATE += UpdateManager_GUIUPDATE;

        
    }

    private void UpdateManager_GUIUPDATE()
    {
        mousePosition = null;
        CheckKeyboardInput();
        GroupCount = SelectedGroup.Count;

        UpdateRectangles();

    }

    private void UpdateRectangles()
    {
        //FocusSprite.DoUpdate();
        GroupSprite.DoUpdate();
    }
    
    void MouseEvents_LEFTCLICK(Ray qamRay, bool hold)
    {
        
        if (MouseEvents.State.Position.IsOverMainMapArea)
        {
            if (hold)
            {
                SelectionRectangle = new Rect(SelectionRectangle.x, SelectionRectangle.y, MousePosition.x - SelectionRectangle.x, SelectionRectangle.y - MousePosition.y);
            }
            else
            {
                SelectionRectangle = new Rect(MousePosition.x, MousePosition.y, 0, 0);
                FocusUnit();
            }
        }
        else if (MouseEvents.State.Position.IsOverMiniMapArea)
        {
            if (!hold)
            {
                FocusUnit();
            }
        }
        //else if (MiniMapArea.Contains(MousePosition))
        //{
        //    RaycastHit groundclick;
        //    if (Ground.Current.collider.Raycast(MiniMap.camera.ScreenPointToRay(MousePosition), out groundclick, 400))
        //    {

        //    }
        //}
    }

    /* if the SlectionRectangle was drawn over 10 (x or y) it will create a UnitGroup from the selection */
    void MouseEvents_LEFTRELEASE()
    {
        if (snapingAlowed)
        {
            SnapSelectionRectangle();
            snapingAlowed = false;
        }
    }

    void MouseEvents_RIGHTCLICK(Ray qamRay, bool hold)
    {
        if (!hold)
        {
            if (MouseEvents.State.Position.IsOverMainMapArea)
            {
                /* Try Focus the unit thats clicked, or release Focus if clicked on ground...*/
                if (!FocusUnit())
                {
                    if (MouseEvents.State.Position.AsObjectUnderCursor.layer == (int)EnumProvider.LAYERNAMES.Ignore_Raycast
                    && !Focus.IsLocked)
                    {
                        if (UnitFocused)
                            Component.Destroy(Focus.masterGameObject.GetComponent<Focus>());
                        if (SelectedGroup)
                        {
                            SelectedGroup.ResetGroup();
                            //ScriptableObject.DestroyObject(SelectedGroup);
                            //SelectedGroup = ScriptableObject.CreateInstance<UnitGroup>();
                        }

                    }
                }
            }
            else if (MouseEvents.State.Position.IsOverMiniMapArea)
            {

                Vector3 buffer = MouseEvents.State.Position;
                buffer.y = Camera.main.transform.position.y;
                Camera.main.transform.position=buffer;

            }
        }
    }

    private bool FocusUnit()
    {
        if(MouseEvents.State.Position.AsUnitUnderCursor)
        {
            if (!UnitFocused && GroupCount == 0)
            {
                if (SelectedGroup)
                    SelectedGroup.ResetGroup();
                //    ScriptableObject.DestroyObject(SelectedGroup);
                //SelectedGroup = ScriptableObject.CreateInstance<UnitGroup>();
                MouseEvents.State.Position.AsObjectUnderCursor.AddComponent<Focus>();
                //   UnitUnderCursor.gameObject.AddComponent<Focus>();
                //UnitUnderCursor.UNIT.ShowLifebar();
                return true;
            }
            else if ((GroupCount > 0) && !MouseEvents.State.Position.AsUnitUnderCursor.IsEnemy(SelectedGroup.GoodOrEvil))
                SelectedGroup.ResetGroup();   
        }
        return false;
    }

    // Selection finished -> Now Select the Units inside the Area
    private void SnapSelectionRectangle()
    {
        // Get group of selected elements
        SelectedGroup = SelectionSprite.GetComponent<SelectorScript>().SnapSelection();

        // if group has 1 unit -> do single focus
        if (SelectedGroup.Count == 1)
        {
            GameObject unit = SelectedGroup[0];
            SelectedGroup.ResetGroup();
            unit.AddComponent<Focus>();
            //unit.GetComponent<UnitScript>().ShowLifebar();
        }
        else
        {
            // Activate Lifebar at all selected units
            for (int i = 0; i < SelectedGroup.Count; i++)
                SelectedGroup[i].GetComponent<UnitScript>().ShowLifebar();
        }

        // Hide selection rectangle
        SelectionRectangle = new Rect(SelectionRectangle.x, SelectionRectangle.y, 0f, 0f);
    }

    //void OnGUI()
    //{

    //    GUI.BeginGroup(new Rect((1718*Scale.x ), (24*Scale.y ), (180 * Scale.x), (160 * Scale.y)));
    //    if (GUI.Button(new Rect((0*Scale.x), (0*Scale.y), (180*Scale.x), (40*Scale.y)), MainGuiContent[0]) || Input.GetKeyDown(KeyCode.Escape))
    //    {
    //        Application.LoadLevel("MainMenu");
    //    }
        
    //    if (GUI.Button(new Rect((0 * Scale.x), (60 * Scale.y), (80 * Scale.x), (40 * Scale.y)), MainGuiContent[1])) 
    //    {
    //        scrolling.SwitchScrollingStatus();
    //    }
    //    if (GUI.Button(new Rect((100 * Scale.x), (60 * Scale.y), (80 * Scale.x), (40 * Scale.y)), MainGuiContent[2])) 
    //    {
    //        MiniMap.SwitchActive();
    //    }

    //    if (GUI.Button(new Rect((0 * Scale.x), (120 * Scale.y), (47 * Scale.x), (40 * Scale.y)), MainGuiContent[3]))
    //    {
    //        Ground.Switch(0);
    //    }
    //    if (GUI.Button(new Rect((68 * Scale.x), (120 * Scale.y), (47 * Scale.x), (40 * Scale.y)), MainGuiContent[4]))
    //    {
    //        Ground.Switch(1);
    //    }
    //    if (GUI.Button(new Rect((134 * Scale.x), (120 * Scale.y), (47 * Scale.x), (40 * Scale.y)), MainGuiContent[5]))
    //    {
    //        Ground.Switch(2);
    //    }
    //    GUI.enabled = true;
    //    GUI.EndGroup();

    //}

    private void CheckKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Ground.Switch(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Ground.Switch(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Ground.Switch(2);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            MiniMap.SwitchActive();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (InGameText.ShowDebugText)
            {
                InGameText.ShowDebugText = false;
                guiText.text = "";
            }
            else
            {
                InGameText.ShowDebugText = true;
            }
        }
        /* Space Key Switch Camera */
        if (Input.GetKeyDown(KeyCode.Space))
            Camera.main.GetComponent<Cam>().SwitchCam();
    }



}
