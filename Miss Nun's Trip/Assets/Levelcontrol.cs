using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Levelcontrol : MonoBehaviour
{
    public void LevelChange(int Level)
    {
        Level +=1;
        SceneManager.LoadScene(Level);
    }

}
