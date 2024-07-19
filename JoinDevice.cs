/*
Copyright (c) 2024 hotaru86
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

//ワールド内のユーザは1人1つこのJoinDeviceがアタッチされたオブジェクトのOwnerを保持する
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class JoinDevice : UdonSharpBehaviour
{
    //このJoinDeviceを保持しているユーザのID
    //誰も保持していない場合は0
    [UdonSynced, FieldChangeCallback(nameof(userID))] int _userID = 0;
    
    //例えば、このユーザがゲームにJoinしているかどうかを保持する
    [UdonSynced, FieldChangeCallback(nameof(isJoin))] bool _isJoin = false;
    
    [SerializeField] JoinManager joinManager;

    public int userID
    {
        get => _userID;
        set
        {
            _userID = value;
            OnUserIDCganged();
        }
    }

    public bool isJoin
    {
        get => _isJoin;
        set
        {
            _isJoin = value;
            OnIsJoinChanged();
        }
    }


    //userIDが変更された時の処理
    public void OnUserIDCganged()
    {
        if (userID == Networking.LocalPlayer.playerId)
        {
            //userID本人のみのローカル処理
            //userIDが自分のIDだった場合、このJoinDeviceを割り当てられたということなので、
            //Ownerを取得する
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        //全員の各自ローカル処理
        joinManager.OnSomeoneUserIDChanged(userID);
    }

    //isJoinが変更された時の処理
    public void OnIsJoinChanged()
    {
        if (userID == Networking.LocalPlayer.playerId)
        {
            //userID本人のみのローカル処理
        }
        //全員の各自ローカル処理
        joinManager.OnSomeoneIsJoinedChanged(userID);
    }

    //このJoinDeviceを、playerIDがidの人に割り当てる処理
    //実際にOwnerが変更されるのは、OnUserIDChangedの中
    public void Attach(int id)
    {
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        userID = id;
        RequestSerialization();
    }

    //このJoinDeviceを割り当てから外す処理
    public void Detach()
    {
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        userID = 0;
        isJoin = false;
        RequestSerialization();
    }

    public void ToggleJoinState()
    {
        //このJoinDeviceが自分のものでない場合は何もしない
        if (userID != Networking.LocalPlayer.playerId) return;
        if (isJoin)
        {
            Leave();
        }
        else
        {
            Join();
        }
    }

    public void Join()
    {
        if (userID != Networking.LocalPlayer.playerId) return;
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        isJoin = true;
        RequestSerialization();
    }
    public void Leave()
    {
        if (userID != Networking.LocalPlayer.playerId) return;
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        isJoin = false;
        RequestSerialization();
    }
}