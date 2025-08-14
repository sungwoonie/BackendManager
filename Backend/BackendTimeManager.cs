using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarCloudgamesLibrary;
using System;
using BackEnd;

public class BackendTimeManager : SingleTon<BackendTimeManager>
{
    private DateTime currentServerTime;
    private Coroutine timeTickCoroutine;

    private Action<DateTime> serverTimeChangedAction;
    private Action<DateTime> serverTimeDayChangedAction;

    #region "Unity"

    protected override void Awake()
    {
        base.Awake();

        InitializeEvent();
    }

    #endregion

    #region "Initialize"

    private void InitializeEvent()
    {
        ApplicationController.instance.AddAction(SystemActionType.Resume, () =>
        {
            InitializeTime();
        });
    }

    public void InitializeTime()
    {
        Backend.Utils.GetServerTime((callback) =>
        {
            string time = callback.GetReturnValuetoJSON()["utcTime"].ToString();
            currentServerTime = DateTime.Parse(time);

            if(timeTickCoroutine != null)
            {
                StopCoroutine(timeTickCoroutine);
            }

            timeTickCoroutine = StartCoroutine(ServerTimeTick());
        });
    }

    #endregion

    #region "Time"

    private IEnumerator ServerTimeTick()
    {
        while(true)
        {
            var prevDate = currentServerTime.Date;
            currentServerTime = currentServerTime.AddSeconds(1);

            if(currentServerTime.Date != prevDate)
            {
                NotifyServerTimeChanged(true);
            }

            NotifyServerTimeChanged(false);
            yield return Yielder.WaitForSeconds(1f);
        }
    }

    public DateTime GetCurrentServerTime()
    {
        return currentServerTime;
    }

    #endregion

    #region "Action"

    public void AddAction(Action<DateTime> action, bool dayChanged)
    {
        if(dayChanged)
        {
            serverTimeDayChangedAction += action;
        }
        else
        {
            serverTimeChangedAction += action;
        }
    }

    public void RemoveAction(Action<DateTime> action, bool dayChanged)
    {
        if(dayChanged)
        {
            serverTimeDayChangedAction -= action;

        }
        else
        {
            serverTimeChangedAction -= action;
        }
    }

    private void NotifyServerTimeChanged(bool dayChanged)
    {
        if(dayChanged)
        {
            serverTimeDayChangedAction?.Invoke(currentServerTime); 
        }
        else
        {
            serverTimeChangedAction?.Invoke(currentServerTime);
        }
    }

    #endregion
}