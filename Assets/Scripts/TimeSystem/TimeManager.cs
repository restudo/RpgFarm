using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : SingletonMonobehaviour<TimeManager>
{
    private int gameYear = 1;
    private Season gameSeason = Season.Kemarau;
    private int gameDay = 1;
    public int GameDay { get => gameDay; set => gameDay = value; }
    private int gameDayUnreset = 1;
    private int gameHour = 6;
    public int GameHour { get => gameHour; set => gameHour = value; }
    private int gameMinute = 30;
    private int gameSecond = 0;
    private string gameDayOfWeek = "Senin";

    private bool gameClockPaused = false;

    private float gameTick = 0f;

    private void Start()
    {
        EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
    }

    private void Update()
    {
        if (!gameClockPaused)
        {
            GameTick();
        }
    }

    private void GameTick()
    {
        gameTick += Time.deltaTime;

        if (gameTick >= Settings.secondsPerGameSecond)
        {
            gameTick -= Settings.secondsPerGameSecond;

            UpdateGameSecond();
        }
    }

    private void UpdateGameSecond()
    {
        gameSecond++;

        if (gameSecond > 59)
        {
            gameSecond = 0;
            gameMinute++;

            if (gameMinute > 59)
            {
                gameMinute = 0;
                gameHour++;

                if (gameHour > 23)
                {
                    gameHour = 0;
                    gameDay++;
                    gameDayUnreset++;

                    if (gameDay > 30)
                    {
                        gameDay = 1;

                        int gs = (int)gameSeason;
                        gs++;

                        gameSeason = (Season)gs;

                        if (gs > 1)
                        {
                            gs = 0;
                            gameSeason = (Season)gs;

                            gameYear++;

                            if (gameYear > 9999)
                                gameYear = 1;


                            EventHandler.CallAdvanceGameYearEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                        }

                        EventHandler.CallAdvanceGameSeasonEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                    }

                    gameDayOfWeek = GetDayOfWeek();
                    EventHandler.CallAdvanceGameDayEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
                }

                EventHandler.CallAdvanceGameHourEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);
            }

            EventHandler.CallAdvanceGameMinuteEvent(gameYear, gameSeason, gameDay, gameDayOfWeek, gameHour, gameMinute, gameSecond);

        }

        // Call to advance game second event would go here if required
    }

    private string GetDayOfWeek()
    {
        // int totalDays = (((int)gameSeason) * 30) + gameDay;
        int dayOfWeek = gameDayUnreset % 7;

        switch (dayOfWeek)
        {
            case 1:
                return "Senin";

            case 2:
                return "Selasa";

            case 3:
                return "Rabu";

            case 4:
                return "Kamis";

            case 5:
                return "Jumat";

            case 6:
                return "Sabtu";

            case 0:
                return "Minggu";

            default:
                return "";
        }
    }

    //TODO:Remove
    /// <summary>
    /// Advance 1 game minute
    /// </summary>
    public void TestAdvanceGameMinute()
    {
        for (int i = 0; i < 60; i++)
        {
            UpdateGameSecond();
        }
    }

    //TODO:Remove
    /// <summary>
    /// Advance Penalty Day
    /// </summary>
    public void TestAdvancePenaltyGameDay()
    {
        for (int i = 0; i < 86400; i++)
        {
            if (gameHour == 11 && gameMinute >= 59)
            {
                break;
            }
            UpdateGameSecond();
        }
    }

    //TODO:Remove
    /// <summary>
    /// Advance Normal Day
    /// </summary>
    public void TestAdvanceNormalGameDay()
    {
        for (int i = 0; i < 86400; i++)
        {
            if (gameHour == 5 && gameMinute >= 59)
            {
                break;
            }
            UpdateGameSecond();
        }
    }
}
