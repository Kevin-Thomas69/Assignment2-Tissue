/*using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.Playables;

public class PicupObjectOnTable : MonoBehaviour,ICanPickup 
{
   public bool PickupCompleted{get; private set;}
   public float PickTime =1.1f;
   public Transform Object;
   public Transform PlayerStandPosition;
   public void Pickup(PlayerPickupAbility picker)
   {
StartCoroutine (_Pickup(picker));
   }
   IEnumerator _Pickup(PlayerPickupAbility picker)
   {
    var thirdPersonController = picker.GetComponent <ThirdPersonController >();
    thirdPersonController.IsStopMove = true;
    var animator = picker.GetComponent <Animator>();
    var director = GetComponent <PlayableDirector >();

foreach(var output in director.playableAsset .outputs )
{
if(output.streamName == "Player Track")
{
director.SetGenericBinding(output.sourceObject,animator);
break;
}
   }
   director.Play();
   while (director.state == PlayState .Playing)
   {
    if(director.time >= PickTime && Object.parent !=picker.hand)
    {
        Object.SetParent (picker.hand);
        Object.localPosition =Vector3.zero;
        Object.localRotation = Quaternion.identity;
    }
    yield return null;
   }
   PickupCompleted = true;
   thirdPersonController.IsStopMove = false;
   Destroy(Object.gameObject);
}
}*/