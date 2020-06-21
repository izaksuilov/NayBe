using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BankController : MonoBehaviour
{
    public GameObject playerPref;
    void Start()
    {
        Input.multiTouchEnabled = false;
        playerPref.GetComponent<PlayerControl>().enabled = false;
        playerPref.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = Settings.nickName;

        RawImage image = playerPref.transform.GetChild(1).GetComponent<RawImage>();
        string directoryPath = Application.persistentDataPath + "/avatar.jpg";
        try { image.texture = NativeGallery.LoadImageAtPath(directoryPath); }
        catch { image.texture = Resources.Load<Texture2D>("icons/Avatar1"); }

        Instantiate(playerPref, GameObject.Find("PlayerPosition").transform);

    }

    // Update is called once per frame
    void Update()
    {
        if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape))
            Leave();
    }
    public void Leave()
    {
        SceneManager.LoadScene(0);
    }
}
