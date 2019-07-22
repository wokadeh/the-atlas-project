% (c) Jan Storm, Alexander Wollert
%  2019
%  Hochschule Duesseldorf

% -------------- BEGIN Intro -----------------------------------------------------------------------------------------

% INPUT: Set your wishes! 
BIT_DEPTH = 8;
DATA_DIR = 'C:\Users\Alexander\Downloads\COPERNICUS\';
FILE_NAME = '2018-01-00061218_1-1000_pv_oz';
FILE_SURFIX = '.nc';
FILE = strcat( FILE_NAME, FILE_SURFIX );
DATA_FILE_PATH = strcat( DATA_DIR, FILE );
FOLDER_NAME = strcat(FILE_NAME, '_', int2str( BIT_DEPTH ), 'bit');

TIFF_DIR = '../Data/';
META_DATA_FILE_NAME =  strcat( FOLDER_NAME, '___META_DATA___', '.xml' );

% --------------------------------------------------------------------------------------------------------------------

% More misc constants
UINT8_NUM = 256;
UINT16_NUM = 65535;
% UINT32_NUM = 4294967295; <- unnessessary, needs TIFF class

UINT_DIM = [UINT8_NUM, UINT16_NUM];

% Get all possible variables
VARIABLES = getNETCDFDataVariables( DATA_FILE_PATH );

% Load dimensions from NetCDF file
LATITUDE_DIM = ncread( DATA_FILE_PATH, 'latitude' );
LONGITUDE_DIM = ncread( DATA_FILE_PATH, 'longitude' );
LEVEL_DIM = ncread( DATA_FILE_PATH, 'level' );
TIME_DIM = ncread( DATA_FILE_PATH, 'time' );

TIME_INTERVAL = 0;

if( length( TIME_DIM ) > 0 )
    TIME_INTERVAL = TIME_DIM(2,1) - TIME_DIM(1,1);
end

createBaseDirectory( TIFF_DIR );
createMetaData( META_DATA_FILE_NAME, FILE_NAME, VARIABLES, TIFF_DIR, BIT_DEPTH, length( LONGITUDE_DIM ), length( LATITUDE_DIM ), length( LEVEL_DIM ), TIME_INTERVAL );

% -------------- END Intro -----------------------------------------------------------------------------------------

% Run for all variables
for i = 1 : length( VARIABLES )
    variableName = VARIABLES( i );
    createTiffFilesForVariables( DATA_FILE_PATH, TIFF_DIR, FOLDER_NAME, variableName, TIME_DIM, LEVEL_DIM, BIT_DEPTH, UINT_DIM );
end

function [valuesNorm] = normalizeValues( valuesInFile )
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
end

function createTiffFilesForVariables( filePath, tiffDirectory, folderName, variableName, timeDim, levelDim, bitDepthSize, uintDim )
    % Actual reading out the values
    valuesInFile = ncread( filePath, variableName.id );
    variableFileName = lower( variableName.name );
    
    variableDirectory = strcat( tiffDirectory, '/', folderName, '/', variableFileName );

    mkdir( variableDirectory );

    valuesNorm = normalizeValues( valuesInFile );


    % Run through every time step
    for timeStep = 1 : length( timeDim )

         % We convert the weird netcdf timestamp to something usable
         timestamp = makeTimeStamp( timeStep, timeDim );

         % Create the directory for the current time stamp
         timeDirectory = strcat( variableDirectory, '/', datestr( timestamp, 'yyyy-mm-dd_HH-MM-SS' ) );

        mkdir( timeDirectory );
        
        % Go up every hpa-level in altitude
        for levelStep = 1 : length( levelDim )
            
            % Read data to file and transpose
            values = valuesNorm( :, :, levelStep, timeStep )';
            
            % Convert to uint image
            if( bitDepthSize == 16 )
                valuesImage = uint16( round( values  * uintDim( 2 ) ));
                debugFileBithDeth( valuesImage, uintDim( 2 ) );
            else
                valuesImage = uint8( round( values  * uintDim( 1 ) ));
                debugFileBithDeth( valuesImage, uintDim( 1 ) );
            end
     
            axis off;
            
            % Write actual tiff file to disk
            fileName = strcat( timeDirectory, '/', variableFileName, '_', int2str( levelDim ( levelStep ) ), '.tif' );
            imwrite( valuesImage, fileName );
            
        end
    end
end

function debugFileBithDeth( valuesImage, uintDim )
    % Must be smaller than uint size!
    debugOutputUniqueValues = length( unique( valuesImage ) );
    
    if( debugOutputUniqueValues > uintDim )
        warning( strcat("The bit size of ", int2str( uintDim ), "is not high enough, the unique values are ",  int2str( debugOutputUniqueValues ) , ". INFORMATION LOSS!!!" ) );
    end
end

function createMetaData( metaDataFileName, fileName, variables, tiffDirectory, bitDepth, width, height, levels, timeInterval )
    document = com.mathworks.xml.XMLUtils.createDocument( 'variables' );
    root = document.getDocumentElement;
    
    % Hardcoded for now but that might change in the future
    root.setAttribute( 'bit_depth', int2str( bitDepth ) );
    root.setAttribute( 'width', int2str( width ) );
	root.setAttribute( 'height', int2str( height ) );
    root.setAttribute( 'levels', int2str( levels ) );
    root.setAttribute( 'time_interval', int2str( timeInterval ) );
	
    for v = 1 : length( variables )
        % Create element for variable
        variable = variables( v );
        variable_element = document.createElement( 'variable' );
        variable_element.setAttribute( 'name', variable.name );
        
        % Append element to root
        root.appendChild( variable_element );
    end
    
    % Write xml to disk
    xmlwrite( strcat( tiffDirectory, '/', fileName, '/', metaDataFileName ), document );
end

% Retrieves a list of all actual data variable names in a netcdf file
function variables = getNETCDFDataVariables( netcdfFileName )
    netcdfId = netcdf.open( netcdfFileName, 'NC_NOWRITE' );
    
    [~, variable_count, ~, ~] = netcdf.inq( netcdfId );
    
    variables = [];
    for i = 0 : variable_count - 1
        % We are interested in the number of dimensions the variable takes as an "input"
        [variable_name, ~, dimension_ids, ~] = netcdf.inqVar( netcdfId, i );
        dimension_count = length( dimension_ids );
       
        variable.id = variable_name;
        variable.name = netcdf.getAtt( netcdfId, i, 'long_name' );
        
        % We assume every variable with 4 dimensions is actual data we care about
        if (dimension_count == 4)
            variables = [variables, variable];
        end
    end
    
    netcdf.close( netcdfId );
end

function createBaseDirectory( tiffDirectory )
%     if( exist( tiffDirectory, 'dir' ) ) 
%        rmdir( tiffDirectory, 's' ); 
%     end
    mkdir( tiffDirectory );
end

function timeStamp = makeTimeStamp( timeStep, timeDim )
    timeStamp = double( timeDim( timeStep ) ) / 24 + datenum( '1900-01-01 00:00:00' );
end



