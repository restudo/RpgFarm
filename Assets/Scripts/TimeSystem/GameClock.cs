using TMPro;
using UnityEngine;


public class GameClock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText = null;
    [SerializeField] private TextMeshProUGUI dateText = null;
    [SerializeField] private TextMeshProUGUI seasonText = null;
    [SerializeField] private TextMeshProUGUI gameDayOfWeekText = null;


    private void OnEnable()
    {
        EventHandler.AdvanceGameMinuteEvent += UpdateGameTime;
    }

    private void OnDisable()
    {
        EventHandler.AdvanceGameMinuteEvent -= UpdateGameTime;
    }

    private void UpdateGameTime(int gameYear, Season gameSeason, int gameDay, string gameDayOfWeek, int gameHour, int gameMinute, int gameSecond)
    {
        // Update time

        gameMinute = gameMinute - (gameMinute % 10);

        string ampm = "";
        string minute;
        string day;

        if (gameHour >= 12)
        {
            ampm = " pm";
        }
        else
        {
            ampm = " am";
        }

        if (gameHour >= 13)
        {
            gameHour -= 12;
        }

        if (gameMinute < 10)
        {
            minute = "0" + gameMinute.ToString();
        }
        else
        {
            minute = gameMinute.ToString();
        }

        if (gameDay < 10)
        {
            day = "0" + gameDay.ToString();
        }
        else
        {
            day = gameDay.ToString();
        }

        string time = gameHour.ToString() + " : " + minute + ampm;


        seasonText.SetText(gameSeason.ToString());
        dateText.SetText("Tgl. " + day);
        timeText.SetText(time);
        gameDayOfWeekText.SetText(gameDayOfWeek);
    }

}
