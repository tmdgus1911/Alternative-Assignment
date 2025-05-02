using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static UnityEditor.PlayerSettings;

class Object : MonoBehaviour
{
    public bool is_white { get; set; }

    public string name { get; set; }

    public Vector2 pos { get; set; }

    public int delta_cnt { get; set; }
    public Vector2[] deltapos { get; set; }


    public bool is_live { get; set; } = true;
    public bool is_move { get; set; } = false;
    public bool is_promotion { get; set; } = false;

    public int type { get; set; } = 0; //0:normal 1:only one move 2:pawn

    public GameObject instance { get; set; }

    public void Init()
    {
        instance.GetComponent<ObjectDetails>().target_name = (is_white ? "w_" : "b_") + name;
        Vector2 posi = Tools.PosToPosition(pos);
        instance.transform.position = new Vector3(posi.x, posi.y, 0);
    }
    public void Copy(Object _object)
    {
        is_white = _object.is_white;

        name = _object.name;

        pos = _object.pos;

        delta_cnt = _object.delta_cnt;
        deltapos = new Vector2[delta_cnt];
        for (int i = 0; i < delta_cnt; i++)
        {
            deltapos[i] = _object.deltapos[i];
        }
        
        is_live = _object.is_live;
        is_move = _object.is_move;

        type = _object.type;

        instance = _object.instance;
    }
    public void Promotion()
    {
        if (is_promotion) return;
        name = "queen";
        delta_cnt = 8;
        deltapos = new Vector2[]
        {
            new Vector2(1, 1),
            new Vector2(-1, 1),
            new Vector2(1, -1),
            new Vector2(-1, -1),
            new Vector2(1, 0),
            new Vector2(-1, 0),
            new Vector2(0, 1),
            new Vector2(0, -1),
        };
        type = 0;
        is_promotion = true;
        Reloaded();
    }
    void Reloaded()
    {
        instance.GetComponent<ObjectDetails>().ReLoadSprite((is_white ? "w_" : "b_") + name);
    }
    public bool CanMove(int i, int mult)
    {
        if (i >= delta_cnt) return false;
        Vector2 new_pos = pos + deltapos[i] * mult;

        if (Math.Min(new_pos.x, new_pos.y) <= 0 || new_pos.x > ConstVar.MAP_WIDTH || new_pos.y > ConstVar.MAP_HEIGHT) return false;
        return true;
    }
    public Vector2 MovePos(int i, int mult)
    {
        Vector2 new_pos = pos + deltapos[i] * mult;
        return new_pos;
    }
    public void Moving(Vector2 _pos)
    {
        pos = _pos;
        is_move = true;
        instance.transform.position = Tools.PosToPosition(_pos);
    }
}


class Tools : MonoBehaviour
{
    public static Vector2 PosToPosition(Vector2 pos)
    {
        return new Vector2(pos.x - ConstVar.MAP_CELL * 4.5f, pos.y - ConstVar.MAP_CELL * 4.5f);
    }
    public static void Swap(ref int a, ref int b)
    {
        (a, b) = (b, a);
    }
    public static int FloatToInt(float a)
    {
        return (int)Math.Round(a);
    }
}