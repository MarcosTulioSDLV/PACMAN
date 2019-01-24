using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OnClick : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool click = false;
    private Vector3 originalSizeOfText;
    [SerializeField] private AudioSource audioSourceForSoundEffects;
    [SerializeField] private AudioClip audioButtonClick;
    [SerializeField] private AudioClip audioPointerEnter;

    public bool Click
    {
        get
        {
            return click;
        }

        set
        {
            click = value;
        }
    }

    void Start()
    {
        originalSizeOfText = transform.localScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Click = true;

        audioSourceForSoundEffects.clip = audioButtonClick;
        audioSourceForSoundEffects.Play();

    }

    //CODIGO PARA MUDAR TAMANHO DO TEXTO, E ATIVAR SOM QUANDO O MOUSE ESTIVER ACIMA DELE
    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalSizeOfText + new Vector3(0.4f, 0.4f, 0.4f);

        audioSourceForSoundEffects.clip = audioPointerEnter;
        audioSourceForSoundEffects.Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalSizeOfText;
    }
}
