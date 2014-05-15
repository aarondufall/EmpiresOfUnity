﻿using UnityEngine;
using System.Collections;

public class Cam : MonoBehaviour {

    public enum CAMERAMODE : byte
    {
        ORTHOGRAFIC,
        PERSPECTIVE,
        FIRSTPERSON,
    }

    public GUIScript mainGUI;
    public float ORTHOGRAFIC_Y_HEIGHT = 75f;
    public float PERSPECTIVE_VIEW_SIZE = 25f;
    public float Orthografic_View_Size = 500f;
    public float Orthografic_Y_MaxHeight = 400f;

    private Quaternion Perspective_Rotation;
    private Quaternion Orthografic_Rotation;
    private float Perspective_Y_Height = 27f;
    private float Perspective_Y_MaxHeight = 400f;

    private CAMERAMODE camMode = CAMERAMODE.ORTHOGRAFIC;

    private CAMERAMODE CamMode
    {
        get { return camMode; }
        set
        {
            if (camMode != value)
                switch (value)
                {
                    case CAMERAMODE.ORTHOGRAFIC:
                        if (!gameObject.camera.isOrthoGraphic) {
                            Perspective_Y_Height = gameObject.transform.position.y;
                            Perspective_Rotation = gameObject.transform.rotation;
                        }
                        gameObject.camera.enabled = false;
                        gameObject.transform.position = new Vector3(gameObject.transform.position.x, ORTHOGRAFIC_Y_HEIGHT, gameObject.transform.position.z + 50);
                        gameObject.transform.rotation = Orthografic_Rotation;
                        gameObject.camera.orthographic = true;
                        gameObject.camera.orthographicSize = Orthografic_View_Size;
                        gameObject.camera.enabled = true;
                        break;

                    case CAMERAMODE.PERSPECTIVE:
                        if (gameObject.camera.isOrthoGraphic) {
                            Orthografic_View_Size = gameObject.camera.orthographicSize;
                            Orthografic_Rotation = gameObject.transform.rotation;
                        }
                        gameObject.camera.enabled = false;
                        gameObject.camera.orthographic = false;
                        gameObject.camera.fieldOfView = PERSPECTIVE_VIEW_SIZE;

                        gameObject.transform.position = new Vector3(gameObject.transform.position.x, Perspective_Y_Height, gameObject.transform.position.z - 50);

                        gameObject.transform.rotation = Perspective_Rotation;
                        gameObject.camera.enabled = true;
                        break;
                    case CAMERAMODE.FIRSTPERSON:
                        break;
                }
            camMode = value;
        }
    }
    public CAMERAMODE GetCamMode()
    {
        return this.CamMode;
    }

	void Start ()
    {
        MouseEvents.MOUSEWHEEL += MouseEvents_MQUSEWHEEL;

        Perspective_Rotation = new Quaternion(0.258819f, 0f, 0f, 0.9659258f);
        Orthografic_Rotation = new Quaternion(0.7071068f, 0f, 0f, 0.7071068f);

        this.CamMode = CAMERAMODE.PERSPECTIVE;
	}
     
    public void MouseEvents_MQUSEWHEEL(MouseEvents.MOUSEWHEELSTATE wheelstate)
    {
        if (CamMode==CAMERAMODE.ORTHOGRAFIC)
        {
            gameObject.camera.orthographicSize -= (int)wheelstate * (camera.orthographicSize / 120f);

            Vector3 buffer = gameObject.transform.position;
            buffer.y -= (int)wheelstate * (gameObject.transform.position.y / 120f);
            if (buffer.y > Orthografic_Y_MaxHeight)
            {
                buffer.y = Orthografic_Y_MaxHeight;
            }
            gameObject.transform.position = buffer;
        }
        else if (CamMode == CAMERAMODE.PERSPECTIVE)
        {
            Vector3 buffer = gameObject.transform.position;
            buffer.y -= (int)wheelstate * (gameObject.transform.position.y / 120f);
            if (buffer.y > Perspective_Y_MaxHeight)
            {
                buffer.y = Perspective_Y_MaxHeight;
            }
            Ray centerRay = camera.ScreenPointToRay(new Vector3(camera.pixelWidth / 2f, camera.pixelHeight / 2f, 0f));
            RaycastHit groundHit;
            Ground.Current.collider.Raycast(centerRay, out groundHit, camera.farClipPlane);
            gameObject.transform.position = buffer;

            Vector3 newDirection = (groundHit.point - gameObject.transform.position).normalized;
            gameObject.transform.forward = newDirection;
        }
    }

    public CAMERAMODE SwitchCam()
    {
        if (CamMode == CAMERAMODE.ORTHOGRAFIC)
            CamMode = CAMERAMODE.PERSPECTIVE;
        else if (CamMode == CAMERAMODE.PERSPECTIVE)
            CamMode = CAMERAMODE.ORTHOGRAFIC;
        return CamMode;
    }
}
