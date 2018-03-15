using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

/*------------------------------------------------怪物的基类-----------------------------------------
 说明：所有的怪物继承这个基类，并继承接口EnemyFunc.

    按K,发现玩家。
    按L，玩家离开。
    按H，怪物受伤。
    SimulateInGame()接收键盘按键，模拟怪物发现玩家，被玩家攻击，玩家离开
*/

[System.Serializable]
public struct Parameter//怪物参数
{
    [Header("速度")]
    public int speed;

    [Header("cd时间")]
    public float cdTime;

    [Header("巡逻时间")]
    public float patrolTime;

    [Header("范围")]
    public float range;

    public int Hp;      //血值
}

/*----------怪物状态枚举---------
    有新的怪物被添加进来时，有要添加的状态，在这里进行添加，
    并且更改UpDateBehavior()函数里的状态刷新,在接口EnemyFunc 里添加新的函数DoTaoZou();
    比如：新增加呆萌，有新的状态->逃走，
     */
public enum State
{
    none,
    move,
    cd,
    attack,
    faxian,
    die,
    //新添加状态
    //taoZou,
}

public class EnemyAI : MonoBehaviour
{
    [Header("将人物控制器拖入")]
    public CharacterController characterCtr;

    public          Animation   animation;                  //动画组件
    public          GameObject  player;                     //玩家
    public          GameObject  HpCanvas;                   //血值画布
    public          GameObject  HpValue;                    //血值slider
    
    
    public          Parameter   enemyData;                  //怪物参数
    public          EnemyFunc   enmeyfunc;                  //怪物函数接口

    protected       State       currentState = State.move;  //怪物默认状态

    protected       float       timer;                      //计时器
    protected       Vector3     patrolDir;                  //巡逻方向
    private         Vector3     birthPoint;                 //出生点
    private         float       leftPoint;                  //左边点
    private         float       rightPoint;                 //右边点
    protected       RaycastHit  hit;                        //射线检测输出

  
    
    private void Awake()
    {
        enmeyfunc = GetComponent<EnemyFunc>();
    }
    public virtual void Start()
    {
        patrolDir = new Vector3(enemyData.speed, 0,0);
        birthPoint = GetBirthPoint();
        leftPoint = birthPoint.x - enemyData.range;
        rightPoint = birthPoint.x + enemyData.range;

        HpValue.GetComponent<Slider>().maxValue = enemyData.Hp;
        HpValue.GetComponent<Slider>().minValue = 0;
        HpValue.GetComponent<Slider>().value = enemyData.Hp;
    }
    public virtual void Update()
    {
        //让怪物落在地上
        MakeCharacterOnGround();
        if (characterCtr.isGrounded)
        {
            //探测悬崖的线
            Debug.DrawRay(new Vector3(characterCtr.transform.position.x + GetDir() * 1f, characterCtr.transform.position.y, characterCtr.transform.position.z), Vector3.down, Color.red);
            Debug.DrawRay(new Vector3(characterCtr.transform.position.x, characterCtr.transform.position.y+0.5f, characterCtr.transform.position.z), Vector3.right*GetDir(), Color.red);
            enmeyfunc.UpDateBehavior();

        }
    }

    //怪物的行为入口
    public virtual void UpDateBehavior() {
        switch (currentState)
        {
            case State.none:
                break;
            case State.move:
                enmeyfunc.DoInMove();
                break;
            case State.cd:
                enmeyfunc.DoInCd();
                break;
            case State.attack:
                enmeyfunc.DoInAttack();
                break;
            case State.faxian:
                enmeyfunc.DoInFind();
                break;
            case State.die:
                enmeyfunc.DoDie();
                break;
            //新添加的状态
            //case State.taoZou:
            // enmeyfunc.DoTaoZou();
            //    break;
            default:
                break;
        }

        /*                      鼠标测试                        */
        SimulateInGame();
    }

    /*----------------------------------------按键模拟-------------------------------------------------*/

    //按键模拟
    public virtual void SimulateInGame() {
        //模拟遇到玩家
        if (Input.GetKeyDown(KeyCode.K))
        {
            //面向玩家
            SetDir(GetDir(player));
            SetState(State.faxian, () => {
                this.transform.parent.DOShakeScale(2f, 1, 10, 50).OnComplete(() => {
                    Debug.Log("进入攻击");
                    SetState(State.attack);
                });
            });
        }
        //模拟玩家离开
        if (Input.GetKeyDown(KeyCode.L))
        {
            characterCtr.transform.GetChild(0).rotation = Quaternion.identity;
            SetState(State.move, () => {
                SetDir(-1);
            });
        }



        if (Input.GetKeyDown(KeyCode.H))
        {
            Debug.Log("受伤");
            GetHit();
        }
    }

    /*----------------------------------------功能函数（不用重写）---------------------------------------------------------------*/
    //检测悬崖和墙壁
    protected void CheckCliffAndWall() {
        //悬崖射线
        if (!Physics.Raycast(new Vector3(characterCtr.transform.position.x + GetDir() * 1f, characterCtr.transform.position.y, characterCtr.transform.position.z),
       Vector3.down, 1))
        {
            //转向
            SetDir(-1 * GetDir());
        }

        //墙壁射线
        if (Physics.Raycast(new Vector3(characterCtr.transform.position.x, characterCtr.transform.position.y + 0.5f, characterCtr.transform.position.z),
       Vector3.right * GetDir(), out hit, 1))
        {
            if (!hit.collider.isTrigger)
                //转向
                SetDir(-1 * GetDir());
        }
    }

    //怪物掉在地面上
    protected void MakeCharacterOnGround()
    {
        if (!characterCtr.isGrounded)
            characterCtr.Move(Vector3.down);
    }

    //总是在范围内巡逻
    public void AlwayPatrolInRange()
    {
        if (GetDir() == 1)
        {
            if (characterCtr.transform.position.x >= rightPoint)
            {
                SetDir(-1);
            }
        }
        else
        {
            if (characterCtr.transform.position.x <= leftPoint)
            {
                SetDir(1);
            }
        }

    }

    //始终面向朝玩家
    protected void AlwaysOrientedPlayer() {
        if (GetDir() * GetDir(player) == -1)
        {
            SetDir(-1 * GetDir());
        }
    }

    //获得出生点
    private Vector3 GetBirthPoint() {
        return characterCtr.transform.position;
    }

    //获得自己的朝向
    protected int GetDir() {
        if (characterCtr.transform.rotation == Quaternion.Euler(0, 90, 0))
        {
            return -1;//往左
        }
        else if (characterCtr.transform.rotation == Quaternion.Euler(0, 270, 0))
        {
            return 1;//往右
        }
        else
        {
            return 0;
        }
    }

    //获得向玩家移动的方向
    protected int GetDir(GameObject player) {
        if (characterCtr.transform.position.x-player.transform.position.x>0)
        {
            return -1;//向左转
        }
        else
        {
            return 1;//向右转
        }

    }

    protected void SetDir(int dir) {
        if (dir == -1)
        {
            characterCtr.transform.rotation = Quaternion.Euler(0, 90, 0);
            HpCanvas.transform.rotation = Quaternion.identity;
        } else if (dir==1) {
            characterCtr.transform.rotation = Quaternion.Euler(0, 270, 0);
            HpCanvas.transform.rotation = Quaternion.identity;
        }
    }

    /*-------------------------------------其他函数（根据不同的怪物更改不同的参数）----------------------------------------------------------------*/
    //受到攻击
    public virtual void GetHit()
    {
        if (enemyData.Hp <= 0)
        {
            enemyData.Hp = 0;
            HpValue.GetComponent<Slider>().value = enemyData.Hp;
            SetState(State.die, () => {
                Destroy(this.transform.parent.gameObject, 0.5f);
            });
            return;
        }
        enemyData.Hp -= 6;
        HpValue.GetComponent<Slider>().value = enemyData.Hp;
    }
    /// <summary>
    /// 切换到新的状态
    /// </summary>
    /// <param name="下一个新状态"></param>
    /// <param name="切换到这个新的状态时,执行代码以委托方式传入"></param>
    public virtual void SetState( State nextState,Action doWhileChangeToThisState=null) {
        if (currentState == nextState)
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
                if (doWhileChangeToThisState!=null)
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
    //切换状态之前执行
    public virtual void DoBeforeChangeState() {
        //还原计时器
        timer = 0;
    }

}
