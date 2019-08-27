using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class timestamp : MonoBehaviour
{
    DataManager dataManager;
    TextMeshProUGUI text;

    IMetaData currentData;

    public string currentDate;
    public double varDate;
    public int currentIndex;

    public DateTime dateTime;

    void Start()
    {
        dataManager = GameObject.Find("SCRIPTS").GetComponent<DataManager>();
        text = transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        updateTimestamp(0);
    }

    public void updateTimestamp(int dateIndex)
    {
        Debug.Log("updating time");

        currentData = dataManager.MetaData;

        if (currentData != null)
        {
            currentIndex = dateIndex;

            varDate = currentData.Timestamps[0][dateIndex].DateTime;
            dateTime = DateTime.FromOADate(varDate - 693960);

            currentDate = dataManager.CurrentVariable + "_" + dateTime.ToString();

            text.text = "Variable: " + dataManager.CurrentVariable + "\n"
            + "Time: " + dateTime.ToString();
        }
        else
        {
            Debug.Log("no Data");
        }
    }

}
