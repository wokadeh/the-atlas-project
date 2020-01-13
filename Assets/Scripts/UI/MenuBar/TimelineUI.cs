// ****************************** LOCATION ********************************
//
// [UI] TimelinePanel -> attached
//
// ************************************************************************

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimelineUI : MonoBehaviour
{
    [SerializeField] private GameObject m_TimelinePanel;
    [SerializeField] private int m_FPS;
    [SerializeField] private Color m_SelectedColor;
    [SerializeField] private Slider m_TimelineSlider;
    [SerializeField] private Button m_ToStartButton;
    [SerializeField] private Button m_SlowerButton;
    [SerializeField] private Button m_FasterButton;
    [SerializeField] private Button m_StepBackwardsButton;
    [SerializeField] private Button m_PlayButton;
    [SerializeField] private TMP_Text m_PlayButtonText;
    [SerializeField] private TMP_Text m_SpeedLabelText;
    [SerializeField] private Button m_StepForwardButton;
    [SerializeField] private Button m_ToEndButton;
    [SerializeField] private Button m_LoopButton;
    [SerializeField] private TMP_Text m_LoopButtonText;

    public TimestampUI m_TimestampUI;

    private float m_SpeedFactor;
    private float m_SpeedCounter;
    private bool m_IsPlaying;
    private bool m_ShouldLoop;

    private void Start()
    {
        this.Reset();

        m_TimelineSlider.onValueChanged.AddListener( this.OnTimelineChanged );
        m_ToStartButton.onClick.AddListener( () => m_TimelineSlider.value = 0 );
        m_StepBackwardsButton.onClick.AddListener( this.OnBackward );
        m_PlayButton.onClick.AddListener( this.OnPlay );
        m_StepForwardButton.onClick.AddListener( this.OnForward );
        m_ToEndButton.onClick.AddListener( () => m_TimelineSlider.value = m_TimelineSlider.maxValue );
        m_LoopButton.onClick.AddListener( this.OnLoop );
        m_FasterButton.onClick.AddListener( this.OnFaster );
        m_SlowerButton.onClick.AddListener( this.OnSlower );
    }

    private void OnEnable()
    {
        this.Reset();

        if( Singleton.GetDataManager().CurrentTimeStepDataAsset != null )
        {
            m_TimelineSlider.minValue = 0;
            m_TimelineSlider.maxValue = Singleton.GetDataManager().CurrentDataAssetList.Count - 1;
            m_TimelineSlider.wholeNumbers = true;
        }
    }

    private void OnDisable()
    {
        this.Reset();

        if( m_IsPlaying )
        {
            this.OnPlay();
        }
    }

    private void OnForward()
    {
        this.MoveInTimeline( 1 );
    }

    private void OnBackward()
    {
        this.MoveInTimeline( -1 );
    }

    private void OnSlower()
    {
        this.ChangeSpeed( 1 );
    }

    private void OnFaster()
    {
        this.ChangeSpeed( -1 );
    }

    private void Reset()
    {
        m_SpeedFactor = 1F;
        m_SpeedCounter = 0F;
        m_IsPlaying = false;
        m_ShouldLoop = false;
    }

    private void MoveInTimeline( int _change )
    {
        m_TimelineSlider.value += _change;
    }

    private void ChangeSpeed( int _change )
    {
        Log.Info( this, "OnChange speed " + _change );
        m_SpeedCounter += _change;

        if( m_SpeedCounter < 0 ) // We are slow already
        {
            m_SpeedLabelText.text = Mathf.Abs( m_SpeedCounter - 1 ) + "x";
            m_SpeedFactor = 1F / ( Globals.TIMELINE_SPEEDFACTOR * Mathf.Abs( m_SpeedCounter ) );
        }
        else if( m_SpeedCounter > 0 ) // We are fast already
        {
            m_SpeedLabelText.text = "1/" + Mathf.Abs( m_SpeedCounter + 1 ) + "x";
            m_SpeedFactor = Globals.TIMELINE_SPEEDFACTOR * Mathf.Abs( m_SpeedCounter );
        }
        else
        {
            m_SpeedLabelText.text = "";
            m_SpeedFactor = 1;
        }
    }

    public void Show( bool _isShown )
    {
        // Warning: Normally the timelinepanel will be toggles from Unity UI (Button -> onClick() -> TogglePanel (item)
        m_TimelinePanel.SetActive( _isShown );
    }

    private void OnPlay()
    {
        // Bail out if we have no assets yet
        if( Singleton.GetDataManager().CurrentDataAssetList.Count <= 0 )
        {
            return;
        }

        m_IsPlaying = !m_IsPlaying;

        if( m_IsPlaying )
        {
            m_PlayButtonText.text = "\uf04c";
            m_TimelineSlider.interactable = false;
            this.StartCoroutine( this.PlayAnimation() );
        }
        else
        {
            m_PlayButtonText.text = "\uf04b";
            m_TimelineSlider.interactable = true;
            this.StopAllCoroutines();
        }
    }

    private void OnLoop()
    {
        m_ShouldLoop = !m_ShouldLoop;

        // Set text color
        if( m_ShouldLoop )
        {
            m_LoopButtonText.color = m_SelectedColor;
        }
        else
        {
            m_LoopButtonText.color = Color.white;
        }
    }

    private void OnTimelineChanged( float value )
    {
        // Bail out if we have no assets yet
        if( Singleton.GetDataManager().CurrentDataAssetList.Count <= 0 )
        {
            return;
        }

        TimeStepDataAsset asset = Singleton.GetDataManager().CurrentDataAssetList[ ( int ) value ];
        Singleton.GetDataManager().SetCurrentAsset( asset );
        Singleton.GetVolumeRenderer().SetTexture3D( asset );

        m_TimestampUI.UpdateTimestamp( (uint)value );
    }

    private IEnumerator PlayAnimation()
    {
        int start = Mathf.Clamp( ( int ) m_TimelineSlider.value + 1, 0, ( int ) m_TimelineSlider.maxValue );

        for( int f = start; f <= m_TimelineSlider.maxValue; f++ )
        {
            yield return new WaitForSeconds( ( 1F / m_FPS ) * m_SpeedFactor * Time.deltaTime );

            m_TimelineSlider.value = f;

            // Reset timeline if we want to loop the animation
            if( f == m_TimelineSlider.maxValue && m_ShouldLoop )
            {
                f = -1;
            }
        }

        this.OnPlay();
    }
}
