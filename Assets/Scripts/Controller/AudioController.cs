using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioController : Photon.MonoBehaviour
{
    //private Transform myTran;
    private AudioSource[] _audioSources;
    private AudioSource[] audioSources { get { return _audioSources != null ? _audioSources : _audioSources = transform.GetComponentsInChildren<AudioSource>(); } }

    //void Start()
    //{
    //    myTran = transform;
    //    audioSources = myTran.GetComponentsInChildren<AudioSource>();
    //    foreach (AudioSource audioSource in audioSources)
    //    {
    //        Debug.Log(name+" >> "+ audioSource.clip);
    //    }
    //}

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
        Play(no, false);
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
