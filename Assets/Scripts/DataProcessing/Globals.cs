﻿// Performance & Tricks
// Microsoft and Unity
// https://docs.microsoft.com/en-us/windows/mixed-reality/performance-recommendations-for-unity


using SFB;
using Unity;
using UnityEngine;

public struct Globals
{
    public static readonly ExtensionFilter[] XML_FILE_FILTER = new ExtensionFilter[] {
        new ExtensionFilter("Xml File", "xml")
    };

    public const string SAVE_PROJECTS_PATH = "Assets/Saved_Projects/";
    public const string TEXTURE3D_FOLDER_NAME = "texture3D/";
    public const string TEXTURE3D_PREFEX = "tex3d_";

    public const string BIT_DEPTH_ATTRIBUTE = "bit_depth";
    public const string WIDTH_ATTRIBUTE = "width";
    public const string HEIGHT_ATTRIBUTE = "height";
    public const string LEVELS_ATTRIBUTE = "levels";
    public const string START_DATETIME_ATTRIBUTE = "start_datetime";
    public const string END_DATETIME_ATTRIBUTE = "end_datetime";
    public const string TIME_INTERVAL_ATTRIBUTE = "time_interval";

    public const string TIMESTAMP_LIST_ELEMENT = "timestamp";
    public const string TIMESTAMP_DATETIME_ATTRIBUTE = "datetime";

    public const string VARIABLE_ELEMENT = "variable";
    public const string VARIABLE_NAME_ATTRIBUTE = "name";

    public const string TIME_STAMP_DATA_ASSET_ELEMENT = TIMESTAMP_LIST_ELEMENT;
    public const string TIME_STAMP_DATA_ASSET_DIM_X_ATTRIBUTE = "dim_x";
    public const string TIME_STAMP_DATA_ASSET_DIM_Y_ATTRIBUTE = "dim_y";
    public const string TIME_STAMP_DATA_ASSET_DIM_Z_ATTRIBUTE = "dim_z";
    public const string TIME_STAMP_DATA_ASSET_DATA_TEXTURE_NAME_ATTRIBUTE = "data_texture_name";
    public const string TIME_STAMP_DATA_ASSET_HISTO_TEXTURE_NAME_ATTRIBUTE = "histo_texture_name";


    public const string EARTH_DATA_FRAME_TF_NODE_NAME_ATTRIBUTE = "tf_node";

    public const int TRANSFER_FUNTCTION_TEXTURE_WIDTH = 512;
    public const int TRANSFER_FUNTCTION_TEXTURE_HEIGHT = 2;

    public const string CAMERA_ORBIT_TITLE = "Orbit";
    public const string CAMERA_FIRSTP_TITLE = "First Person";

    public const float TIMELINE_SPEEDFACTOR = 100;

    public static Vector3 CARTESIAN_SCALE = new Vector3( 1, 0.2f, -0.75f );
    public static Quaternion CARTESIAN_ROTATION = Quaternion.Euler( 180, 0, 0 );
    public static Vector3 SPHERICAL_SCALE = new Vector3( 1, 1, 1 );
    public static Quaternion SPHERIAL_ROTATION = Quaternion.Euler( -90, 0, 0 );
}
