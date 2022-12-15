using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaController : SingletonMonobehaviour<StaminaController>
{
    [SerializeField] private Slider staminaSlider;
    [SerializeField] private Sprite[] moodSprite;
    [SerializeField] private GameObject moodObj;
    private Image moodImage;

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
        if (Player.Instance.Stamina >= 60)
        {
            moodImage.sprite = moodSprite[0];
        }
        else if (Player.Instance.Stamina < 60 && Player.Instance.Stamina >= 30)
        {
            moodImage.sprite = moodSprite[1];
        }
        else if (Player.Instance.Stamina < 30 && Player.Instance.Stamina >= 0)
        {
            moodImage.sprite = moodSprite[2];
        }
    }

    public void IncraseStamina(int currentStamina)
    {
        staminaSlider.value = currentStamina;

        if (Player.Instance.Stamina >= 60)
        {
            moodImage.sprite = moodSprite[0];
        }
        else if (Player.Instance.Stamina < 60 && Player.Instance.Stamina >= 30)
        {
            moodImage.sprite = moodSprite[1];
        }
        else if (Player.Instance.Stamina < 30 && Player.Instance.Stamina >= 0)
        {
            moodImage.sprite = moodSprite[2];
        }
    }

    public void IncraseMaxStamina(int currentMaxStamina)
    {
        staminaSlider.maxValue = currentMaxStamina;
    }
}
