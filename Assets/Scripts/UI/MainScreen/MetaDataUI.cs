using TMPro;
using UnityEngine;

public class MetaDataUI : MonoBehaviour
{
    private TextMeshProUGUI m_Text;

    // Start is called before the first frame update
    private void Awake()
    {
        this.ReadWriteData();
    }

    private void ReadWriteData()
    {
        m_Text = this.GetText();

        IMetaData metaData = Singleton.GetDataManager().MetaData;

        Debug.Log( "End datetime: " + metaData.EndDateTimeNumber );

        m_Text.text = "";
        // Start time
        m_Text.text += "\nStart time:\t\t" + Utils.TryConvertDoubleToDateTimeString( metaData.StartDateTimeNumber );
        // End time
        m_Text.text += "\nEnd time:\t\t\t" + Utils.TryConvertDoubleToDateTimeString( metaData.EndDateTimeNumber );
        // List parameters
        m_Text.text += "\nVariables:\t\t\t";
        for(int i = 0; i < metaData.Variables.Count; i++)
        {
            m_Text.text += metaData.Variables[i].Name + " ( Min: " + metaData.Variables[ i ].Min + ", Max: " + metaData.Variables[ i ].Max + " )";

            if( i != metaData.Variables.Count - 1 ) m_Text.text += "\n\t\t\t\t";
        }
        // Number of levels
        m_Text.text += "\nAlt. pres. levels:\t\t" + metaData.Levels.ToString();
        // Hour intervals
        m_Text.text += "\nHourly interval:\t\t" + metaData.TimeInterval.ToString();
        // Height
        m_Text.text += "\nHeight:\t\t\t" + metaData.Height.ToString();
        // Width
        m_Text.text += "\nWidth:\t\t\t" + metaData.Width.ToString();
        // Bit depth
        m_Text.text += "\nBit depth:\t\t\t" + metaData.BitDepth.ToString();
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
