/*
Copyright (c) 2024 hotaru86
Released under the MIT license
https://opensource.org/licenses/mit-license.php
*/

using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class JoinManager : UdonSharpBehaviour
{
    //参加者のIDを保持する配列
    [UdonSynced] public int[] playerIDs = new int[16];
    //現在ワールドにいる人数を保持する変数
    [UdonSynced] public int playerNum = 0;
    //JoinDeviceをワールド上限人数分格納しておく
    [SerializeField] public JoinDevice[] joinDevices;
    private void Start()
    {
        DisplayJoinedPlayerList();
        DisplayDebugText();
    }

    //誰かがJoinしたら、JoinDeviceを1つアタッチする
    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        //JoinManagerのOwnerのみが処理を行う
        if (!Networking.IsOwner(gameObject)) return;
        //空きJoinDeviceを検索
        var jd = GetJoinDeviceById(0);
        if (jd == null) return;
        jd.Attach(player.playerId);
    }

    //誰かがインスタンスを離れたら、JoinDeviceをデタッチする
    public override void OnPlayerLeft(VRCPlayerApi player)
    {
        //JoinManagerのOwnerのみが処理を行う
        if (!Networking.IsOwner(gameObject)) return;
        var jd = GetJoinDeviceById(player.playerId);
        if (jd == null) return;
        jd.Detach();
    }

    //いずれかのJoinDeviceのuserIDが変更されたら呼ばれる
    public void OnSomeoneUserIDChanged(int IDAfterChanged)
    {
        DisplayDebugText();
    }

    //いずれかのJoinDeviceのisJoinが変更されたら呼ばれる
    public void OnSomeoneIsJoinedChanged(int IDAfterChanged)
    {
        if (Networking.IsOwner(gameObject))
        {
            //Ownerのみが処理
            //playerIDsを更新
            RefreshPlayerIDs();
            
            //Owner以外はOnDeserializationで以下の処理を行う
            DisplayJoinedPlayerList();
            DisplayDebugText();
        }
    }

    //指定したIDをuserIDに持つJoinDeviceを検索して返す
    public JoinDevice GetJoinDeviceById(int id)
    {
        foreach (JoinDevice jd in joinDevices)
        {
            if (jd.userID == id)
            {
                return jd;
            }
        }
        return null;
    }

    //自分のplayerIDのplayerIDs内でのインデックスを取得
    public int GetMyIndex()
    {
        int myID = Networking.LocalPlayer.playerId;
        for (int i = 0; i < playerIDs.Length; i++)
        {
            if (playerIDs[i] == myID)
            {
                return i;
            }
        }
        return -1;
    }

    
    public override void OnDeserialization()
    {
        DisplayJoinedPlayerList();
        DisplayDebugText();
    }

    //デバッグ用
    [SerializeField] Text joinedPlayerListText;
    public void RefreshPlayerIDs()
    {
        //直前の参加者リストを見て、抜けた人を探す
        for (int i = 0; i < playerIDs.Length; i++)
        {
            if (playerIDs[i] == 0) continue;
            //参加済みプレイヤーのJDを検索
            var tmp = GetJoinDeviceById(playerIDs[i]);
            if (tmp == null) continue;
            //参加しなくなってたら
            if (!tmp.isJoin)
            {
                //参加者リストから抜いて0にする
                playerIDs[i] = 0;
            }
        }
        //新しく参加した人を探す
        foreach (JoinDevice jd in joinDevices)
        {
            //参加してる人を探す
            if (jd.isJoin)
            {
                //直前の参加者リストにもう入ってるかを確認
                bool alreadyJoined = false;
                for (int i = 0; i < playerIDs.Length; i++)
                {
                    if (jd.userID == playerIDs[i])
                    {
                        alreadyJoined = true;
                        break;
                    }
                }
                //参加希望してるのにまだ入ってない
                if (!alreadyJoined)
                {
                    //空きがあったら入れる
                    for (int i = 0; i < playerIDs.Length; i++)
                    {
                        if (playerIDs[i] == 0)
                        {
                            playerIDs[i] = jd.userID;
                            break;
                        }
                    }
                }
            }
        }
        //playerNumを更新
        playerNum = 0;
        for (int i = 0; i < playerIDs.Length; i++)
        {
            if (playerIDs[i] != 0)
            {
                playerNum++;
            }
        }
        RequestSerialization();
    }

    public void DisplayJoinedPlayerList()
    {
        if(joinedPlayerListText == null) return;
        string str = "JoinedPlayers\n";
        for (int i = 0; i < playerIDs.Length; i++)
        {
            int id = playerIDs[i];
            if (id == 0) continue;
            str += $"{id}\t{VRCPlayerApi.GetPlayerById(id).displayName}\n";
        }
        joinedPlayerListText.text = str;
    }

    //デバッグ用テキスト表示
    [SerializeField] Text debugText;
    public void DisplayDebugText()
    {
        if (debugText == null) return;
        string str = "";
        str += "playerIDs\ns";
        foreach (int id in playerIDs)
        {
            str += $"{id}\t";
        }
        str += "\n";
        str += "\n";

        for (int i = 0; i < joinDevices.Length; i++)
        {
            str += $"{i}\t";
            str += $"{joinDevices[i].userID}\t";
            str += $"J:{joinDevices[i].isJoin}\t";
            var ownerPlayer = Networking.GetOwner(joinDevices[i].gameObject);
            str += $"{ownerPlayer.displayName}({ownerPlayer.playerId})";
            str += "\n";
        }


        debugText.text = str;
    }


}
