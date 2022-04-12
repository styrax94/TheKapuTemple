using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class SumoPushAbility : PlayerAbility
{
    //Transforms to use for the abilities animation
    [Header("Animation Transforms")]
    public Transform rAnimPreT;
    public Transform lAnimPreT;
    public Transform rAnimMidT;
    public Transform lAnimMidT;
    public Transform rAnimEndT;
    public Transform lAnimEndT;
      
    //Variables that will make affect the animation, like changing the scale of the objects or speed of the animation
    [Header("Animation Variables")]
    public float animChargeSpeed;
    public float animAttackSpeed;
    public Vector3 targetScale;
    public float trailTime; //alter the trail time of the spheres to improve visuals

    [Header("Dash Variables")]
    public float sumoDashDistance;
    public float sumoDashSpeed;
    public PlayerDashAbility dash; 
    
    public CombatAttributes comboSumoPushAtr;
    public BoxCollider boxColR;
    public BoxCollider boxColL;
   
  
    public void ActivateAbility(GameObject rHand, GameObject lHand)
    {
        isFinished = false;
        //Starts the coroutine that will handle the abilities animation and effects
        StartCoroutine(StartSumoPush(rHand,lHand,rAnimPreT.localPosition,lAnimPreT.localPosition, rAnimMidT.localPosition, lAnimMidT.localPosition, rAnimEndT.localPosition, lAnimEndT.localPosition));
    }

    IEnumerator StartSumoPush(GameObject rHand, GameObject lHand, Vector3 rPrePos, Vector3 lPrePos, Vector3 rMidPos, Vector3 lMidPos,
       Vector3 rEndPos, Vector3 lEndPos)
    {
        float journey = 0f;

        Vector3 rStartPos, lStartPos;
        rStartPos = rHand.transform.localPosition;
        lStartPos = lHand.transform.localPosition;

        Vector3 rStartScale, lStartScale;
        rStartScale = rHand.transform.localScale;
        lStartScale = lHand.transform.localScale;

        while (journey < 1f)
        {
            yield return null;

            if (!myDamaged.canMove)// Stops the coroutine from advancing with playing the animation due to the player getting hit
            {             
                isFinished = true;
                yield break;
            }
           
            journey += 1f * Time.deltaTime * animChargeSpeed;
            MyAnimation.LerpTwoTransformsLocal(rHand.transform, lHand.transform, rStartPos, rPrePos, lStartPos, lPrePos, journey);                   
            rHand.transform.localScale = Vector3.Lerp(rStartScale, targetScale, journey);
            lHand.transform.localScale = Vector3.Lerp(lStartScale, targetScale, journey);
            
        }
       
        ActivateTriggers(comboSumoPushAtr, true);
        EnableSumoPushCollider(rHand,lHand, true);
        dash.BeginDash(sumoDashDistance, sumoDashSpeed, false, 0f);
       
        StartCoroutine(PlayAttackAnimation(rHand, lHand, rMidPos, lMidPos, rEndPos, lEndPos, trailTime, true));
    }
    IEnumerator PlayAttackAnimation(GameObject rHand, GameObject lHand, Vector3 rMidPos, Vector3 lMidPos, Vector3 rEndPos, Vector3 lEndPos, float trailTime, bool scale)
    {
        float journey = 0f;
        Vector3 rStartPos, lStartPos;
        rStartPos = rHand.transform.localPosition;
        lStartPos = lHand.transform.localPosition;
        AlterTrails(true, trailTime);

        while (journey < 1f)
        {
            if (!myDamaged.canMove)// Stops the while loop from advancing with the animation
            {
                isFinished = true;
                break;
            }
            yield return null;
            journey += 1f * Time.deltaTime * animAttackSpeed;
            MyAnimation.LerpThreePointsLocal(rHand.transform, rStartPos, rMidPos, rEndPos, journey);
            MyAnimation.LerpThreePointsLocal(lHand.transform, lStartPos, lMidPos, lEndPos, journey);
            
        }
        AlterTrails(false, trailTimeDefault);
        ActivateTriggers(comboSumoPushAtr, false);      
        EnableSumoPushCollider(rHand, lHand, false);
        isFinished = true;
    }
    public void EnableSumoPushCollider(GameObject rHand, GameObject lHand, bool enable)
    {
        rHand.GetComponent<SphereCollider>().enabled = !enable;
        lHand.GetComponent<SphereCollider>().enabled = !enable;
        boxColR.enabled = enable;
        boxColL.enabled = enable;

    }
}
