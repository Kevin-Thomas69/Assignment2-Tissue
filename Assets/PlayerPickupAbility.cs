using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using UnityEngine;

public interface ICanPickup
{
    void Pickup(PlayerPickupAbility picker);
    bool PickupCompleted{get;} 
}

public class PlayerPickupAbility : MonoBehaviour
{ 
    ICanPickup current;
    public Transform hand;
    void OnOperate()
    {
        if (current !=null)
        {
            return;
        }
        if(list.Count ==0)
        {
            return ;
        }
        current =list[0];
        list.RemoveAt(0);
        current.Pickup (this);
    }
    private void Update()
    {
        if(current !=null && current.PickupCompleted)
        {
            current =null;
        }
    }
   List <ICanPickup >list =new List<ICanPickup> ();
   private void OnTriggerEnter(Collider other)
   {
    if(other.gameObject.tag =="Box")
    {
        var Pickupobject =other.gameObject.GetComponent <ICanPickup >();
        if(Pickupobject != null && !list.Contains(Pickupobject ))
        {
            if(!Pickupobject.PickupCompleted )
            {
                list.Add(Pickupobject );
            }
        }
    }
   
   }
   private void OnTriggerExit(Collider other)
   {
       if(other.gameObject.tag =="Box")
       {
        var Pickupobject = other.gameObject.GetComponent <ICanPickup >();
        if(Pickupobject != null&& list.Contains(Pickupobject ))
        {
         list.Remove (Pickupobject );
        }
       }   
   }
}

