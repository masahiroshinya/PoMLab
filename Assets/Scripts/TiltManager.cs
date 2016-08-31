using UnityEngine;
using System.Collections;

using UnityEngine.UI;
// for List<T> （　List型を使うため　）
using System.Collections.Generic;

public class TiltManager : MonoBehaviour {

    // acceleration （加速度）
    private Vector3 rawAcceleraton;
    public Vector3 filtAcceleraton;

    // time　（タイムスタンプ等、時間関係）
    private float t0;         // 起動時の時刻
    private float timeStamp;  // （FixedUpdate内で）加速度が計測される時刻
    private float dt = 0.005f;         // サンプリングレート (FixedUpdateの間隔）
    public float alpha ; // 相補フィルタ係数

    // save
    public bool isRecording ;
    public List<float> timeStampRecord;
    public List<Vector3> rawAccRecord;
    public List<Vector3> filtAccRecord;




    // Use this for initialization　（　起動時の処理　）
    void Start()
    {
        
        // get initial values（　起動時の加速度・タイムスタンプを取得　）
        filtAcceleraton = Input.acceleration;
        t0 = Time.time;

    }
    

    // set sampling rate, filter parameter ( called by ExperimentalManager )
    public void SetConfiguration()
    {
        // set interval of FixedUpdate （　FixedUpateの呼び出し間隔の設定　加速度データの取得サンプリングレートに影響）
        // （　デバイスや処理内容によって、設定された間隔で動作しない可能性：　詳しくは未検証）
        // （　開発機 Nexus 9 では、200Hzで実行可能の模様　）
        dt = float.Parse(Config.instance.configParameters["Sampling Interval"]);
        Time.fixedDeltaTime = dt;
        alpha = float.Parse(Config.instance.configParameters["Filter Coefficient Alpha"]);

    }


    // physics-related measurement & calculation are called in FixedUpdate. not in Update
    // （　加速度の取得・フィルター等の物理関係の演算は、Updateではなく、間隔が固定されているFixedUpdateで行う　） 
    void FixedUpdate()
    {

        // get time stamp （　タイムスタンプの取得・シーン開始からの経過時間を計算　）
        float t = Time.time;
        timeStamp = t - t0;

        // get acceleration of the device　（ デバイスにかかる加速度を取得　）
        rawAcceleraton = Input.acceleration;

        // filter to the acc data　（ 加速度データを平滑化　）
        filtAcceleraton = alpha * filtAcceleraton + (1 - alpha) * rawAcceleraton;
        
        // save data
        if (isRecording)
        {
            timeStampRecord.Add(timeStamp);
            rawAccRecord.Add(rawAcceleraton);
            filtAccRecord.Add(filtAcceleraton);
        }
        
    }
    

    // enable other object to use acceleration data
    // （　他のオブジェクトが、加速度データを取得するためのメソッド　）
    public Vector3 getAcceleration(bool isFiltered)
    {

        if (isFiltered)
        {
            return filtAcceleraton;
        }
        else
        {
            return rawAcceleraton;
        }

    }


    // control data recording 
    public void startRecording()
    {
        timeStampRecord.Clear();
        rawAccRecord.Clear();
        filtAccRecord.Clear();
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
    public List<Vector3> getRawAccRecord()
    {
        return rawAccRecord;
    }
    public List<Vector3> getFiltAccRecord()
    {
        return filtAccRecord;
    }
    public float getTimeStamp()
    {
        return timeStamp;
    }



}
