using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class PlotResult : MonoBehaviour {

    private ExperimentManager experimentManager;

    private GameObject graphObj;

    public WMG_Axis_Graph graph;
    public WMG_Series vmRotSeries;
    public WMG_Series angErrorSeries;

    public List<Vector2> vmRotData;
    public List<Vector2> angErrorData;

    void Awake()
    {

        // check if there's Graph_Maker asset
        if (Config.instance.hasPaidAssets)
        {
            
            // reference
            GameObject experimentManagerObj = GameObject.Find("Experiment Manager");
            experimentManager = experimentManagerObj.GetComponent<ExperimentManager>();
            

        }

    }


	
    public void Plot()
    {


        // instantiate graph object
        graphObj = Instantiate(Resources.Load("Prefabs/VMRotGraph")) as GameObject;
        graphObj.transform.SetParent(transform, false);

        graph = graphObj.GetComponent<WMG_Axis_Graph>();

        Debug.Log("add series");
        vmRotSeries = graph.addSeries();
        angErrorSeries = graph.addSeries();

        // set priperties
        vmRotSeries.seriesName = "Visuo-Motor Rotation";
        vmRotSeries.pointPrefab = 0; // 0: circle, 1: square
        vmRotSeries.pointColor = Color.yellow;

        angErrorSeries.seriesName = "Angular Error";
        angErrorSeries.pointPrefab = 0; // 0: circle, 1: square
        angErrorSeries.pointColor = Color.white;


        // prepare data to plot
        int iTrial = 1;
        foreach (Dictionary<string, string> expPrm in experimentManager.protocol)
        {
            vmRotData.Add( new Vector2(iTrial, float.Parse(expPrm["Visuo Motor Rotation Angle"])) );
            iTrial++;
        }
        iTrial = 1;
        foreach (Dictionary<string, string> result in experimentManager.resultList)
        {
            angErrorData.Add( new Vector2(iTrial, float.Parse(result["MaxAngDevAtTMaxVel"])) );
            iTrial++;
        }

        
        // set data
        vmRotSeries.pointValues.SetList(vmRotData);
        angErrorSeries.pointValues.SetList(angErrorData);

        graph.xAxis.AxisMaxValue = vmRotData.Count + 1;
        graph.xAxis.AxisNumTicks = vmRotData.Count + 1;
        

    }

}
