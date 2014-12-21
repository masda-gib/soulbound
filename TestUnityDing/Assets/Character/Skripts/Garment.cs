using UnityEngine;
using System.Collections;

public class Garment : MonoBehaviour, IPersonContainer, ICharacterController
{
    private Person currPilot;

    #region IPersonContainer implementation
    public bool RequestPilotEntering (IPerson pilot)
    {
        if (currPilot == null && pilot != null) 
        {
            currPilot = (Person)pilot;
            currPilot.CurrentContainer = this;

            this.transform.parent = currPilot.transform;
            this.transform.localPosition = Vector3.zero;
            this.transform.localRotation = Quaternion.identity;
            return true;
        }
        return false;
    }
    public bool EjectPilot ()
    {
        currPilot.CurrentContainer = null;
        currPilot = null;

        this.transform.parent = null;
        return true;
    }
    public IPerson CurrentPilot 
    {
        get { return currPilot; }
    }
    #endregion

    #region ICharacterController implementation

    public void EnterLeave ()
    {
        EjectPilot();
    }

    public void Look (Vector2 dirChange)
    {
        if (currPilot != null) 
        {
            currPilot.Look(dirChange);
        }
    }

    public void MoveForward (float amount)
    {
        if (currPilot != null) 
        {
            currPilot.MoveForward(amount);
        }
    }

    public void MoveToSide (float amount)
    {
        if (currPilot != null) 
        {
            currPilot.MoveToSide(amount);
        }
    }

    public void SelectUp ()
    {
        throw new System.NotImplementedException ();
    }

    public void SelectDown ()
    {
        throw new System.NotImplementedException ();
    }

    #endregion

    #region IActionController implementation

    public void BeginJump ()
    {
        if (currPilot != null) 
        {
            currPilot.BeginJump();
        }
    }

    public void ContinueJump (bool doContinue)
    {
        throw new System.NotImplementedException ();
    }

    public void BeginDuck ()
    {
        throw new System.NotImplementedException ();
    }

    public void ContinueDuck (bool doContinue)
    {
        throw new System.NotImplementedException ();
    }

    public void BeginUse ()
    {
        throw new System.NotImplementedException ();
    }

    public void ContinueUse (bool doContinue)
    {
        throw new System.NotImplementedException ();
    }

    #endregion
}
