using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingComponent : MonoBehaviour
{
    private Image imgLoading;

    private float rotateAmount = 0.02f;

    private float speed = 1;

    void Awake()
    {
        imgLoading = GetComponent<Image>();       
    }

    private void OnEnable() {
        StartCoroutine(RotateCoroutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
    }

    // Update is called once per frame
    public IEnumerator RotateCoroutine ()
    {
        while(gameObject.activeSelf){
            gameObject.transform.Rotate(0,0, speed);

            speed += rotateAmount;

            if(speed > 5f){
                speed = 1f;
            }

            // imgLoading.fillAmount += up ? rotateAmount : -rotateAmount;
            // if(imgLoading.fillAmount >= 1f){
            //     up = false;
            // }else if(imgLoading.fillAmount <= 0f){
            //     up = true;
            // }

            yield return null;
        }
    }
}
