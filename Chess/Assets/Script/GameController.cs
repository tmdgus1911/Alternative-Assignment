using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameController : MonoBehaviour
{
    Object[] objects = new Object[32];
    public int[,] map = new int[ConstVar.MAP_HEIGHT + 1, ConstVar.MAP_WIDTH + 1];
    List<(GameObject, Vector2)> dots = new List<(GameObject, Vector2)>();

    public GameObject prefeb;
    public GameObject prefeb_dot;
    public GameObject game_message;

    int target;
    bool is_targeting = false;
    bool is_white_turn = true;

    void Start()
    {
        game_message.SetActive(false);
        for (int i = 1; i <= ConstVar.MAP_HEIGHT; i++)
        {
            for (int j = 1; j <= ConstVar.MAP_WIDTH; j++)
            {
                map[i, j] = -1;
            }
        }
        int cnt = 0;
        void AddObject(Object _object)
        {
            objects[cnt] = new Object();
            objects[cnt].Copy(_object);
            objects[cnt].is_white = true;
            objects[cnt].instance = Instantiate(prefeb);
            map[Tools.FloatToInt(objects[cnt].pos.y), Tools.FloatToInt(objects[cnt].pos.x)] = cnt;
            objects[cnt++].Init();

            objects[cnt] = new Object();
            objects[cnt].Copy(_object);
            objects[cnt].is_white = false;
            objects[cnt].pos = new Vector2(objects[cnt].pos.x, ConstVar.MAP_HEIGHT - objects[cnt].pos.y + 1);
            for (int i = 0; i < objects[cnt].delta_cnt; i++)
            {
                objects[cnt].deltapos[i] *= new Vector2(1, -1);
            }
            objects[cnt].instance = Instantiate(prefeb);
            map[Tools.FloatToInt(objects[cnt].pos.y), Tools.FloatToInt(objects[cnt].pos.x)] = cnt;
            objects[cnt++].Init();
        }

        for (int i = 0; i < ConstVar.MAP_WIDTH; i++)
        {
            AddObject(new Object
            {
                name = "pawn",

                pos = new Vector2(i + 1, 2),
                delta_cnt = 3,
                deltapos = new Vector2[] {
                    new Vector2(0,1),
                    new Vector2(1,1),
                    new Vector2(-1,1),
                },

                type = 2,
            });
        }

        AddObject(new Object
        {
            name = "rook",
            pos = new Vector2(1, 1),
            delta_cnt = 4,
            deltapos = new Vector2[]
            {
                new Vector2(1, 0),
                new Vector2(-1, 0),
                new Vector2(0, 1),
                new Vector2(0, -1),
            }
        });
        AddObject(new Object
        {
            name = "knight",
            pos = new Vector2(2, 1),
            delta_cnt = 8,
            deltapos = new Vector2[]
            {
                new Vector2(-2, 1),
                new Vector2(-1, 2),
                new Vector2(1, 2),
                new Vector2(2, 1),
                new Vector2(-2, -1),
                new Vector2(-1, -2),
                new Vector2(1, -2),
                new Vector2(2, -1),
            },
            type = 1,
        });
        AddObject(new Object
        {
            name = "bishop",
            pos = new Vector2(3, 1),
            delta_cnt = 4,
            deltapos = new Vector2[]
            {
                new Vector2(1, 1),
                new Vector2(-1, 1),
                new Vector2(1, -1),
                new Vector2(-1, -1),
            }
        });
        AddObject(new Object
        {
            is_white = true,
            name = "queen",
            pos = new Vector2(4, 1),
            delta_cnt = 8,
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
            }
        });
        AddObject(new Object
        {
            name = "king",
            pos = new Vector2(5, 1),
            delta_cnt = 8,
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
            },
            type = 1,
        });
        AddObject(new Object
        {
            name = "bishop",
            pos = new Vector2(6, 1),
            delta_cnt = 4,
            deltapos = new Vector2[]
            {
                new Vector2(1, 1),
                new Vector2(-1, 1),
                new Vector2(1, -1),
                new Vector2(-1, -1),
            }
        });
        AddObject(new Object
        {
            name = "knight",
            pos = new Vector2(7, 1),
            delta_cnt = 8,
            deltapos = new Vector2[]
            {
                new Vector2(-2, 1),
                new Vector2(-1, 2),
                new Vector2(1, 2),
                new Vector2(2, 1),
                new Vector2(-2, -1),
                new Vector2(-1, -2),
                new Vector2(1, -2),
                new Vector2(2, -1),
            },
            type = 1,
        });
        AddObject(new Object
        {
            name = "rook",
            pos = new Vector2(8, 1),
            delta_cnt = 4,
            deltapos = new Vector2[]
            {
                new Vector2(1, 0),
                new Vector2(-1, 0),
                new Vector2(0, 1),
                new Vector2(0, -1),
            }
        });
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !game_message.activeSelf)
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.zero, 0f);

            if (hit.collider != null)
            {
                Targeting(hit.collider.gameObject);
                int res = EndGame();
                if (res!=0)
                {
                    TMP_Text text = game_message.GetComponent<TMP_Text>();
                    if (res == 1) 
                    {
                        text.text = "Checkmate\n" + (is_white_turn?"Black":"White")+ " Win";
                    }
                    else
                    {
                        text.text = "Stellmate";
                    }
                    game_message.SetActive(true);
                }
            }
            else
            {
                is_targeting = false;
                foreach ((GameObject i, Vector2 _pos) in dots)
                {
                    Destroy(i);
                }
                dots.Clear();
            }
        }
    }

    public void Targeting(GameObject _gameobject)
    {
        if (is_targeting)
        {
            foreach ((GameObject i, Vector2 pos) in dots)
            {
                if (i == _gameobject)
                {
                    int tar = map[Tools.FloatToInt(pos.y), Tools.FloatToInt(pos.x)];

                    if (tar != -1)
                    {
                        objects[tar].is_live = false;
                        Destroy(objects[tar].instance);
                    }

                    map[Tools.FloatToInt(objects[target].pos.y), Tools.FloatToInt(objects[target].pos.x)] = -1;
                    map[Tools.FloatToInt(pos.y), Tools.FloatToInt(pos.x)] = target;
                    objects[target].Moving(pos);

                    //propotion
                    if(target < ConstVar.PAWN)
                    {
                        if((is_white_turn?ConstVar.MAP_HEIGHT:1) == objects[target].pos.y)
                        {
                            objects[target].Promotion();
                        }
                    }

                    is_white_turn = is_white_turn == true ? false : true;
                }
            }

            is_targeting = false;
            foreach ((GameObject i, Vector2 pos) in dots)
            {
                Destroy(i);
            }
            dots.Clear();
        }

        for (int l = 0; l < ConstVar.MAX_OBJECT; l++)
        {
            Object i = objects[l];
            if (i.instance == _gameobject && is_white_turn == (l % 2 == 0))
            {
                List<Vector2> res = CaseFind(l);
                foreach (Vector2 pos in res)
                {
                    if (!CaseAvailable(l, pos)) continue;
                    Vector2 posi = Tools.PosToPosition(pos);
                    dots.Add((Instantiate(prefeb_dot, new Vector3(posi.x,posi.y,-1), Quaternion.identity), pos));
                }

                is_targeting = true;
                target = l;
                break;
            }
        }
    }

    public List<Vector2> CaseFind(int l)
    {
        List<Vector2> res = new List<Vector2>();

        Object i = objects[l];

        for (int j = 0; j < i.delta_cnt; j++)
        {
            Vector2 pos;
            int tar;
            switch (i.type)
            {
                case 0:
                    for (int k = 1; k < System.Math.Max(ConstVar.MAP_WIDTH, ConstVar.MAP_HEIGHT); k++)
                    {
                        if (!i.CanMove(j, k)) break;

                        pos = i.MovePos(j, k);

                        tar = map[Tools.FloatToInt(pos.y), Tools.FloatToInt(pos.x)];
                        if (tar != -1 && l % 2 == tar % 2) break;

                        res.Add(pos);

                        if (tar != -1 && l % 2 != tar % 2) break;
                    }
                    break;
                case 1:
                    if (!i.CanMove(j, 1)) break;

                    pos = i.MovePos(j, 1);

                    tar = map[Tools.FloatToInt(pos.y), Tools.FloatToInt(pos.x)];
                    if (tar != -1 && l % 2 == tar % 2) break;

                    res.Add(pos);

                    break;
                case 2:
                    if (j == 0)
                    {
                        for (int k = 1; k <= (i.is_move == false ? 2 : 1); k++)
                        {
                            if (!i.CanMove(j, k)) break;

                            pos = i.MovePos(j, k);

                            tar = map[Tools.FloatToInt(pos.y), Tools.FloatToInt(pos.x)];
                            if (tar != -1) break;

                            res.Add(pos);
                        }
                    }
                    else
                    {
                        if (!i.CanMove(j, 1)) break;

                        pos = i.MovePos(j, 1);

                        tar = map[Tools.FloatToInt(pos.y), Tools.FloatToInt(pos.x)];
                        if (tar == -1 || l % 2 == tar % 2) break;

                        res.Add(pos);
                    }
                    break;
            }
        }

        return res;
    }
    public bool CaseAvailable(int tar, Vector2 pos)
    {
        //교환도 문제임. 먹는거면 어칼려고
        //map만 바꾸면 안됨. 세부정보도 같이.
        int last = map[Tools.FloatToInt(pos.y), Tools.FloatToInt(pos.x)];
        Vector2 last_pos = objects[tar].pos;

        map[Tools.FloatToInt(objects[tar].pos.y), Tools.FloatToInt(objects[tar].pos.x)] = -1;
        map[Tools.FloatToInt(pos.y), Tools.FloatToInt(pos.x)] = tar;

        objects[tar].pos = pos;

        if(last != -1)
        {
            objects[last].is_live = false;
        }

        bool res = IsCheck(is_white_turn);

        map[Tools.FloatToInt(pos.y), Tools.FloatToInt(pos.x)] = last;
        map[Tools.FloatToInt(last_pos.y), Tools.FloatToInt(last_pos.x)] = tar;

        objects[tar].pos = last_pos;

        if (last != -1)
        {
            objects[last].is_live = true;
        }

        return !res;
    }

    public bool IsCheck(bool attack_white)
    {
        List<Vector2> res = new List<Vector2>();
        for (int i = (!attack_white ? 0 : 1); i < ConstVar.MAX_OBJECT; i+=2)
        {
            if (!objects[i].is_live) continue;
            res.AddRange(CaseFind(i));
        }
        foreach (Vector2 pos in res)
        {
            if (attack_white && map[Tools.FloatToInt(pos.y), Tools.FloatToInt(pos.x)] == ConstVar.WHITE_KING)return true;
            if (!attack_white && map[Tools.FloatToInt(pos.y), Tools.FloatToInt(pos.x)] == ConstVar.BLACK_KING)return true;
        }
        return false;
    }
    public int EndGame()
    {
        List<Vector2> res = new List<Vector2>();
        List<Vector2> res2 = new List<Vector2>();
        for (int i = (is_white_turn ? 0 : 1); i < ConstVar.MAX_OBJECT; i+=2)
        {
            if (!objects[i].is_live) continue;
            res.Clear();
            res.AddRange(CaseFind(i));
            foreach (Vector2 pos in res)
            {
                if (CaseAvailable(i,pos))res2.Add(pos);
            }
        }

        if(res2.Count == 0)
        {
            if (IsCheck(is_white_turn)) return 1; //체크메이트
            else return 2; //스텔메이트
        }
        return 0;
    }
}
