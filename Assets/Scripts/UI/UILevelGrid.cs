using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILevelGrid : MonoBehaviour
{
    [SerializeField] UILevelWidget levelWidget;

    // Start is called before the first frame update
    void Start()
    {
        string key;
        int levelScore;
        int level=1;
        do
        {
            key = GameManager.PLAYER_PREF_KEY.Replace(GameManager.PLAYER_PREF_ID, level.ToString());
            levelScore = PlayerPrefs.GetInt(key, 0);

            if (levelScore > 0)
            {
                UILevelWidget newWidget = Instantiate(levelWidget, transform);
                newWidget.Init(level, levelScore);
            }
            level++;
        } while (levelScore > 0 && level<30);
    }
}
