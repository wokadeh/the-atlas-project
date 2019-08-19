﻿using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimelineUI : MonoBehaviour
{
    [SerializeField] private GameObject m_TimelinePanel;
    [SerializeField] private DataManager m_DataManager;
    [SerializeField] private VolumeRenderer m_VolumeRenderer;
    [SerializeField] private int m_FPS;
    [SerializeField] private Color m_SelectedColor;
    [SerializeField] private Slider m_TimelineSlider;
    [SerializeField] private Button m_ToStartButton;
    [SerializeField] private Button m_StepBackwardsButton;
    [SerializeField] private Button m_PlayButton;
    [SerializeField] private TMP_Text m_PlayButtonText;
    [SerializeField] private Button m_StepForwardButton;
    [SerializeField] private Button m_ToEndButton;
    [SerializeField] private Button m_LoopButton;
    [SerializeField] private TMP_Text m_LoopButtonText;

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
    }

    private void OnEnable()
    {
        this.Reset();

        if ( m_DataManager.m_CurrentAsset != null )
        {
            m_TimelineSlider.minValue = 0;
            m_TimelineSlider.maxValue = m_DataManager.CurrentDataAssets.Count - 1;
            m_TimelineSlider.wholeNumbers = true;
        }
    }

    private void OnDisable()
    {
        this.Reset();

        if ( m_IsPlaying )
        {
            this.OnPlay();
        }
    }

    private void OnForward()
    {
        this.ChangeSpeed( -1 );
    }

    private void OnBackward()
    {
        this.ChangeSpeed( 1 );
    }

    private void Reset()
    {
        m_SpeedFactor = 1F;
        m_SpeedCounter = 0F;
        m_IsPlaying = false;
        m_ShouldLoop = false;
    }

    private void ChangeSpeed( float change )
    {
        Log.Info( this, "OnChange speed " + change );
        if ( !m_IsPlaying )
        {
            m_TimelineSlider.value += change;
        }
        else
        {
            m_SpeedCounter += change;


            if ( m_SpeedCounter < 0 ) // We are slow already
            {
                Log.Info( this, "Small step" );
                m_SpeedFactor = 1F / ( Globals.TIMELINE_SPEEDFACTOR * Mathf.Abs( m_SpeedCounter ) );
            }
            else if ( m_SpeedCounter > 0 )// We are fast already
            {
                Log.Info( this, "Big step" );
                m_SpeedFactor = Globals.TIMELINE_SPEEDFACTOR * Mathf.Abs( m_SpeedCounter );
            }
            else
            {
                m_SpeedFactor = 1;
            }

            Log.Info( this, "SpeedCounter is now " + m_SpeedCounter );
            Log.Info( this, "Speedfactor is now " + m_SpeedFactor );
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
        if ( m_DataManager.CurrentDataAssets.Count <= 0 )
        {
            return;
        }

        m_IsPlaying = !m_IsPlaying;

        if ( m_IsPlaying )
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
        if ( m_ShouldLoop )
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
        if ( m_DataManager.CurrentDataAssets.Count <= 0 )
        {
            return;
        }

        TimeStepDataAsset asset = m_DataManager.CurrentDataAssets[ ( int ) value ];
        m_DataManager.SetCurrentAsset( asset );
        m_VolumeRenderer.SetData( asset );
    }

    private IEnumerator PlayAnimation()
    {
        int start = Mathf.Clamp( ( int ) m_TimelineSlider.value + 1, 0, ( int ) m_TimelineSlider.maxValue );

        for ( int f = start; f <= m_TimelineSlider.maxValue; f++ )
        {
            yield return new WaitForSeconds( ( 1F / m_FPS ) * m_SpeedFactor * Time.deltaTime );

            m_TimelineSlider.value = f;

            // Reset timeline if we want to loop the animation
            if ( f == m_TimelineSlider.maxValue && m_ShouldLoop )
            {
                f = -1;
            }
        }

        this.OnPlay();
    }
}
