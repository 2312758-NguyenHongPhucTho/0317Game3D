using System.Collections;
using TMPro;
using UnityEngine;

public class TextEffect : MonoBehaviour
{
    public TMP_Text text;
    public float fadeSpeed = 1f;
    public float displayTime = 5f;

    void Start()
    {
        StartCoroutine(ShowText());
    }

    IEnumerator ShowText()
    {
        // Bắt đầu trong suốt
        Color color = text.color;
        color.a = 0;
        text.color = color;

        // Hiện từ từ
        while (text.color.a < 1)
        {
            color = text.color;
            color.a += fadeSpeed * Time.deltaTime;
            text.color = color;

            yield return null;
        }

        // Giữ chữ 5 giây
        yield return new WaitForSeconds(displayTime);

        // Tắt từ từ
        while (text.color.a > 0)
        {
            color = text.color;
            color.a -= fadeSpeed * Time.deltaTime;
            text.color = color;

            yield return null;
        }

        text.gameObject.SetActive(false);
    }
}