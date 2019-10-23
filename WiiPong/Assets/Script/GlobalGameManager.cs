using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameManager : MonoBehaviour
{

    public const float PLAYER_RESPAWN_TIME = 4.0f;

    public const int PLAYER_MAX_LIVES = 3;

    public static Color GetColor(int colorChoice)
    {
        if (colorChoice > 12)
        {
            colorChoice = colorChoice % 12;
        }
        switch (colorChoice)
        {
            case -1: return Color.black;
            case 0: return Color.red;
            case 1: return Color.green;
            case 2: return Color.blue;
            case 3: return Color.yellow;
            case 4: return Color.cyan;
            case 5: return Color.grey;
            case 6: return Color.magenta;
            case 7: return Color.white;
            case 8: return new Color(97, 0, 125);
            case 9: return new Color(255, 125, 0);
            case 10: return new Color(255, 162, 0);
            case 11: return new Color(0, 125, 255);
            case 12: return new Color(0, 255, 125);
        }

        return Color.black;
    }
}
