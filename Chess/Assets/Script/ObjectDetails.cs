using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class ObjectDetails : MonoBehaviour
{
    public string target_name = null;
    void Start()
    {
        IEnumerator SetTexture()
        {
            yield return new WaitUntil(() => target_name!=null);
            LoadSprite();
        }

        StartCoroutine(SetTexture());
    }
    public void ReLoadSprite(string _name)
    {
        target_name = _name;
        LoadSprite();
    }
    void LoadSprite()
    {
        Sprite target_sprite = Resources.Load<Sprite>(target_name);
        GetComponent<SpriteRenderer>().sprite = target_sprite;
    }
}
