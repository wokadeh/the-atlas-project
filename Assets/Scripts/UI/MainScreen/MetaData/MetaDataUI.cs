using TMPro;
using UnityEngine;

public class MetaDataUI : MonoBehaviour
{
    private TextMeshProUGUI m_Text;

    DataManager m_DataManager;

    // Start is called before the first frame update
    void Start()
    {
        Log.Warn( this, "Start MetaDataUI" );
        m_DataManager = GameObject.Find( "SCRIPTS" ).GetComponent<DataManager>();

        foreach(Component cp in transform)
        {
            Log.Info( this, "Found child " + cp.ToString() );
        }
        m_Text = this.transform.GetChild( 0 ).GetComponent<TextMeshProUGUI>();
    }

    private void Awake()
    {
        this.ReadWriteData();
    }

    private void ReadWriteData()
    {
        m_Text = this.GetText();
        m_Text.text = "";
        // Start time
        m_Text.text += "\nStart time:\t" + m_DataManager.MetaData.StartDateTimeNumber.ToString();
        // End time
        m_Text.text += "\nEnd time:\t\t" + m_DataManager.MetaData.EndDateTimeNumber.ToString();
        // List parameters
        m_Text.text += "\nVariables:\t\t";
        for(int i = 0; i < m_DataManager.MetaData.Variables.Count; i++)
        {
            m_Text.text += m_DataManager.MetaData.Variables[i].Name;

            if( i != m_DataManager.MetaData.Variables.Count - 1 ) m_Text.text += ", ";
        }
        // Number of levels
        m_Text.text += "\nAlt. pres. levels:\t\t" + m_DataManager.MetaData.Levels.ToString();
        // Hour intervals
        m_Text.text += "\nHourly interval:\t\t" + m_DataManager.MetaData.TimeInterval.ToString();
        // Bit depth
        m_Text.text += "\nBit depth:\t\t" + m_DataManager.MetaData.BitDepth.ToString();
    }

    private TextMeshProUGUI GetText()
    {
        if(m_Text == null)
        {
            return this.transform.GetChild( 0 ).GetComponent<TextMeshProUGUI>();
        }
        else
        {
            return m_Text;
        }
    }
}
