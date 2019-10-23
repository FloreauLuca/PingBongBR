using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalGameManager : MonoBehaviour
{
    public const float PLAYER_RESPAWN_TIME = 4.0f;

    public const int PLAYER_MAX_LIVES = 3;

    public static Color GetColor(int colorChoice)
    {
        switch (colorChoice)
        {
            case 0: return Color.red;
            case 1: return Color.green;
            case 2: return Color.blue;
            case 3: return Color.yellow;
            case 4: return Color.cyan;
            case 5: return Color.grey;
            case 6: return Color.magenta;
            case 7: return Color.white;
        }

        return Color.black;
    }
}
