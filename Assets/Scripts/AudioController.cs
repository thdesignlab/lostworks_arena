using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioController : Photon.MonoBehaviour
{
    private Transform myTran;
    private AudioSource[] audioSources;

    void Awake()
    {
        myTran = transform;
        audioSources = myTran.GetComponentsInChildren<AudioSource>();
        //Debug.Log("###: " + myTran.root.name);
        //foreach (AudioSource tmp in audioSources)
        //{
        //    Debug.Log(tmp.transform.name);
        //}
    }

    public void Play(int no = 0, bool isSendRPC = true)
    {
        if (!IsExistsAudio(no)) return;
        audioSources[no].Play();
        if (isSendRPC)
        {
            photonView.RPC("PlayRPC", PhotonTargets.Others, no);
        }
    }
    [PunRPC]
    private void PlayRPC(int no = 0)
    {
        audioSources[no].Play();
    }

    public void Stop(int no = 0, bool isSendRPC = true)
    {
        if (!IsExistsAudio(no)) return;
        audioSources[no].Stop();
        if (isSendRPC)
        {
            photonView.RPC("StopRPC", PhotonTargets.Others, no);
        }
    }
    [PunRPC]
    private void StopRPC(int no = 0)
    {
        audioSources[no].Stop();
    }

    private bool IsExistsAudio(int no)
    {
        if (audioSources != null && (0 < audioSources.Length && no + 1 <= audioSources.Length)) return true;
        return false;
    }
}
