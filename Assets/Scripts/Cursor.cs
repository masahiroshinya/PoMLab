using UnityEngine;
using System.Collections;

// for dictionary
using System.Collections.Generic;

public class Cursor : MonoBehaviour {

    // instance of GameObject and C# class to get acceleration data
    // （　TiltMangerオブジェクトから加速度データを取得するためのインスタンス　）
    public GameObject tiltManagerObj;
    public TiltManager tiltManager;
    // parameters (set as public just for debug)
    public float cursorSize;
    public float offsetTiltX;
    public float offsetTiltY;
    public float cursorGainX;
    public float cursorGainY;
    public float visuoMotorRotationAngle;
    public bool isMirrorReversed;
    public float mirrorReversalAxisDirection;
    public bool isClamped;
    public float clampDirection;
    public float maxRadius;

    // save
    public bool isRecording;
    public List<float> timeStampRecord;
    public List<Vector3> cursorPositionRecord;


    // Use this for initialization
    void Start()
    {
        
        // get instance of TiltManager
        tiltManagerObj = GameObject.Find("Tilt Manager");
        tiltManager = tiltManagerObj.GetComponent<TiltManager>();

        float size = cursorSize;
        transform.localScale = Vector3.one * cursorSize;


    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = Vector3.one * cursorSize;

        // get acceleration data from Test_TiltManager script
        // （　Test_TiltManger のメソッドを使って加速度データを取得　）
        bool isFiltered = true;
        Vector3 acc = tiltManager.getAcceleration(isFiltered);

        // translate tilt into cursor position
        // （　Test_TiltMangerから取得したデータを、Cursorの位置に反映させる　）
        Vector3 cursorPosition = Acc2CursorPosition(acc);
        transform.position = cursorPosition;

        // save data
        if (isRecording)
        {
            float timeStamp = tiltManager.getTimeStamp();
            timeStampRecord.Add(timeStamp);
            cursorPositionRecord.Add(cursorPosition);
        }

    }


    // Translate acceleration into cursor position
    public Vector3 Acc2CursorPosition(Vector3 acc)
    {

        // acceleration to tilt angle
        Vector3 tilt1 = new Vector3(Mathf.Asin(acc.x) * Mathf.Rad2Deg, Mathf.Asin(acc.y) * Mathf.Rad2Deg, 0);

        // offset
        Vector3 tilt2 = new Vector3(tilt1.x - offsetTiltX, tilt1.y - offsetTiltY, tilt1.z);

        // gain
        Vector3 pos1 = new Vector3(tilt2.x * cursorGainX * 1 / 90, tilt2.y * cursorGainY * 1 / 90, tilt2.z);

        // visuo-motor rotation
        Vector3 pos2 = RotateInXYPlane(pos1, visuoMotorRotationAngle);

        // mirror reversal
        Vector3 pos = pos2;
        if (isMirrorReversed)
        {
            pos = RotateInXYPlane(pos, -mirrorReversalAxisDirection);
            pos = new Vector3(pos.x, -pos.y, pos.z);
            pos = RotateInXYPlane(pos, mirrorReversalAxisDirection);
        }

        // cursor clamp
        if (isClamped)
        {
            Vector3 clampVec = new Vector3(Mathf.Cos(clampDirection * Mathf.Deg2Rad), Mathf.Sin(clampDirection * Mathf.Deg2Rad), 0);
            pos = clampVec * Vector3.Dot(pos, clampVec);
        }

        // maximum radius
        pos = pos.normalized * Mathf.Min(pos.magnitude, maxRadius);

        // output
        return pos;
    }

    // rotation in XY plane. z value will not be changed
    private Vector3 RotateInXYPlane(Vector3 vec3, float theta)
    {
        float thetaRad = theta * Mathf.Deg2Rad;
        float xx = vec3.x * Mathf.Cos(thetaRad) - vec3.y * Mathf.Sin(thetaRad);
        float yy = vec3.x * Mathf.Sin(thetaRad) + vec3.y * Mathf.Cos(thetaRad);
        return new Vector3(xx, yy, vec3.z);
    }


    // public parameter setter
    public void SetCursorParameters(Dictionary<string, string> expPrm)
    {
        cursorSize = float.Parse(expPrm["Cursor Size"]);
        transform.localScale = new Vector3(cursorSize, cursorSize, cursorSize);

        offsetTiltX = float.Parse(expPrm["Offset Tilt X"]);
        offsetTiltY = float.Parse(expPrm["Offset Tilt Y"]);
        cursorGainX = float.Parse(expPrm["Cursor Gain X"]);
        cursorGainY = float.Parse(expPrm["Cursor Gain Y"]);
        visuoMotorRotationAngle = float.Parse(expPrm["Visuo Motor Rotation Angle"]);
        isMirrorReversed = bool.Parse(expPrm["Is Mirror Reversed"]);
        mirrorReversalAxisDirection = float.Parse(expPrm["Mirror Reversal Axis Direction"]);
        isClamped = bool.Parse(expPrm["Is Clamped"]);
        clampDirection = float.Parse(expPrm["Clamp Direction"]);
        maxRadius = float.Parse(expPrm["Max Radius"]);
    }


    // control data recording 
    public void startRecording()
    {
        timeStampRecord.Clear();
        cursorPositionRecord.Clear();
        isRecording = true;
    }
    public void stopRecording()
    {
        isRecording = false;
    }

    // getter
    public List<float> getTimeStampRecord()
    {
        return timeStampRecord;
    }
    public List<Vector3> getCursorPositionRecord()
    {
        return cursorPositionRecord;
    }


}
