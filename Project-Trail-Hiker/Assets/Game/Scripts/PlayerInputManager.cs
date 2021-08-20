﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    public bool aWasPressed = false;
    public bool dWasPressed = false;

    public float aTime = 0f;
    public float dTime = 0f;

    public bool fall = false;
    
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (!aWasPressed)
            {
                aWasPressed = true;
                aTime = Time.time;
            }
            else
            {
                aWasPressed = false;
                fall = true;
            }
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (!dWasPressed)
            {
                dWasPressed = true;
                dTime = Time.time;
            }
            else
            {
                dWasPressed = false;
                fall = true;
            }
        }
    }

    public bool IsJumpButtonDown()
    {
        return Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow);
    }

    public bool IsJumpButtonReleased()
    {
        return Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow);
    }

    public bool IsCrouchButtonDown()
    {
        return Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow);
    }

    public bool IsCrouchButtonReleased()
    {
        return Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow);
    }

    public bool IsSwipeDirectionButtonDown()
    {
        return Input.GetKeyDown(KeyCode.Q);
    }
}
