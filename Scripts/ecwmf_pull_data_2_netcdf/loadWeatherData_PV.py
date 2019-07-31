#! python
import inspect, os, sys

from ecmwfapi import ECMWFDataServer
server = ECMWFDataServer()

levelList = [1000,975,950,925,900,875,850,825,800,775,750,700,650,600,550,500,450,400,350,300,250,225,200,175,150,125,100,70,50,30,20,10,7,5,3,2,1]

pv = "130.128"

targetFolderName = "C:\\Users\\wokad\\Documents\\ParaView_DATA\\WeatherData\\download"

paramList = [pv]
paramNameList = ["Potential vorticity"]

classVar = "ei"
dataSetVar = "interim"
dateVar = "2018-02-12/to/2018-02-12"
expverVar = "1"
gridVar = "0.125/0.125"
levtypeVar = "pl"
stepVar = "0"
streamVar = "oper"
timeVar = "00:00:00"
typeVar = "an"
formatVar = "netcdf"


for param in range(0, 2):
    for lvl in levelList:
        targetVar = targetFolderName + "\\" + paramNameList[param] + "\\" + "ecmwf_0125_" + paramNameList[param] + "_" + repr(lvl) + ".nc"
        server.retrieve({
            "class": classVar,
            "dataset": dataSetVar,
            "date": dateVar,
            "expver": expverVar,
            "grid": gridVar,
            "levelist": lvl,
            "levtype": levtypeVar,
            "param": paramList[param],
            "step": stepVar,
            "stream": streamVar,
            "time": timeVar,
            "type": typeVar,
            "format": formatVar,
            "target": targetVar
        })