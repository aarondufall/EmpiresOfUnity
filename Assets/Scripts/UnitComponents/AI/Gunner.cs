﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//--   A.I. to improve shooting-behavior of AttackUnits
[AddComponentMenu("Program-X/UNIT/AI - Gunner")]
public class Gunner : UnitComponent
{
    public override string IDstring
    {
        get
        {
            return "Rambo";
        }
    }
    public bool FireAtWill;
    public SortedDictionary<Vector3, float> Targets = new SortedDictionary<Vector3, float>();
    [SerializeField]
    private bool _battle = false;
    public bool Battle
    {
        get
        {
            return _battle;
        }
        set
        {
            _battle = value;
            if (this.gameObject.GetComponent<Pilot>())
                this.GetComponent<Pilot>().PerceptionIsGunnerControlled = _battle;
        }
    }
    public float MAXIMUM_ATTACK_RANGE
    {
        get { return weapon.GetMaximumRange(); }
    }
    public UnitWeapon weapon;
    public SphereCollider MySpace;

    public EnumProvider.ORDERSLIST OrderState
    {
        get { return (EnumProvider.ORDERSLIST)UNIT.Options.UnitState; }
        set { UNIT.Options.UnitState = value; }
    }
    public EnumProvider.ORDERSLIST BattleState = EnumProvider.ORDERSLIST.Patrol;
    void Awake()
    {
        weapon = GetComponent<UnitWeapon>();
        if (!gameObject.GetComponent<SphereCollider>())
            gameObject.AddComponent<SphereCollider>().isTrigger = true;

    }

    void Start()
    {
        MySpace = GetComponent<SphereCollider>();
        PflongeOnUnit();
    }

    public override void DoUpdate()
    {
        if (Battle) Battle = Fight();
    }

    private bool Fight()
    {
        if (FireAtWill)
        {
            float nearest = MAXIMUM_ATTACK_RANGE;
            Vector3 index = Vector3.zero;
            Debug.Log("enter Fight");
            foreach (Vector3 position in Targets.Keys)
            {
                if (nearest > Targets[position])
                {
                    nearest = Targets[position];
                    index = position;
                }
            }
            weapon.Engage(index);
        }
        return ((UNIT.IsUnderAttack) || (this.gameObject.GetComponent<Attackability>().IsAttacking) || (Targets.Count > 0));
    }

    private float MaximizePerseptionRadius()
    {
        if (((EnumProvider.ORDERSLIST)UNIT.Options.UnitState) != EnumProvider.ORDERSLIST.Hide)
        {
            if (this.gameObject.GetComponent<Pilot>())
                this.gameObject.GetComponent<Pilot>().PerceptionIsGunnerControlled = true;
            
            return MySpace.radius = weapon.GetMaximumRange();
        }
        else
            return MySpace.radius;

    }

    void OnTriggerEnter(Collider other)
    {
        if (UNIT.IsEnemy(other.gameObject))
        {
            //Debug.Log("Enemy entered Trigger !");
            MaximizePerseptionRadius();
            //Debug.Log(weapon.GetMaximumRange().ToString());

            if (UNIT.ALARM < UnitScript.ALLERT_LEVEL.A)
                UNIT.ALARM++;
            //Debug.Log("Alarm ok");
            //Debug.Log(UNIT.ALARM.ToString());
            if (FireAtWill)
            {
                //Debug.Log("IfFireAtWill");
                if (weapon.IsOutOfAmu)
                {
                    //Debug.Log("out of amu");
                    UNIT.Options.UnitState = EnumProvider.ORDERSLIST.Hide;
                    if (!UNIT.IsABuilding)
                        UNIT.Options.FocussedLeftOnGround(-(other.gameObject.transform.position - this.transform.position));
                }

                //Debug.Log("WeaponEngaged");
                weapon.Engage(other.gameObject);
                    
              
            }

            switch ((EnumProvider.ORDERSLIST)OrderState)
            {
                case EnumProvider.ORDERSLIST.Attack:
                    
                    break;
                case EnumProvider.ORDERSLIST.Guard:

                    break;
                case EnumProvider.ORDERSLIST.Patrol:
                    //Debug.Log("PatrolCase");
                    UNIT.ALARM = UnitScript.ALLERT_LEVEL.A;
                    break;
                case EnumProvider.ORDERSLIST.Seek:

                    break;
                case EnumProvider.ORDERSLIST.Hide:
                    //Debug.Log("HideCase");
                    if (!UNIT.IsABuilding)
                        UNIT.Options.FocussedLeftOnGround(-(other.gameObject.transform.position - this.transform.position));
                    break;

            }
            //Debug.Log("after switch");
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (UNIT.IsEnemy(other.gameObject))
        {
            if (FireAtWill)
            {
                float distance;
                Vector3 targetPosition = other.transform.position;
                if (Targets.TryGetValue(targetPosition, out distance))
                    Targets[targetPosition] = Vector3.Distance(other.transform.position, this.transform.position);
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (UNIT.IsEnemy(other.gameObject))
        {
            UNIT.ALARM--;
            Targets.Remove(other.transform.position);
        }
    }

    protected override EnumProvider.ORDERSLIST on_UnitStateChange(EnumProvider.ORDERSLIST stateorder)
    {
        return stateorder;
    }

    void OnDestroy()
    {
        if (!this.gameObject.GetComponent<Pilot>())
            Component.Destroy(this.gameObject.GetComponent<SphereCollider>());
    }
}
