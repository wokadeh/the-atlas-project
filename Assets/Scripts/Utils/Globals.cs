// Performance & Tricks
// Microsoft and Unity recommendations:
// https://docs.microsoft.com/en-us/windows/mixed-reality/performance-recommendations-for-unity


using SFB;
using Unity;
using UnityEngine;

// This file contains constant values. In the future these values have to be moved into a Settings window inside the UI
public struct Globals
{
    // ---------------> DATA FORMAT
    public static readonly ExtensionFilter[] XML_FILE_FILTER = new ExtensionFilter[] {
        new ExtensionFilter("Xml File", "xml")
    };

    // ---------------> PROJECT
    public const string IMPORT_DATA_PATH = "Data\\";
    public const string SAVE_PROJECTS_PATH = "Assets\\Saved_Projects\\";
    public const string SAVE_SNAPSHOTS_PATH = "Assets\\Snapshots\\";
    public const string SNAPSHOT_NAME = "camerasnapshot";

    // ---------------> TEXTURE
    public const string TEXTURE3D_FOLDER_NAME = "texture3D/";
    public const string TEXTURE3D_PREFEX = "tex3d_";

    // ---------------> PARAMETERS
    public const string BIT_DEPTH_ATTRIBUTE = "bit_depth";
    public const string WIDTH_ATTRIBUTE = "width";
    public const string HEIGHT_ATTRIBUTE = "height";
    public const string LEVELS_ATTRIBUTE = "levels";
    public const string START_DATETIME_ATTRIBUTE = "start_datetime";
    public const string END_DATETIME_ATTRIBUTE = "end_datetime";
    public const string TIME_INTERVAL_ATTRIBUTE = "time_interval";

    // ---------------> VARIABLES
    public const string VARIABLE_ELEMENT = "variable";
    public const string VARIABLE_NAME_ATTRIBUTE = "name";
    public const string VARIABLE_MIN_ATTRIBUTE = "min";
    public const string VARIABLE_MAX_ATTRIBUTE = "max";

    // ---------------> TIMESTAMPS
    public const string TIME_STAMP_LIST_ELEMENT = "timestamp";
    public const string TIME_STAMP_DATETIME_ATTRIBUTE = "datetime";

    public const string TIME_STAMP_DATA_ASSET_ELEMENT = TIME_STAMP_LIST_ELEMENT;
    public const string TIME_STAMP_DATA_ASSET_DIM_X_ATTRIBUTE = "dim_x";
    public const string TIME_STAMP_DATA_ASSET_DIM_Y_ATTRIBUTE = "dim_y";
    public const string TIME_STAMP_DATA_ASSET_DIM_Z_ATTRIBUTE = "dim_z";
    public const string TIME_STAMP_DATA_ASSET_DATA_TEXTURE_NAME_ATTRIBUTE = "data_texture_name";
    public const string TIME_STAMP_DATA_ASSET_HISTO_TEXTURE_NAME_ATTRIBUTE = "histo_texture_name";

    // ---------------> TRANSFER FUNCTION
    public const int TRANSFER_FUNTCTION_TEXTURE_WIDTH = 512;
    public const int TRANSFER_FUNTCTION_TEXTURE_HEIGHT = 2;

    // ---------------> CAMERA
    public const string CAMERA_ORBIT_TITLE = "Orbit";
    public const string CAMERA_FIRSTP_TITLE = "First Person";


    // ---------------> REPRESENTATION
    public static Vector3 CARTESIAN_SCALE = new Vector3( 1, 0.2f, -0.75f );
    public static Quaternion CARTESIAN_ROTATION = Quaternion.Euler( 180, 0, 0 );
    public static Vector3 SPHERICAL_SCALE = new Vector3( 1, 1, 1 );
    public static Quaternion SPHERIAL_ROTATION = Quaternion.Euler( -90, 0, 0 );

    // ---------------> CONSTANT VALUES
    public static int DATE_FIX_NUMBER = 693960;
    public const float TIMELINE_SPEEDFACTOR = 100;

    public const float MAX_PRESSURE = 1000;
    public const float LOG_MAX_PRESSURE = 6.907755279F;

    // ---------------> CREATE LIST OF PRESSURE LEVELS
    public static int[] LEVEL_LIST_37()
    {
        int[] outputLevels = new int[37];

        outputLevels[ 0 ] = 1;
        outputLevels[ 1 ] = 2;
        outputLevels[ 2 ] = 3;
        outputLevels[ 3 ] = 5;
        outputLevels[ 4 ] = 7;

        for( int i = 5; i < 11; i++ ) outputLevels[ i ] = outputLevels[ i - 5 ] * 10;
        for( int i = 11; i < 17; i++ ) outputLevels[ i ] = outputLevels[ i - 1 ] + 25;
        for( int i = 17; i < 27; i++ ) outputLevels[ i ] = outputLevels[ i - 1 ] + 50;
        for( int i = 27; i < 37; i++ ) outputLevels[ i ] = outputLevels[ i - 1 ] + 25;

        return outputLevels;
    }
}
