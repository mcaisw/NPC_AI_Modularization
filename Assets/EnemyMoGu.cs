using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;

public class EnemyMoGu : EnemyAI,EnemyFunc
{
    [Header("跳起的动画")]
    [SerializeField]
    AnimationClip       jumpClip;

    [Header("跳起的时间")]
    [SerializeField]
    float               jumpStartTime;

    [Header("落下的时间")]
    [SerializeField]
    float               jumpEndTime;

    bool                jump;               //动画是否播放到跳起
    bool                turnBack;           //在攻击状态时，是否碰到悬崖或墙壁需要转身
    bool                playerLeave;        //玩家是否离开


    // Use this for initialization
    public override void Start () {
        base.Start();
        //针对跳起的动画，添加事件
        AddAniamtionEvent.AddEvent(jumpClip, "JumpUp", jumpStartTime);
        AddAniamtionEvent.AddEvent(jumpClip, "JumpDowm", jumpEndTime);
    }
	
	// Update is called once per frame
	public override void Update () {
        base.Update();
	}

    /*-----------------------------------怪物行为刷新--------------------------------------------------*/
    public void DoInMove()
    {
        if (jump)
        {
            //移动    
            characterCtr.Move(GetDir() * patrolDir * Time.deltaTime);
            //射线检测     
            CheckCliffAndWall();
            //计时
            timer += Time.deltaTime;
            if (timer > enemyData.cdTime)
            {
                SetState(State.cd,()=> {
                    animation.CrossFade("idle01",0.1f);
                });
            }
        }
    }
    public void DoInCd()
    {
        //计时
        timer += Time.deltaTime;
        if (timer > enemyData.cdTime)
        {
            SetState(State.move,()=> {
                animation.CrossFade("run01", 0.1f);
            });
        }
    }
    public void DoInAttack()
    {
        //第一次攻击完毕
        if (!animation.isPlaying)
        {
            //再进行攻击
            animation.CrossFade("attack01", 0.1f);
            //然后走路
            animation.CrossFadeQueued("run01", 0.1f);
        }
       
        //跳起来
        if (jump)
        {
            if(Math.Abs(characterCtr.transform.position.x-player.transform.position.x)>1f)
                //移动    
                characterCtr.Move(GetDir() * patrolDir * Time.deltaTime);

            //没有碰到墙或悬崖
            if(!turnBack)
                AlwaysOrientedPlayer(); //始终面向玩家

            #region 攻击状态下射线检测
            //悬崖射线
            if (!Physics.Raycast(new Vector3(characterCtr.transform.position.x + GetDir() * 1f, characterCtr.transform.position.y, characterCtr.transform.position.z),
           Vector3.down, 1))
            {
                //转向
                SetDir(-1 * GetDir()); turnBack = true;

                //两秒后
                float time = 0;
                DOTween.To(() => time, x => time = x,1,2).OnComplete(()=> {
                    turnBack = false;
                });
            }

            //墙壁射线
            if (Physics.Raycast(new Vector3(characterCtr.transform.position.x, characterCtr.transform.position.y + 0.5f, characterCtr.transform.position.z),
           Vector3.right * GetDir(), out hit, 1))
            {
                if (!hit.collider.isTrigger)
                    //转向
                    SetDir(-1 * GetDir()); turnBack = true;
                    //两秒后
                    float time = 0;
                    DOTween.To(() => time, x => time = x, 1, 2).OnComplete(() => {
                        turnBack = false;
                    });
            }
            #endregion

            //计时
            timer -= Time.deltaTime;
        }

        //cd完成后
        if (timer<=0)
        {
            //再进入攻击
            SetState(State.attack, () => {
                animation.CrossFade("attack01", 0.1f);
                timer = 1;
                AlwaysOrientedPlayer();
            });
        }

        //玩家离开
        if (playerLeave&&!animation.IsPlaying("attack01"))
        {
            SetState(State.move, () => {
                SetDir(-1);
            });
        }
    }
    public void DoInFind()
    {
    }
    public void DoDie()
    {
    }
    /*-----------------------------------重写的函数---------------------------------------------------*/

    public override void SimulateInGame()
    {

        if (Input.GetKeyDown(KeyCode.K)) {
            SetDir(GetDir(player));
            SetState(State.faxian,()=> {
                animation.CrossFade("faxian01",0.1f);
                float time = 0;
                DOTween.To(() => time, x => time = x, 1, animation.GetClip("faxian01").length).OnComplete(()=> {
                    SetState(State.attack,()=> {
                        animation.CrossFade("attack01", 0.1f);
                        timer = 1;
                        AlwaysOrientedPlayer();
                    });
                });
            });
        }

        //模拟玩家离开
        if (Input.GetKeyDown(KeyCode.L))
        {
            playerLeave = true;
            
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("受伤");
            GetHit();
        }
    }
    public override void GetHit()
    {
        if (enemyData.Hp <= 0)
        {
            enemyData.Hp = 0;
            HpValue.GetComponent<Slider>().value = enemyData.Hp;
            SetState(State.die, () => {
                animation.CrossFade("die01",0.1f);
                Destroy(this.transform.parent.gameObject, 1f);
            });
            return;
        }
        enemyData.Hp -= 6;
        HpValue.GetComponent<Slider>().value = enemyData.Hp;
    }
    public override void DoBeforeChangeState()
    {
        base.DoBeforeChangeState();
        jump = false;
        playerLeave = false;
    }
    public override void SetState(State nextState, Action doWhileChangeToThisState = null)
    {
        if (currentState == nextState&&currentState!=State.attack)
        {
            return;
        }
        DoBeforeChangeState();
        currentState = nextState;
        switch (currentState)
        {
            case State.none:
                break;
            case State.move:
                if (doWhileChangeToThisState != null)
                    doWhileChangeToThisState();
                break;
            case State.cd:
                if (doWhileChangeToThisState != null)
                    doWhileChangeToThisState();
                break;
            case State.attack:
                if (doWhileChangeToThisState != null)
                    doWhileChangeToThisState();
                break;
            case State.faxian:
                if (doWhileChangeToThisState != null)
                    doWhileChangeToThisState();
                break;
            case State.die:
                if (doWhileChangeToThisState != null)
                    doWhileChangeToThisState();
                break;
            default:
                break;
        }
    }

    /*----------------------------------注册的动画帧事件----------------------------------------------*/
    void JumpUp()
    {
        jump = true;
    }
    void JumpDowm()
    {
        jump = false;
    }
}

//给动画片段添加事件
public class AddAniamtionEvent {
    public static void AddEvent(AnimationClip clip, string functionName, float time)
    {
        AnimationEvent[] events = clip.events;
        foreach (AnimationEvent item in events)
        {
            if (item.functionName == functionName)
                return;
        }
        AnimationEvent animationEvent = new AnimationEvent();
        animationEvent.functionName = functionName;
        animationEvent.time = time;
        clip.AddEvent(animationEvent);
    }
}

