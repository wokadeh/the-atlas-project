#! python
import inspect, os, sys

accessRights = 0o755

from ecmwfapi import ECMWFDataServer
server = ECMWFDataServer()

# find docu at https://confluence.ecmwf.int/display/WEBAPI/Accessing+ECMWF+data+servers+in+batch

# all 37 Levels
levelList = [1000,975,950,925,900,875,850,825,800,775,750,700,650,600,550,500,450,400,350,300,250,225,200,175,150,125,100,70,50,30,20,10,7,5,3,2,1]

# param codes, see https://apps.ecmwf.int/codes/grib/param-db
tp = "130.128"
pv = "60.128"
gp = "129.128"
om = "203.128"

gridRes = 0.125

targetFolderName = "C:\\Users\\Alexander\\Documents\\WeatherData\\download"

paramList = [pv, gp, tp, om]
paramNameList = ["Potential vorticity", "Geopotential", "Temperature", "Ozone Mass mixing ratio"]

begin = "2018-01-01"
end = "2018-02-28"

classVar = "ei"
dataSetVar = "interim"
dateVar = begin + "/to/" + end
expverVar = "1"
gridVar = repr(gridRes) + "/" + repr(gridRes)
levtypeVar = "pl"
stepVar = "0"
streamVar = "oper"
timeVar = "00/06/12/18"
typeVar = "an"
formatVar = "netcdf"

dataFolderName = dataSetVar + "_" + begin + "_to_" + end + "_" + repr(gridRes)

for param in range(0, 4):
    for lvl in levelList:

        folders = targetFolderName + "\\" + dataFolderName + "\\" + paramNameList[param]  + "\\"
        if not os.path.exists(folders): os.makedirs(folders) 

        targetVar = folders + "ecmwf_" + repr(gridRes) + "_" + paramNameList[param] + "_" + repr(lvl) + ".nc"
        print(targetVar)
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