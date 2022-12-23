using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StaminaController : SingletonMonobehaviour<StaminaController>
{
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Sprite[] moodSprite;
    [SerializeField] private GameObject moodObj;
    [SerializeField] private TextMeshProUGUI staminaText;
    private Image moodImage;

    private float time;
    private float lerpSpeed = 0.25f;

    protected override void Awake()
    {
        base.Awake();
        staminaSlider = GetComponent<Slider>();
        moodImage = moodObj.GetComponent<Image>();
    }

    private void Start()
    {
        // initial stamina
        staminaSlider.maxValue = Player.Instance._stamina;
        staminaSlider.value = Player.Instance.Stamina;
        staminaText.text = staminaSlider.value.ToString() + " / " + staminaSlider.maxValue;

        float percentage = Mathf.InverseLerp(staminaSlider.minValue, staminaSlider.maxValue, Player.Instance.Stamina) * 100;

        if (percentage >= 60)
        {
            moodImage.sprite = moodSprite[0];
        }
        else if (percentage < 60 && percentage >= 30)
        {
            moodImage.sprite = moodSprite[1];
        }
        else if (percentage < 30 && percentage >= 0)
        {
            moodImage.sprite = moodSprite[2];
        }
    }

    private void Update()
    {
        SmoothSlider();
    }

    public void UpdateStamina(int currentStamina)
    {
        time = 0;
        // staminaSlider.value = currentStamina;
        staminaText.text = Player.Instance.Stamina + " / " + staminaSlider.maxValue;

        float percentage = Mathf.InverseLerp(staminaSlider.minValue, staminaSlider.maxValue, Player.Instance.Stamina) * 100;

        if (percentage >= 60)
        {
            moodImage.sprite = moodSprite[0];
        }
        else if (percentage < 60 && percentage >= 30)
        {
            moodImage.sprite = moodSprite[1];
        }
        else if (percentage < 30 && percentage >= 0)
        {
            moodImage.sprite = moodSprite[2];
        }
    }

    public void IncraseMaxStamina(int currentMaxStamina)
    {
        staminaSlider.maxValue = currentMaxStamina;

        staminaText.text = staminaSlider.value.ToString() + " / " + staminaSlider.maxValue;
    }

    private void SmoothSlider()
    {
        time += Time.deltaTime * lerpSpeed;
        staminaSlider.value = Mathf.Lerp(staminaSlider.value, Player.Instance.Stamina, time);
    }
}
