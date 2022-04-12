using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Serializable]
    public class ComboList
    {
       public int[] comboKeys;      
    }
   
    [Header("General")]
    public GameObject rHand;
    public GameObject lHand;
    public Transform rHandDefault;
    public Transform lHandDefault;
    public TrailRenderer[] trails;
    public TrailRenderer rightTrail;
    public TrailRenderer leftTrail;
    public CapsuleCollider col;
    public GameObject playerHitEffect;
    public float handAnimReturnSpeed;
    [HideInInspector]
    public bool isAttacking = false;
    [HideInInspector]
    public Vector3 sphereDefaultScale;
    [HideInInspector]
    public bool abilityActive = false;
    [HideInInspector]
    public float trailTimeDefault;

    [Header("Scripts")]
    public SphereTrigger[] triggers;
    public PlayerGetDamaged getDamaged;
    public SumoPushAbility sumoPush;
    public PlayerDashAbility dash;
    public RocketPunch rocketPunch;
    public TornadoAbility tornado;
  
    [Header("ComboKeys")]
    public List<ComboList> playerComboList;
    public List<int> inputs;
    int comboCount = 0;
    bool comboSuccessful = false; 
    public float comboCD;
    Coroutine comboCoroutine = null;
    bool comboActivated = false;
    int activable;

    [Header("BasicAttack")]
    public Transform rFirstEndPos;
    public Transform rFirstMidPos;
    public Transform lFirstEndPos;
    public Transform lFirstMidPos;
    public float firstAttackSpeed;
    public Vector3 firstTargetScale;
    public CombatAttributes basicAttackAtr;

    public ComboDisplayer cDisplayer;
    public bool dashDuringAbility;
    public GameObject abilityScriptsHolder;

    // Start is called before the first frame update
    void Start()
    {
        sphereDefaultScale = rHand.transform.localScale;
        trailTimeDefault = leftTrail.time;
        foreach  (PlayerAbility ability in abilityScriptsHolder.GetComponents<PlayerAbility>())
        {
            ability.InitializeAbilityScripts(trails, triggers, getDamaged, trailTimeDefault);
        }
    }

    public void Attack(int input)
    {      
        isAttacking = true;               
            switch (input)
            {
             case 0:
                abilityActive = true;
                AttackWithLeftHand();
                //Start the time for the player to input the next step of a combo & check if the player is doing the correct inputs for the combo
                StartComboCD(input);             
                break;
             case 1:
                abilityActive = true;
                //Start Attack Animation (right hand)
                AttackWithRightHand();
                StartComboCD(input);             
                break;
            case 2:
                if(activable == input)
                {
                    cDisplayer.CreateCombatInputUI(input);
                    cDisplayer.AnimateTextSize();
                    comboActivated = true;
                    sumoPush.ActivateAbility(rHand, lHand);
                    StartCoroutine(WaitTillAttackIsDone(sumoPush,true));                            
                }
                else { isAttacking = false; }
                break;
            case 3:
                if (activable == input)
                {
                    cDisplayer.CreateCombatInputUI(input);
                    cDisplayer.AnimateTextSize();
                    comboActivated = true;
                    abilityActive = true;
                    tornado.ActivateAbility(rHand, lHand);
                    StartCoroutine(WaitTillAttackIsDone(tornado, true));
                }
                else { isAttacking = false; }
                break;
            case 4:
                if (dash.canDash)
                {
                    dash.BeginDash(true);
                    StartCoroutine(WaitTillAttackIsDone(dash, dashDuringAbility));
                }
                else isAttacking = false;                           
                break;
            case 5:
                rocketPunch.ActivateAbility(rHand, lHand);
                StartCoroutine(WaitTillAttackIsDone(rocketPunch, true));
                abilityActive = true;
                break;
            }
            
        activable = 0;   
    }
   


    public void AttackWithLeftHand()
    {         
        StartCoroutine(PlayAttackAnimation(lHand, lHandDefault, leftTrail, lHand.transform.localPosition,
                lFirstMidPos.localPosition, lFirstEndPos.localPosition, firstAttackSpeed, false, firstTargetScale));
    }
    public void AttackWithRightHand()
    {     
       StartCoroutine(PlayAttackAnimation(rHand, rHandDefault, rightTrail, rHand.transform.localPosition,
                       rFirstMidPos.localPosition, rFirstEndPos.localPosition, firstAttackSpeed, false, firstTargetScale));
    }    
    
    IEnumerator CheckCombo(int input)
    {
        comboSuccessful = false;
        inputs.Add(input);
        for (int i = 0; i < playerComboList.Count; i++)
        {
            yield return null;        
            if (CheckComboListWithPlayerInputs(playerComboList[i].comboKeys))
            {            
                   comboSuccessful = true;
                   cDisplayer.CreateCombatInputUI(input);               
                   comboCount++;                     
                  if(comboCount == playerComboList[i].comboKeys.Length - 1)
                    {
                      activable = playerComboList[i].comboKeys[comboCount];
                    }
                  break;           
            }
        }
        if (!comboSuccessful)
        {
            comboCount = 0;
            cDisplayer.ResetDisplay();
            inputs.Clear();
        }
    }
     
    public bool CheckComboListWithPlayerInputs(int [] comboKeys)
    {
        for (int i = 0; i < inputs.Count; i++)
        {
            if (i >= comboKeys.Length)
            {
                return false;
            }

            if (inputs[i] != comboKeys[i])
            {
                return false;
            }           
        }
        return true;
    }

   
    IEnumerator ComboCooldwon(float cd)
    {
        for (float i = 0; i < cd; i+= Time.deltaTime)
        {
            yield return null;
        }
        comboCount = 0;     
        activable = 0;
        if (!comboActivated)
        {
            inputs.Clear();
            cDisplayer.ResetDisplay();
        }
           
    }

    public void StartComboCD(int input)
    {
        if (comboCoroutine != null)
            StopCoroutine(comboCoroutine);

        comboCoroutine = StartCoroutine(ComboCooldwon(comboCD));

       StartCoroutine(CheckCombo(input));
    }

   
    IEnumerator PlayAttackAnimation(GameObject hand, Transform handDefault, TrailRenderer trail, Vector3 startPos,
       Vector3 midPos, Vector3 endPos, float speed, bool scale, Vector3 targetScale)
    {
        float journey = 0f;
        trail.enabled = true;
        hand.GetComponent<SphereTrigger>().EnableSphereDamage(basicAttackAtr);
        Vector3 startScale = hand.transform.localScale;
        while (journey < 1f)
        {
            yield return null;
            if (hand.GetComponent<SphereTrigger>().hasHitEnemy)
            {
                GameObject effect = ObjectPool.Spawn(playerHitEffect);
                effect.transform.position = hand.transform.position;
                effect.SetActive(true);
                break;
            }
            journey += 1f * Time.deltaTime * speed;
            MyAnimation.LerpThreePointsLocal(hand.transform, startPos, midPos, endPos, journey);
            if (scale)
                hand.transform.localScale = Vector3.Lerp(startScale, targetScale, journey);
        }
        trail.enabled = false;
        hand.GetComponent<SphereTrigger>().DisableSphereDamage();
        StartCoroutine(MoveHandToDefaultPosition(hand, handDefault.localPosition, handAnimReturnSpeed, false, scale, false));
    }

    IEnumerator WaitTillAttackIsDone(PlayerAbility ability, bool affectCombo)
    {
        while (!ability.isFinished)
        {
            yield return null;
        }
       
        StartCoroutine(MoveHandToDefaultPosition(rHand,rHandDefault.localPosition,handAnimReturnSpeed,true,true, affectCombo));
    }
    IEnumerator MoveHandToDefaultPosition(GameObject hand, Vector3 endPos, float speed, bool bothHands, bool scale, bool affectCombo)
    {
        float journey = 0f;
        Vector3 rStartPos, lStartPos, startPos, startScale, rStartScale, lStartScale;
        rStartPos = rHand.transform.localPosition;
        lStartPos = lHand.transform.localPosition;
        startPos = hand.transform.localPosition;
        startScale = hand.transform.localScale;
        rStartScale = rHand.transform.localScale;
        lStartScale = lHand.transform.localScale;

        while (journey < 1f)
        {
            yield return null;
            journey += 1f * Time.deltaTime * speed;

            if (bothHands)
            {
                rHand.transform.localPosition = Vector3.Lerp(rStartPos, rHandDefault.localPosition, journey);
                lHand.transform.localPosition = Vector3.Lerp(lStartPos, lHandDefault.localPosition, journey);
                if (scale)
                {
                    rHand.transform.localScale = Vector3.Lerp(rStartScale, sphereDefaultScale, journey);
                    lHand.transform.localScale = Vector3.Lerp(lStartScale, sphereDefaultScale, journey);
                }

            }
            else
            {
                hand.transform.localPosition = Vector3.Lerp(startPos, endPos, journey);
                if (scale)
                    hand.transform.localScale = Vector3.Lerp(startScale, sphereDefaultScale, journey);
            }

        }

        if (affectCombo)
        {
            cDisplayer.ResetDisplay();
            inputs.Clear();
            comboActivated = false;
            comboCount = 0;
            activable = 0;
        }
        isAttacking = false;
        abilityActive = false;
    }

}
