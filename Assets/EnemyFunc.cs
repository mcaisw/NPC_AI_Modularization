using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface EnemyFunc{
    void DoInMove();
    void DoInCd();
    void DoInAttack();
    void DoInFind();
    void DoDie();
    void UpDateBehavior();
    //新添加状态后，新的行为函数
    //void DoTaoZou();
}
