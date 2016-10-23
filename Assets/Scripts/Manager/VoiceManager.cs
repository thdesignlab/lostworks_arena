using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class VoiceManager : MonoBehaviour
{
    private AudioSource audioSource;

    [SerializeField]
    private List<AudioClip> attackVoiceList;
    [SerializeField]
    private List<AudioClip> extraAttackVoiceList;
    [SerializeField]
    private List<AudioClip> damageVoiceList;
    [SerializeField]
    private List<AudioClip> deadVoiceList;
    [SerializeField]
    private List<AudioClip> winVoiceList;
    [SerializeField]
    private List<AudioClip> battleStartVoiceList;

    private int nowStatus = 0;
    const int STATUS_ATTACK = 1;
    const int STATUS_EXTRA_ATTACK = 2;
    const int STATUS_DAMAGE = 3;
    const int STATUS_DEAD = 4;
    const int STATUS_WIN = 5;
    const int STATUS_START = 6;

    void Awake()
    {
        audioSource = transform.GetComponent<AudioSource>();
    }

    private void Play(List<AudioClip> clips, int no, int status)
    {
        if (audioSource == null || clips.Count == 0) return;
        if (audioSource.isPlaying)
        {
            if (nowStatus == STATUS_DEAD || nowStatus == STATUS_WIN) return;
            if (nowStatus == status) return;
        }
        nowStatus = status;

        int index = no;
        if (no < 0) index = Random.Range(0, clips.Count);
        audioSource.clip = clips[index];
        audioSource.Play();
    }

    public void Attack(int no = -1)
    {
        Play(attackVoiceList, no, STATUS_ATTACK);
    }
    public void ExtraAttack(int no = -1)
    {
        Play(extraAttackVoiceList, no, STATUS_EXTRA_ATTACK);
    }
    public void Damage(int no = -1)
    {
        Play(damageVoiceList, no, STATUS_DAMAGE);
    }
    public void Win(int no = -1)
    {
        Play(winVoiceList, no, STATUS_WIN);
    }
    public void BattleStart(int no = -1)
    {
        Play(battleStartVoiceList, no, STATUS_START);
    }
    public void Dead(int no = -1)
    {
        if (deadVoiceList.Count == 0) return;
        if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
        AudioSource camAudio = Camera.main.transform.GetComponent<AudioSource>();
        if (camAudio != null)
        {
            int index = Random.Range(0, deadVoiceList.Count);
            camAudio.clip = deadVoiceList[index];
            camAudio.Play();
        }
    }
}
