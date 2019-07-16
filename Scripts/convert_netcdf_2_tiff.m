
% --------------------------------------------------------------------------------------------------------------------
% INPUT: Set your wishes! 
BIT_DEPTH = 16;
FILE_NAME = 'C:\Users\wokad\Documents\ParaView_DATA\Pro Paraview SSW pv\potential_vorticity_copernicus_Guacamole\pv_2018_01_025.nc'
PARAMETER_NAME = 'pv'
% --------------------------------------------------------------------------------------------------------------------

% More misc constants
UINT8_NUM = 256;
UINT16_NUM = 65535;
UINT32_NUM = 4294967295;

% Read the header
ncdisp( FILE_NAME );

% Open the file in read only mode
ncid = netcdf.open( FILE_NAME,'NOWRITE' );

% Inspect num of dimensions, variables, attributes, unim
[ndim, nvar, natt, unlim] = netcdf.inq( ncid );

% varname: name of parameter
% xtype: 2=character, 3=short integer, 4=integer, 5=real, 6=double
% dimid: number of elements, e.g. number of columns of matrix
for i = 0:nvar-1
    [varname, xtype, dimid, natt]=netcdf.inqVar( ncid, i );

    % Set parameter index
    if strcmp( varname, PARAMETER_NAME ) == 1
        varnumber = i;
    end
end

[varname, xtype, dimid, natt] = netcdf.inqVar( ncid, varnumber );

% Extract info for each dimension
for i=1:length(dimid)
    [dimname, dimlength] = netcdf.inqDim( ncid, dimid( 1, i ) );
end 

for i = 0:nvar-1
    [varname, xtype, dimid, natt]=netcdf.inqVar(ncid,i);

    if strcmp(varname,'latitude')==1
        dimnumber = i;
    end
end

% Load dimensions from NetCDF file
LATITUDE_DIM = ncread( FILE_NAME, 'latitude' );
LONGITUDE_DIM = ncread( FILE_NAME, 'longitude' );
LEVEL_DIM = ncread( FILE_NAME, 'level' );
TIME_DIM = ncread( FILE_NAME, 'time' );

% Actual reading out the values
valuesInFile = ncread( FILE_NAME, PARAMETER_NAME );

mkdir( PARAMETER_NAME );

% Find the range values of the entire dataset!
minValue = min( min( min( min ( valuesInFile ) ) ) );

% In case there exist negative values add range of distance from 0 to
% minimum
if( minValue < 0 )
    valuesInFile = valuesInFile + ( -minValue );  
end

maxValue = max( max ( max ( max ( valuesInFile ) ) ) );

% Normalize to 0 - 1 range
valuesNorm = valuesInFile / maxValue;

% Run through every time step
for timeStep = 1:length( TIME_DIM )
    mkdir( PARAMETER_NAME, int2str( timeStep ) );
    
    % Go up every hpa-level in altitude
    for levelStep = 1:length( LEVEL_DIM )
        
        % Read data to file and transpose
        values = valuesNorm( :, :, levelStep, timeStep )';
          
        % Convert to uint image
        if( BIT_DEPTH == 16 )
            valuesImage = uint16( round( values  * UINT16_NUM ));
        else
            valuesImage = uint8( round( values  * UINT8_NUM ));
        end
        
        debugOutputUniqueValues = length(unique(valuesImage))
        
        axis off;
        
        newImageFileName = strcat(PARAMETER_NAME, '/', int2str(timeStep), '/' , PARAMETER_NAME, '_', int2str(LEVEL_DIM(levelStep)), '.tif' );
        
        imwrite( valuesImage, newImageFileName );
        
    end
end
clear FILE_NAME
clear PARAMETER_NAME



