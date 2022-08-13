using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Tutorial : MonoBehaviour
{
    public static Tutorial instance;

    [SerializeField] private GameObject[] _tutorialInteractions;
    [SerializeField] private GameObject _arrow;
    [SerializeField] private GameObject _arrowPosition;
    [SerializeField] private GameObject _craftHand;
    [SerializeField] private GameObject _craftHandArmor;

    private bool _targetClose = false;
    private int _tutorialPhase = 0;

    private GameObject _target;

    private float _lowerAmount;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        _tutorialPhase = Data.Instance.TutorialStep;
        if(_tutorialPhase > 0)
            _tutorialPhase--;

        Invoke(nameof(CheckTutorial), 0.1f);
    }

    private void Update()
    {
        if(!GetTutorialActive() || !_arrow.activeInHierarchy || !_target)
            return;

        if(_tutorialPhase == 10)
            _arrowPosition.SetActive(Vector3.Distance(General.Instance.Player.transform.position, _target.transform.position) <= 10f);
        else
            _arrowPosition.SetActive(true);

        if(Vector3.Distance(General.Instance.Player.transform.position, _target.transform.position) < 2f && !_targetClose)
        {
            _targetClose = true;

            _arrowPosition.transform.localEulerAngles = new Vector3(180, -45, 0);
            _arrow.transform.eulerAngles = Vector3.zero;

            if(_tutorialPhase == 4 || _tutorialPhase == 7)
            {
                _arrow.transform.DOMove(_target.transform.position + new Vector3(0, 8, -1) - new Vector3(0, _lowerAmount, 0), 0.3f);
            }
            else
                _arrow.transform.DOMove(_target.transform.position + new Vector3(0, 2, -1), 0.3f);
        }
        else if(Vector3.Distance(General.Instance.Player.transform.position, _target.transform.position) >= 2f)
        {
            _arrow.transform.position = General.Instance.Player.transform.position + Vector3.up;

            var lookPos = _target.transform.position - General.Instance.Player.transform.position;
            lookPos.y = 0;
            _arrow.transform.rotation = Quaternion.LookRotation(lookPos);

            if(_targetClose)
            {
                _targetClose = false;
                _arrowPosition.transform.localEulerAngles = new Vector3(90, 0, 0);
            }
        }
    }

    private void CheckTutorial()
    {
        if(GetTutorialActive())
        {
            _arrow.SetActive(true);
            ProgressTutorial();
        }
    }

    public void ProgressTutorial(bool _canProgressFinal = false)
    {
        _lowerAmount = 0;

        if(_tutorialPhase < _tutorialInteractions.Length)
        {
            _target = _tutorialInteractions[_tutorialPhase];
            _tutorialPhase++;
        }
        else
        {
            _tutorialPhase++;
            _arrow.SetActive(false);
        }

        if(_tutorialPhase == 6 || _tutorialPhase == 9)
        {
            _craftHand.SetActive(true);
            _craftHand.transform.localScale = Vector3.one;
            _craftHand.transform.DOScale(1.5f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            _craftHand.transform.DOKill();
            _craftHand.SetActive(false);
        }

        if(_tutorialPhase == 11)
        {
            _craftHandArmor.SetActive(true);
            _craftHandArmor.transform.DOScale(1.5f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }
        else
            _craftHandArmor.SetActive(false);

        _targetClose = false;
        _arrowPosition.transform.localEulerAngles = new Vector3(90, 0, 0);

        Data.Instance.TutorialStep = _tutorialPhase;
    }

    public void LowerArrow()
    {
        _lowerAmount += 0.7f;
        _arrow.transform.DOMove(_target.transform.position + new Vector3(0, 8, -1) - new Vector3(0, _lowerAmount, 0), 0.3f);
    }

    public int GetPhase()
    {
        return _tutorialPhase;
    }

    public bool GetTutorialActive()
    {
        return _tutorialPhase <= _tutorialInteractions.Length;
    }
}