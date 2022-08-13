using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Bed : MonoBehaviour
{
    [SerializeField] private GameObject _progressBar;
    [SerializeField] private GameObject _progressBarFill;
    [SerializeField] private Image _blackout;

    private GameObject _player;
    private bool _isOpening = false;

    private void FixedUpdate()
    {
        if(_isOpening)
            _progressBarFill.GetComponent<Image>().fillAmount += Time.deltaTime;
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.GetComponent<PlayerController>() && !other.GetComponent<PlayerController>().GetMovingStatus() && !_isOpening)
        {
            _isOpening = true;
            _progressBar.SetActive(true);
            _player = other.gameObject;
            StartCoroutine(OpenWait());
        }
        if(General.Instance.Player.GetComponent<PlayerController>().GetMovingStatus())
        {
            _isOpening = false;
            _progressBar.SetActive(false);
            _progressBarFill.GetComponent<Image>().fillAmount = 0;
            StopAllCoroutines();
        }
    }

    private IEnumerator OpenWait()
    {
        yield return new WaitForSeconds(1f);

        _progressBar.SetActive(false);
        StartCoroutine(SleepOnBed());
    }

    private IEnumerator SleepOnBed()
    {
        Vector3 _startPos = _player.transform.position;
        _player.GetComponent<PlayerController>().StopAction();
        _player.GetComponent<Animator>().SetInteger("state", 4);
        _player.GetComponent<Animator>().SetFloat("direction", -1);
        _player.transform.position = transform.position + new Vector3(-0.3f, 0.3f, -0.3f);
        _player.transform.eulerAngles = new Vector3(0, 270, 0);

        //Sleep anim

        yield return new WaitForSeconds(0.5f);

        _blackout.gameObject.SetActive(true);
        _blackout.DOColor(Color.black, 0.5f);

        yield return new WaitForSeconds(0.5f);

        _player.GetComponent<PlayerHealth>().HealPlayer();

        yield return new WaitForSeconds(0.5f);

        _blackout.DOColor(new Color(0, 0, 0, 0), 0.5f);
        _player.GetComponent<Animator>().SetInteger("state", 4);
        _player.GetComponent<Animator>().SetFloat("direction", 1);

        yield return new WaitForSeconds(0.5f);

        _player.transform.position = _startPos;
        _blackout.gameObject.SetActive(false);
        _player.GetComponent<Animator>().SetInteger("state", 0);
        _player.GetComponent<PlayerController>().ResumeAction();
    }
}